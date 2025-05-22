using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace MetroUnlocker.LibTSForge.SPP
{

    public enum ProductKeyAlgorithm
    {
        ProductKey2005,
        ProductKey2009
    }

    public class KeyRange
    {
        public int Start;
        public int End;
        public string EulaType;
        public string PartNumber;
        public bool Valid;

        public bool Contains(int n) { return Start <= n && End <= n; }
    }

    public class ProductConfig
    {
        public int GroupId;
        public string Edition;
        public string Description;
        public string Channel;
        public bool Randomized;
        public ProductKeyAlgorithm Algorithm;
        public List<KeyRange> Ranges;
        public Guid ActivationId;

        private List<KeyRange> GetProductKeyRanges()
        {
            if (Ranges.Count == 0)
                throw new ArgumentException("No key ranges.");

            if (Algorithm == ProductKeyAlgorithm.ProductKey2005)
                return Ranges;

            List<KeyRange> FilteredRanges = Ranges.FindAll(r => !r.EulaType.Contains("WAU"));

            if (FilteredRanges.Count == 0)
                throw new NotSupportedException("Specified Activation ID is usable only for Windows Anytime Upgrade. Please use a non-WAU Activation ID instead.");
            
            return FilteredRanges;
        }

        public ProductKey GetRandomKey()
        {
            List<KeyRange> KeyRanges = GetProductKeyRanges();
            Random random = new Random();

            KeyRange range = KeyRanges[random.Next(KeyRanges.Count)];
            int serial = random.Next(range.Start, range.End);

            return new ProductKey(serial, 0, false, Algorithm, this, range);
        }
    }

    public class ProductKeyConfig
    {
        public Dictionary<Guid, ProductConfig> Products = new Dictionary<Guid, ProductConfig>();
        private List<Guid> _loadedProductKeyConfigs = new List<Guid>();

        public void LoadConfig(Guid actId)
        {
            string pkcData;
            Guid configFileId = SLApi.GetProductKeyConfigFileId(actId);

            if (configFileId == Guid.Empty) throw new Exception("This edition of Windows does not support sideloading keys.");

            if (_loadedProductKeyConfigs.Contains(configFileId)) return;

            string licenseContents = SLApi.GetLicenseContents(configFileId);


            using (TextReader tr = new StringReader(licenseContents))
            {
                XmlDocument lic = new XmlDocument();
                lic.Load(tr);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(lic.NameTable);
                nsmgr.AddNamespace("rg", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
                nsmgr.AddNamespace("random", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
                nsmgr.AddNamespace("tm", "http://www.microsoft.com/DRM/XrML2/TM/v2");

                XmlNode root = lic.DocumentElement;
                XmlNode pkcDataNode = root.SelectSingleNode("/rg:licenseGroup/random:license/random:otherInfo/tm:infoTables/tm:infoList/tm:infoBin[@name=\"pkeyConfigData\"]", nsmgr);
                pkcData = Encoding.UTF8.GetString(Convert.FromBase64String(pkcDataNode.InnerText));
            }

            using (TextReader tr = new StringReader(pkcData))
            {
                XmlDocument lic = new XmlDocument();
                lic.Load(tr);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(lic.NameTable);
                nsmgr.AddNamespace("p", "http://www.microsoft.com/DRM/PKEY/Configuration/2.0");
                XmlNodeList configNodes = lic.SelectNodes("//p:ProductKeyConfiguration/p:Configurations/p:Configuration", nsmgr);
                XmlNodeList rangeNodes = lic.SelectNodes("//p:ProductKeyConfiguration/p:KeyRanges/p:KeyRange", nsmgr);
                XmlNodeList pubKeyNodes = lic.SelectNodes("//p:ProductKeyConfiguration/p:PublicKeys/p:PublicKey", nsmgr);

                Dictionary<int, ProductKeyAlgorithm> algorithms = new Dictionary<int, ProductKeyAlgorithm>();
                Dictionary<string, List<KeyRange>> ranges = new Dictionary<string, List<KeyRange>>();

                Dictionary<string, ProductKeyAlgorithm> algoConv = new Dictionary<string, ProductKeyAlgorithm>
                {
                    { "msft:rm/algorithm/pkey/2005", ProductKeyAlgorithm.ProductKey2005 },
                    { "msft:rm/algorithm/pkey/2009", ProductKeyAlgorithm.ProductKey2009 }
                };

                foreach (XmlNode pubKeyNode in pubKeyNodes)
                {
                    int group = int.Parse(pubKeyNode.SelectSingleNode("./p:GroupId", nsmgr).InnerText);
                    algorithms[group] = algoConv[pubKeyNode.SelectSingleNode("./p:AlgorithmId", nsmgr).InnerText];
                }

                foreach (XmlNode rangeNode in rangeNodes)
                {
                    string refActIdString = rangeNode.SelectSingleNode("./p:RefActConfigId", nsmgr).InnerText;

                    if (!ranges.ContainsKey(refActIdString))
                        ranges[refActIdString] = new List<KeyRange>();

                    KeyRange keyRange = new KeyRange();
                    keyRange.Start = int.Parse(rangeNode.SelectSingleNode("./p:Start", nsmgr).InnerText);
                    keyRange.End = int.Parse(rangeNode.SelectSingleNode("./p:End", nsmgr).InnerText);
                    keyRange.EulaType = rangeNode.SelectSingleNode("./p:EulaType", nsmgr).InnerText;
                    keyRange.PartNumber = rangeNode.SelectSingleNode("./p:PartNumber", nsmgr).InnerText;
                    keyRange.Valid = rangeNode.SelectSingleNode("./p:IsValid", nsmgr).InnerText.ToLower() == "true";

                    ranges[refActIdString].Add(keyRange);
                }

                foreach (XmlNode configNode in configNodes)
                {
                    string refActIdString = configNode.SelectSingleNode("./p:ActConfigId", nsmgr).InnerText;
                    Guid refActId = new Guid(refActIdString);
                    int group = int.Parse(configNode.SelectSingleNode("./p:RefGroupId", nsmgr).InnerText);
                    List<KeyRange> keyRanges = ranges[refActIdString];

                    if (keyRanges.Count > 0 && !Products.ContainsKey(refActId))
                    {
                        ProductConfig productConfig = new ProductConfig();
                        productConfig.GroupId = group;
                        productConfig.Edition = configNode.SelectSingleNode("./p:EditionId", nsmgr).InnerText;
                        productConfig.Description = configNode.SelectSingleNode("./p:ProductDescription", nsmgr).InnerText;
                        productConfig.Channel = configNode.SelectSingleNode("./p:ProductKeyType", nsmgr).InnerText;
                        productConfig.Randomized = configNode.SelectSingleNode("./p:ProductKeyType", nsmgr).InnerText.ToLower() == "true";
                        productConfig.Algorithm = algorithms[group];
                        productConfig.Ranges = keyRanges;
                        productConfig.ActivationId = refActId;

                        Products[refActId] = productConfig;
                    }
                }
            }

            _loadedProductKeyConfigs.Add(configFileId);
        }

        public ProductConfig MatchParams(int groupId, int serial)
        {
            ProductConfig matchingConfig = Products.Values.FirstOrDefault(config => config.GroupId == groupId && config.Ranges.Any(range => range.Contains(serial)));
            if (matchingConfig == null) throw new FileNotFoundException("Failed to find product matching supplied product key parameters.");
            return matchingConfig;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace MetroUnlocker.LibTSForge.SPP
{

    public enum PKeyAlgorithm
    {
        PKEY2005,
        PKEY2009
    }

    public class KeyRange
    {
        public int Start;
        public int End;
        public string EulaType;
        public string PartNumber;
        public bool Valid;

        public bool Contains(int n)
        {
            return Start <= n && End <= n;
        }
    }

    public class ProductConfig
    {
        public int GroupId;
        public string Edition;
        public string Description;
        public string Channel;
        public bool Randomized;
        public PKeyAlgorithm Algorithm;
        public List<KeyRange> Ranges;
        public Guid ActivationId;

        private List<KeyRange> GetPkeyRanges()
        {
            if (Ranges.Count == 0)
            {
                throw new ArgumentException("No key ranges.");
            }

            if (Algorithm == PKeyAlgorithm.PKEY2005)
            {
                return Ranges;
            }

            List<KeyRange> FilteredRanges = Ranges.Where(r => !r.EulaType.Contains("WAU")).ToList();

            if (FilteredRanges.Count == 0)
            {
                throw new NotSupportedException("Specified Activation ID is usable only for Windows Anytime Upgrade. Please use a non-WAU Activation ID instead.");
            }

            return FilteredRanges;
        }

        public ProductKey GetRandomKey()
        {
            List<KeyRange> KeyRanges = GetPkeyRanges();
            Random rnd = new Random();

            KeyRange range = KeyRanges[rnd.Next(KeyRanges.Count)];
            int serial = rnd.Next(range.Start, range.End);

            return new ProductKey(serial, 0, false, Algorithm, this, range);
        }
    }

    public class PKeyConfig
    {
        public Dictionary<Guid, ProductConfig> Products = new Dictionary<Guid, ProductConfig>();
        private List<Guid> loadedPkeyConfigs = new List<Guid>();

        public void LoadConfig(Guid actId)
        {
            string pkcData;
            Guid pkcFileId = SLApi.GetPkeyConfigFileId(actId);

            if (loadedPkeyConfigs.Contains(pkcFileId)) return;

            string licConts = SLApi.GetLicenseContents(pkcFileId);

            using (TextReader tr = new StringReader(licConts))
            {
                XmlDocument lic = new XmlDocument();
                lic.Load(tr);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(lic.NameTable);
                nsmgr.AddNamespace("rg", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
                nsmgr.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
                nsmgr.AddNamespace("tm", "http://www.microsoft.com/DRM/XrML2/TM/v2");

                XmlNode root = lic.DocumentElement;
                XmlNode pkcDataNode = root.SelectSingleNode("/rg:licenseGroup/r:license/r:otherInfo/tm:infoTables/tm:infoList/tm:infoBin[@name=\"pkeyConfigData\"]", nsmgr);
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

                Dictionary<int, PKeyAlgorithm> algorithms = new Dictionary<int, PKeyAlgorithm>();
                Dictionary<string, List<KeyRange>> ranges = new Dictionary<string, List<KeyRange>>();

                Dictionary<string, PKeyAlgorithm> algoConv = new Dictionary<string, PKeyAlgorithm>
                {
                    { "msft:rm/algorithm/pkey/2005", PKeyAlgorithm.PKEY2005 },
                    { "msft:rm/algorithm/pkey/2009", PKeyAlgorithm.PKEY2009 }
                };

                foreach (XmlNode pubKeyNode in pubKeyNodes)
                {
                    int group = int.Parse(pubKeyNode.SelectSingleNode("./p:GroupId", nsmgr).InnerText);
                    algorithms[group] = algoConv[pubKeyNode.SelectSingleNode("./p:AlgorithmId", nsmgr).InnerText];
                }

                foreach (XmlNode rangeNode in rangeNodes)
                {
                    string refActIdStr = rangeNode.SelectSingleNode("./p:RefActConfigId", nsmgr).InnerText;

                    if (!ranges.ContainsKey(refActIdStr))
                    {
                        ranges[refActIdStr] = new List<KeyRange>();
                    }

                    KeyRange keyRange = new KeyRange();
                    keyRange.Start = int.Parse(rangeNode.SelectSingleNode("./p:Start", nsmgr).InnerText);
                    keyRange.End = int.Parse(rangeNode.SelectSingleNode("./p:End", nsmgr).InnerText);
                    keyRange.EulaType = rangeNode.SelectSingleNode("./p:EulaType", nsmgr).InnerText;
                    keyRange.PartNumber = rangeNode.SelectSingleNode("./p:PartNumber", nsmgr).InnerText;
                    keyRange.Valid = rangeNode.SelectSingleNode("./p:IsValid", nsmgr).InnerText.ToLower() == "true";

                    ranges[refActIdStr].Add(keyRange);
                }

                foreach (XmlNode configNode in configNodes)
                {
                    string refActIdStr = configNode.SelectSingleNode("./p:ActConfigId", nsmgr).InnerText;
                    Guid refActId = new Guid(refActIdStr);
                    int group = int.Parse(configNode.SelectSingleNode("./p:RefGroupId", nsmgr).InnerText);
                    List<KeyRange> keyRanges = ranges[refActIdStr];

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

            loadedPkeyConfigs.Add(pkcFileId);
        }

        public ProductConfig MatchParams(int group, int serial)
        {
            foreach (ProductConfig config in Products.Values)
            {
                if (config.GroupId == group)
                {
                    foreach (KeyRange range in config.Ranges)
                    {
                        if (range.Contains(serial))
                        {
                            return config;
                        }
                    }
                }
            }

            throw new FileNotFoundException("Failed to find product matching supplied product key parameters.");
        }

        public PKeyConfig()
        {

        }
    }
}

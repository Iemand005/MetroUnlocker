using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MetroUnlocker.LibTSForge.Crypto;
using MetroUnlocker.LibTSForge.PhysicalStore;

namespace MetroUnlocker.LibTSForge.SPP
{

    public class ProductKey
    {
        private static readonly string ALPHABET = "BCDFGHJKMPQRTVWXY2346789";

        private readonly ulong klow;
        private readonly ulong khigh;

        public int Group;
        public int Serial;
        public ulong Security;
        public bool Upgrade;
        public PKeyAlgorithm Algorithm;
        public string EulaType;
        public string PartNumber;
        public string Edition;
        public string Channel;
        public Guid ActivationId;

        private string mpc;
        private string pid2;

        public byte[] KeyBytes
        {
            get { return BitConverter.GetBytes(klow).Concat(BitConverter.GetBytes(khigh)).ToArray(); }
        }

        public ProductKey(int serial, ulong security, bool upgrade, PKeyAlgorithm algorithm, ProductConfig config, KeyRange range)
        {
            Group = config.GroupId;
            Serial = serial;
            Security = security;
            Upgrade = upgrade;
            Algorithm = algorithm;
            EulaType = range.EulaType;
            PartNumber = range.PartNumber.Split(':', ';')[0];
            Edition = config.Edition;
            Channel = config.Channel;
            ActivationId = config.ActivationId;

            klow = ((security & 0x3fff) << 50 | ((ulong)serial & 0x3fffffff) << 20 | ((ulong)Group & 0xfffff));
            khigh = ((upgrade ? (ulong)1 : 0) << 49 | ((security >> 14) & 0x7fffffffff));

            uint checksum = Utils.CRC32(KeyBytes) & 0x3ff;

            khigh |= ((ulong)checksum << 39);
        }

        public string GetAlgoUri()
        {
            return "msft:rm/algorithm/pkey/" + (Algorithm == PKeyAlgorithm.PKEY2005 ? "2005" : (Algorithm == PKeyAlgorithm.PKEY2009 ? "2009" : "Unknown"));
        }

        public Guid GetPkeyId()
        {
            VariableBag pkb = new VariableBag();
            pkb.Blocks.AddRange(new CRCBlock[]
            {
                new CRCBlock
                {
                    DataType = CRCBlockType.STRING,
                    KeyAsStr = "SppPkeyBindingProductKey",
                    ValueAsStr = ToString()
                },
                new CRCBlock
                {
                    DataType = CRCBlockType.BINARY,
                    KeyAsStr = "SppPkeyBindingMiscData",
                    Value = new byte[] { }
                },
                new CRCBlock
                {
                    DataType = CRCBlockType.STRING,
                    KeyAsStr = "SppPkeyBindingAlgorithm",
                    ValueAsStr = GetAlgoUri()
                }
            });

            return new Guid(CryptoUtils.SHA256Hash(pkb.Serialize()).Take(16).ToArray());
        }

        public string GetDefaultMPC()
        {
            int build = Environment.OSVersion.Version.Build;
            string defaultMPC = build >= 10240 ? "03612" :
                                build >= 9600 ? "06401" :
                                build >= 9200 ? "05426" :
                                "55041";
            return defaultMPC;
        }

        public string GetMPC()
        {
            if (mpc != null)
            {
                return mpc;
            }

            mpc = GetDefaultMPC();

            // setup.cfg doesn't exist in Windows 8+
            string setupcfg = string.Format("{0}\\oobe\\{1}", Environment.SystemDirectory, "setup.cfg");

            if (!File.Exists(setupcfg) || Edition.Contains(";"))
            {
                return mpc;
            }

            string mpcKey = string.Format("{0}.{1}=", Utils.GetArchitecture(), Edition);
            string localMPC = File.ReadAllLines(setupcfg).FirstOrDefault(line => line.Contains(mpcKey));
            if (localMPC != null)
            {
                mpc = localMPC.Split('=')[1].Trim();
            }

            return mpc;
        }

        public string GetPid2()
        {
            if (pid2 != null)
            {
                return pid2;
            }

            pid2 = "";

            if (Algorithm == PKeyAlgorithm.PKEY2005)
            {
                string mpc = GetMPC();
                string serialHigh;
                int serialLow;
                int lastPart;

                if (EulaType == "OEM")
                {
                    serialHigh = "OEM";
                    serialLow = ((Group / 2) % 100) * 10000 + (Serial / 100000);
                    lastPart = Serial % 100000;
                }
                else
                {
                    serialHigh = (Serial / 1000000).ToString("D3");
                    serialLow = Serial % 1000000;
                    lastPart = ((Group / 2) % 100) * 1000 + new Random().Next(1000);
                }

                int checksum = 0;

                foreach (char c in serialLow.ToString())
                {
                    checksum += int.Parse(c.ToString());
                }
                checksum = 7 - (checksum % 7);

                pid2 = string.Format("{0}-{1}-{2:D6}{3}-{4:D5}", mpc, serialHigh, serialLow, checksum, lastPart);
            }

            return pid2;
        }

        public byte[] GetPid3()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(0xA4);
            writer.Write(0x3);
            writer.WriteFixedString(GetPid2(), 24);
            writer.Write(Group);
            writer.WriteFixedString(PartNumber, 16);
            writer.WritePadding(0x6C);
            byte[] data = writer.GetBytes();
            byte[] crc = BitConverter.GetBytes(~Utils.CRC32(data.Reverse().ToArray())).Reverse().ToArray();
            writer.Write(crc);

            return writer.GetBytes();
        }

        public byte[] GetPid4()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(0x4F8);
            writer.Write(0x4);
            writer.WriteFixedString16(GetExtendedPid(), 0x80);
            writer.WriteFixedString16(ActivationId.ToString(), 0x80);
            writer.WritePadding(0x10);
            writer.WriteFixedString16(Edition, 0x208);
            writer.Write(Upgrade ? (ulong)1 : 0);
            writer.WritePadding(0x50);
            writer.WriteFixedString16(PartNumber, 0x80);
            writer.WriteFixedString16(Channel, 0x80);
            writer.WriteFixedString16(EulaType, 0x80);

            return writer.GetBytes();
        }

        public string GetExtendedPid()
        {
            string mpc = GetMPC();
            int serialHigh = Serial / 1000000;
            int serialLow = Serial % 1000000;
            int licenseType;
            uint lcid = Utils.GetSystemDefaultLCID();
            int build = Environment.OSVersion.Version.Build;
            int dayOfYear = DateTime.Now.DayOfYear;
            int year = DateTime.Now.Year;

            switch (EulaType)
            {
                case "OEM":
                    licenseType = 2;
                    break;

                case "Volume":
                    licenseType = 3;
                    break;

                default:
                    licenseType = 0;
                    break;
            }

            return string.Format(
                "{0}-{1:D5}-{2:D3}-{3:D6}-{4:D2}-{5:D4}-{6:D4}.0000-{7:D3}{8:D4}",
                mpc,
                Group,
                serialHigh,
                serialLow,
                licenseType,
                lcid,
                build,
                dayOfYear,
                year
            );
        }

        public byte[] GetPhoneData(PSVersion version)
        {
            int serialHigh = Serial / 1000000;
            int serialLow = Serial % 1000000;

            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(new Guid("B8731595-A2F6-430B-A799-FBFFB81A8D73").ToByteArray());
            writer.Write(Group);
            writer.Write(serialHigh);
            writer.Write(serialLow);
            writer.Write(Upgrade ? 1 : 0);
            writer.Write(Security);

            return writer.GetBytes();
        }

        public override string ToString()
        {
            string keyStr = "";
            Random rnd = new Random(Group * 1000000000 + Serial);

            if (Algorithm == PKeyAlgorithm.PKEY2005)
            {
                keyStr = "H4X3DH4X3DH4X3DH4X3D";

                for (int i = 0; i < 5; i++)
                {
                    keyStr += ALPHABET[rnd.Next(24)];
                }
            }
            else if (Algorithm == PKeyAlgorithm.PKEY2009)
            {
                int last = 0;
                byte[] bKey = KeyBytes;

                for (int i = 24; i >= 0; i--)
                {
                    int current = 0;

                    for (int j = 14; j >= 0; j--)
                    {
                        current *= 0x100;
                        current += bKey[j];
                        bKey[j] = (byte)(current / 24);
                        current %= 24;
                        last = current;
                    }

                    keyStr = ALPHABET[current] + keyStr;
                }

                keyStr = keyStr.Substring(1, last) + "N" + keyStr.Substring(last + 1, keyStr.Length - last - 1);
            }

            for (int i = 5; i < keyStr.Length; i += 6)
            {
                keyStr = keyStr.Insert(i, "-");
            }

            return keyStr;
        }
    }
}

using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Collections.Generic;
//using MetroUnlocker.Crypto;
//using MetroUnlocker.PhysicalStore;
using MetroUnlocker.LibTSForge.SPP;
//using MetroUnlocker.TokenStore;

// Common.cs
namespace MetroUnlocker
{
    public enum PSVersion
    {
        Win8Early,
        Win8,
        WinBlue,
        WinModern
    }

    public enum BlockType : uint
    {
        NONE,
        NAMED,
        ATTRIBUTE,
        TIMER
    }

    public static class Constants
    {
        public static readonly string ZeroCID = new string('0', 48);
        public static readonly byte[] UniversalHWIDBlock =
        {
            0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1c, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x0c, 0x01, 0x00
        };
    }

    public static class Utils
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetSystemDefaultLCID();

        public static string GetArchitecture()
        {
            string arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine).ToUpperInvariant();
            return arch == "AMD64" ? "X64" : arch;
        }

        public static void WritePadding(this BinaryWriter writer, int len)
        {
            writer.Write(Enumerable.Repeat((byte)0, len).ToArray());
        }

        public static void WriteFixedString(this BinaryWriter writer, string str, int bLen)
        {
            writer.Write(Encoding.ASCII.GetBytes(str));
            writer.WritePadding(bLen - str.Length);
        }

        public static void WriteFixedString16(this BinaryWriter writer, string str, int bLen)
        {
            byte[] bstr = Utils.EncodeString(str);
            writer.Write(bstr);
            writer.WritePadding(bLen - bstr.Length);
        }

        public static void Align(this BinaryWriter writer, int to)
        {
            int pos = (int)writer.BaseStream.Position;
            writer.Write(Enumerable.Repeat((byte)0, -pos & (to - 1)).ToArray());
        }

        public static void Align(this BinaryReader reader, int to)
        {
            int pos = (int)reader.BaseStream.Position;
            reader.BaseStream.Seek(-pos & (to - 1), SeekOrigin.Current);
        }

        public static string ReadNullTerminatedString(this BinaryReader reader, int maxLen)
        {
            return Encoding.Unicode.GetString(reader.ReadBytes(maxLen)).Split(new char[] { '\0' }, 2)[0];
        }

        public static byte[] GetBytes(this BinaryWriter writer)
        {
            return ((MemoryStream)writer.BaseStream).ToArray();
        }

        public static void WriteAllBytes(FileStream fs, byte[] data)
        {
            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(data.Length);
            fs.Write(data, 0, data.Length);
        }

        public static byte[] ReadAllBytes(FileStream fs)
        {
            BinaryReader br = new BinaryReader(fs);
            return br.ReadBytes((int)fs.Length);
        }

        public static uint CRC32(byte[] data)
        {
            const uint polynomial = 0x04C11DB7;
            uint crc = 0xffffffff;

            foreach (byte b in data)
            {
                crc ^= (uint)b << 24;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x80000000) != 0)
                    {
                        crc = (crc << 1) ^ polynomial;
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return ~crc;
        }

        public static void KillSPP()
        {
            ServiceController sc;

            try
            {
                sc = new ServiceController("sppsvc");

                if (sc.Status == ServiceControllerStatus.Stopped)
                    return;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Unable to access sppsvc: " + ex.Message);
            }

            //Logger.WriteLine("Stopping sppsvc...");

            bool stopped = false;

            for (int i = 0; stopped == false && i < 60; i++)
            {
                try
                {
                    if (sc.Status != ServiceControllerStatus.StopPending)
                        sc.Stop();

                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(500));
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    continue;
                }
                catch (InvalidOperationException)
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                stopped = true;
            }

            if (!stopped)
                throw new System.TimeoutException("Failed to stop sppsvc");

            //Logger.WriteLine("sppsvc stopped successfully.");

        }

        public static PSVersion DetectVersion()
        {
            int build = Environment.OSVersion.Version.Build;

            if (build >= 9600) return PSVersion.WinModern;
            //if (build >= 6000 && build <= 6003) return PSVersion.Vista;
            //if (build >= 7600 && build <= 7602) return PSVersion.Win7;
            if (build == 9200) return PSVersion.Win8;

            throw new NotSupportedException("This version of Windows is not supported. (build " + build + ")");
        }

        public static bool DetectCurrentKey()
        {
            SLApi.RefreshLicenseStatus();

            using (RegistryKey wpaKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\WPA"))
            {
                foreach (string subKey in wpaKey.GetSubKeyNames())
                {
                    if (subKey.StartsWith("8DEC0AF1") && subKey.EndsWith("-1"))
                    {
                        return subKey.Contains("P");
                    }
                }
            }

            throw new FileNotFoundException("Failed to autodetect key type, specify physical store key with /prod or /test arguments.");
        }

        public static string DecodeString(byte[] data)
        {
            return Encoding.Unicode.GetString(data).Trim('\0');
        }

        public static byte[] EncodeString(string str)
        {
            return Encoding.Unicode.GetBytes(str + '\0');
        }

        internal static string GetTokenStorePath()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform", "TokenStore", string.Empty);
        }
    }

    public class TokenMeta
    {
        public string Name;
        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public byte[] Serialize()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(1);
            byte[] nameBytes = Utils.EncodeString(Name);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);

            foreach (KeyValuePair<string, string> kv in Data)
            {
                byte[] keyBytes = Utils.EncodeString(kv.Key);
                byte[] valueBytes = Utils.EncodeString(kv.Value);
                writer.Write(keyBytes.Length);
                writer.Write(valueBytes.Length);
                writer.Write(keyBytes);
                writer.Write(valueBytes);
            }

            return writer.GetBytes();
        }

        public void Deserialize(byte[] data)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            reader.ReadInt32();
            int nameLen = reader.ReadInt32();
            Name = reader.ReadNullTerminatedString(nameLen);

            while (reader.BaseStream.Position < data.Length - 0x8)
            {
                int keyLen = reader.ReadInt32();
                int valueLen = reader.ReadInt32();
                string key = reader.ReadNullTerminatedString(keyLen);
                string value = reader.ReadNullTerminatedString(valueLen);
                Data[key] = value;
            }
        }

        public TokenMeta(byte[] data)
        {
            Deserialize(data);
        }

        public TokenMeta()
        {

        }
    }
}

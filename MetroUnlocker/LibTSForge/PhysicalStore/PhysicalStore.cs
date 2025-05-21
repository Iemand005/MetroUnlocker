using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Win32;

using MetroUnlocker.LibTSForge.Crypto;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public sealed class PhysicalStore : IDisposable
    {
        private byte[] PreHeaderBytes = new byte[] { };
        private readonly Dictionary<string, List<ModernBlock>> Data = new Dictionary<string, List<ModernBlock>>();
        private readonly FileStream TSFile;
        private readonly PhysicalStoreVersion Version;
        private readonly bool Production;

        public byte[] Serialize()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(PreHeaderBytes);
            writer.Write(Data.Keys.Count);

            foreach (string key in Data.Keys)
            {
                List<ModernBlock> blocks = Data[key];
                byte[] keyNameEnc = Utils.EncodeString(key);

                writer.Write(keyNameEnc.Length);
                writer.Write(keyNameEnc);
                writer.Write(blocks.Count);
                writer.Align(4);


                foreach (ModernBlock block in blocks)
                {
                    block.Encode(writer);
                    writer.Align(4);
                }
            }

            return Utils.GetBytes(writer);
        }

        public void Deserialize(byte[] data)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            PreHeaderBytes = reader.ReadBytes(8);

            while (reader.BaseStream.Position < data.Length - 0x4)
            {
                uint numKeys = reader.ReadUInt32();

                for (int i = 0; i < numKeys; i++)
                {
                    uint lenKeyName = reader.ReadUInt32();
                    string keyName = Utils.DecodeString(reader.ReadBytes((int)lenKeyName)); uint numValues = reader.ReadUInt32();

                    reader.Align(4);

                    Data[keyName] = new List<ModernBlock>();

                    for (int j = 0; j < numValues; j++)
                    {
                        Data[keyName].Add(new ModernBlock(reader));
                        reader.Align(4);
                    }
                }
            }
        }

        public void AddBlock(ModernBlock block)
        {
            if (!Data.ContainsKey(block.KeyAsString))
                Data[block.KeyAsString] = new List<ModernBlock>();

            Data[block.KeyAsString].Add(new ModernBlock
            {
                Type = block.Type,
                Flags = block.Flags,
                Unknown = block.Unknown,
                Value = block.Value,
                Data = block.Data
            });
        }

        public void AddBlocks(IEnumerable<ModernBlock> blocks)
        {
            foreach (ModernBlock block in blocks)
            {
                AddBlock(block);
            }
        }

        public ModernBlock GetBlock(string key, string value)
        {
            List<ModernBlock> blocks = Data[key];

            foreach (ModernBlock block in blocks)
            {
                if (block.ValueAsString == value)
                {
                    return new ModernBlock
                    {
                        Type = block.Type,
                        Flags = block.Flags,
                        Key = Utils.EncodeString(key),
                        Value = block.Value,
                        Data = block.Data
                    };
                }
            }

            return null;
        }

        public ModernBlock GetBlock(string key, uint value)
        {
            List<ModernBlock> blocks = Data[key];

            foreach (ModernBlock block in blocks)
            {
                if (block.ValueAsInteger == value)
                {
                    return new ModernBlock
                    {
                        Type = block.Type,
                        Flags = block.Flags,
                        Key = Utils.EncodeString(key),
                        Value = block.Value,
                        Data = block.Data
                    };
                }
            }

            return null;
        }

        public void SetBlock(string key, string value, byte[] data)
        {
            List<ModernBlock> blocks = Data[key];

            for (int i = 0; i < blocks.Count; i++)
            {
                ModernBlock block = blocks[i];

                if (block.ValueAsString == value)
                {
                    block.Data = data;
                    blocks[i] = block;
                    break;
                }
            }

            Data[key] = blocks;
        }

        public void SetBlock(string key, uint value, byte[] data)
        {
            List<ModernBlock> blocks = Data[key];

            for (int i = 0; i < blocks.Count; i++)
            {
                ModernBlock block = blocks[i];

                if (block.ValueAsInteger == value)
                {
                    block.Data = data;
                    blocks[i] = block;
                    break;
                }
            }

            Data[key] = blocks;
        }

        public void SetBlock(string key, string value, string data)
        {
            SetBlock(key, value, Utils.EncodeString(data));
        }

        public void SetBlock(string key, string value, uint data)
        {
            SetBlock(key, value, BitConverter.GetBytes(data));
        }

        public void SetBlock(string key, uint value, string data)
        {
            SetBlock(key, value, Utils.EncodeString(data));
        }

        public void SetBlock(string key, uint value, uint data)
        {
            SetBlock(key, value, BitConverter.GetBytes(data));
        }

        public void DeleteBlock(string key, string value)
        {
            if (Data.ContainsKey(key))
            {
                List<ModernBlock> blocks = Data[key];

                foreach (ModernBlock block in blocks)
                {
                    if (block.ValueAsString == value)
                    {
                        blocks.Remove(block);
                        break;
                    }
                }

                Data[key] = blocks;
            }
        }

        public void DeleteBlock(string key, uint value)
        {
            if (Data.ContainsKey(key))
            {
                List<ModernBlock> blocks = Data[key];

                foreach (ModernBlock block in blocks)
                {
                    if (block.ValueAsInteger == value)
                    {
                        blocks.Remove(block);
                        break;
                    }
                }

                Data[key] = blocks;
            }
        }

        public static string GetPath()
        {
            string sppRoot = Utils.GetTokenStorePath();

            return Path.Combine(Environment.ExpandEnvironmentVariables(sppRoot), "data.dat");
        }

        public PhysicalStore(PhysicalStoreVersion version, bool production)
        {
            Version = version;
            Production = production;

            TSFile = File.Open(GetPath(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            Deserialize(ReadRaw());
        }

        public void Dispose()
        {
            WriteRaw(Serialize());
        }

        public byte[] ReadRaw()
        {
            byte[] data = PhysicalStoreCrypto.DecryptPhysicalStore(Utils.ReadAllBytes(TSFile), Production);
            TSFile.Seek(0, SeekOrigin.Begin);
            return data;
        }

        public void WriteRaw(byte[] data)
        {
            if (TSFile.CanWrite)
            {
                byte[] encrData = PhysicalStoreCrypto.EncryptPhysicalStore(data, Production, Version);
                TSFile.SetLength(encrData.LongLength);
                TSFile.Seek(0, SeekOrigin.Begin);
                Utils.WriteAllBytes(TSFile, encrData);
                TSFile.Close();
            }
        }
    }
}

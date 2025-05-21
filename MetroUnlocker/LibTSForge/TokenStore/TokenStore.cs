using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MetroUnlocker.LibTSForge.Crypto;

namespace MetroUnlocker.LibTSForge.TokenStore
{

    public class TokenStore : IDisposable
    {
        private static readonly uint Version = 3;
        private static readonly int EntrySize = 0x9E;
        private static readonly int BlockSize = 0x4020;
        private static readonly int EntriesPerBlock = BlockSize / EntrySize;
        private static readonly int BlockPadSize = 0x66;

        private static readonly byte[] Header = Enumerable.Repeat((byte)0x55, 0x20).ToArray();
        private static readonly byte[] Footer = Enumerable.Repeat((byte)0xAA, 0x20).ToArray();

        private List<TokenEntry> Entries = new List<TokenEntry>();
        public FileStream TokensFile { get; set; }

        public void Deserialize()
        {
            if (TokensFile.Length < BlockSize) return;

            TokensFile.Seek(0x24, SeekOrigin.Begin);
            uint nextBlock = 0;

            BinaryReader reader = new BinaryReader(TokensFile);
            do
            {
                uint curOffset = reader.ReadUInt32();
                nextBlock = reader.ReadUInt32();

                for (int i = 0; i < EntriesPerBlock; i++)
                {
                    curOffset = reader.ReadUInt32();
                    bool populated = reader.ReadUInt32() == 1;
                    uint contentOffset = reader.ReadUInt32();
                    uint contentLength = reader.ReadUInt32();
                    uint allocLength = reader.ReadUInt32();
                    byte[] contentData = new byte[] { };

                    if (populated)
                    {
                        reader.BaseStream.Seek(contentOffset + 0x20, SeekOrigin.Begin);
                        uint dataLength = reader.ReadUInt32();

                        if (dataLength != contentLength)
                            throw new FormatException("Data length in tokens content is inconsistent with entry.");

                        reader.ReadBytes(0x20);
                        contentData = reader.ReadBytes((int)contentLength);
                    }

                    reader.BaseStream.Seek(curOffset + 0x14, SeekOrigin.Begin);

                    Entries.Add(new TokenEntry(reader.ReadNullTerminatedString(0x82), reader.ReadNullTerminatedString(0x8), contentData, populated));
                }

                reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);
            } while (nextBlock != 0);
        }

        public void Serialize()
        {
            MemoryStream tokens = new MemoryStream();

            using (BinaryWriter writer = new BinaryWriter(tokens))
            {
                writer.Write(Version);
                writer.Write(Header);

                int curBlockOffset = (int)writer.BaseStream.Position;
                int curEntryOffset = curBlockOffset + 0x8;
                int curContsOffset = curBlockOffset + BlockSize;

                for (int eIndex = 0; eIndex < ((Entries.Count / EntriesPerBlock) + 1) * EntriesPerBlock; eIndex++)
                {
                    TokenEntry entry = eIndex < Entries.Count ? entry = Entries[eIndex] : new TokenEntry();

                    writer.BaseStream.Seek(curBlockOffset, SeekOrigin.Begin);
                    writer.Write(curBlockOffset);
                    writer.Write(0);

                    writer.BaseStream.Seek(curEntryOffset, SeekOrigin.Begin);
                    writer.Write(curEntryOffset);
                    writer.Write(entry.Populated ? 1 : 0);
                    writer.Write(entry.Populated ? curContsOffset : 0);
                    writer.Write(entry.Populated ? entry.Data.Length : -1);
                    writer.Write(entry.Populated ? entry.Data.Length : -1);
                    writer.WriteFixedString16(entry.Name, 0x82);
                    writer.WriteFixedString16(entry.Extension, 0x8);
                    curEntryOffset = (int)writer.BaseStream.Position;

                    if (entry.Populated)
                    {
                        writer.BaseStream.Seek(curContsOffset, SeekOrigin.Begin);
                        writer.Write(Header);
                        writer.Write(entry.Data.Length);
                        writer.Write(CryptoUtils.SHA256Hash(entry.Data));
                        writer.Write(entry.Data);
                        writer.Write(Footer);
                        curContsOffset = (int)writer.BaseStream.Position;
                    }

                    if ((eIndex + 1) % EntriesPerBlock == 0 && eIndex != 0)
                    {
                        if (eIndex < Entries.Count)
                        {
                            writer.BaseStream.Seek(curBlockOffset + 0x4, SeekOrigin.Begin);
                            writer.Write(curContsOffset);
                        }

                        writer.BaseStream.Seek(curEntryOffset, SeekOrigin.Begin);
                        writer.WritePadding(BlockPadSize);

                        writer.BaseStream.Seek(curBlockOffset, SeekOrigin.Begin);
                        byte[] blockHash;
                        byte[] blockData = new byte[BlockSize - 0x20];

                        tokens.Read(blockData, 0, BlockSize - 0x20);
                        blockHash = CryptoUtils.SHA256Hash(blockData);

                        writer.BaseStream.Seek(curBlockOffset + BlockSize - 0x20, SeekOrigin.Begin);
                        writer.Write(blockHash);

                        curBlockOffset = curContsOffset;
                        curEntryOffset = curBlockOffset + 0x8;
                        curContsOffset = curBlockOffset + BlockSize;
                    }
                }

                tokens.SetLength(curBlockOffset);
            }

            byte[] tokensData = tokens.ToArray();
            byte[] tokensHash = CryptoUtils.SHA256Hash(tokensData.Take(0x4).Concat(tokensData.Skip(0x24)).ToArray());

            tokens = new MemoryStream(tokensData);

            BinaryWriter tokWriter = new BinaryWriter(TokensFile);
            using (BinaryReader reader = new BinaryReader(tokens))
            {
                TokensFile.Seek(0, SeekOrigin.Begin);
                TokensFile.SetLength(tokens.Length);
                tokWriter.Write(reader.ReadBytes(0x4));
                reader.ReadBytes(0x20);
                tokWriter.Write(tokensHash);
                tokWriter.Write(reader.ReadBytes((int)reader.BaseStream.Length - 0x4));
            }
        }

        public void AddEntry(TokenEntry entry)
        {
            Entries.Add(entry);
        }

        public void AddEntries(TokenEntry[] entries)
        {
            Entries.AddRange(entries);
        }

        public void DeleteEntry(string name, string ext)
        {
            foreach (TokenEntry entry in Entries)
                if (entry.Name == name && entry.Extension == ext)
                {
                    Entries.Remove(entry);
                    return;
                }
        }

        public void DeleteUnpopulatedEntry(string name, string extension)
        {
            Entries = Entries.FindAll(entry => entry.Name != name && entry.Extension != extension && entry.Populated);
        }

        public TokenEntry GetEntry(string name, string ext)
        {
            foreach (TokenEntry entry in Entries)
                if (entry.Name == name && entry.Extension == ext)
                    if (entry.Populated) return entry;
            return null;
        }

        public TokenMeta GetMetaEntry(string name)
        {
            DeleteUnpopulatedEntry(name, "xml");
            TokenEntry entry = GetEntry(name, "xml");

            return entry == null ? new TokenMeta(name) : new TokenMeta(entry.Data);
        }

        public void SetEntry(string name, string extension, byte[] data)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                TokenEntry entry = Entries[i];

                if (entry.Name == name && entry.Extension == extension && entry.Populated)
                {
                    entry.Data = data;
                    Entries[i] = entry;
                    return;
                }
            }

            Entries.Add(new TokenEntry(name, extension, data));
        }

        public static string GetPath()
        {
            return Path.Combine(Environment.ExpandEnvironmentVariables(Utils.GetTokenStorePath()), "tokens.dat");
        }

        public TokenStore()
        {
            string tokensPath = GetPath();
            TokensFile = File.Open(tokensPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            Deserialize();
        }

        public void Dispose()
        {
            Serialize();
            TokensFile.Close();
        }
    }
}

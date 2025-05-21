using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public class VariableBag
    {
        public List<CRCBlock> Blocks = new List<CRCBlock>();

        public VariableBag() { }
        public VariableBag(byte[] data) { Deserialize(data); }

        public void Deserialize(byte[] data)
        {
            int len = data.Length;

            BinaryReader reader = new BinaryReader(new MemoryStream(data));

            while (reader.BaseStream.Position < len - 0x10)
                Blocks.Add(new CRCBlock(reader));
        }

        public byte[] Serialize()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());

            foreach (CRCBlock block in Blocks)
                block.Encode(writer);

            return ((MemoryStream)writer.BaseStream).ToArray();
        }

        public CRCBlock GetBlock(string key)
        {
            return Blocks.Find(block => block.KeyAsString == key);
        }

        public void SetBlock(string key, byte[] value)
        {
            int index = Blocks.FindIndex(block => block.KeyAsString == key);
            if (index != -1) Blocks[index].Value = value;
        }

        public void DeleteBlock(string key)
        {
            CRCBlock block = GetBlock(key);
            if (block != null) Blocks.Remove(block);
        }
    }
}

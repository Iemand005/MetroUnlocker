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
        public VariableBag(byte[] data)
        {
            Deserialize(data);
        }

        public void Deserialize(byte[] data)
        {
            int len = data.Length;

            BinaryReader reader = new BinaryReader(new MemoryStream(data));

            while (reader.BaseStream.Position < len - 0x10)
            {
                Blocks.Add(CRCBlock.Decode(reader));
            }
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
            foreach (CRCBlock block in Blocks)
            {
                if (block.KeyAsStr == key)
                {
                    return block;
                }
            }

            return null;
        }

        public void SetBlock(string key, byte[] value)
        {
            for (int i = 0; i < Blocks.Count; i++)
            {
                CRCBlock block = Blocks[i];

                if (block.KeyAsStr == key)
                {
                    block.Value = value;
                    Blocks[i] = block;
                    break;
                }
            }
        }

        public void DeleteBlock(string key)
        {
            foreach (CRCBlock block in Blocks)
                if (block.KeyAsStr == key)
                {
                    Blocks.Remove(block);
                    return;
                }
        }
    }
}

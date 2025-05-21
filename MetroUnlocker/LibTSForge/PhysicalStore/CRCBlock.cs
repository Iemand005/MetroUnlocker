using System;
using System.Collections.Generic;
using System.IO;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public enum CRCBlockType : uint
    {
        UInt = 1 << 0,
        String = 1 << 1,
        Binary = 1 << 2
    }

    public class CRCBlock : BasicBlock
    {
        public CRCBlockType DataType;

        public CRCBlock() { }

        public CRCBlock(BinaryReader reader)
        {
            uint crc = reader.ReadUInt32();
            uint type = reader.ReadUInt32();
            uint lenName = reader.ReadUInt32();
            uint lenVal = reader.ReadUInt32();

            byte[] key = reader.ReadBytes((int)lenName);

            reader.Align(8);

            byte[] value = reader.ReadBytes((int)lenVal);
            reader.Align(8);

            DataType = (CRCBlockType)type;
            Key = key;
            Value = value;

            if (CRC() != crc)
                throw new InvalidDataException("Invalid CRC in variable bag.");
        }

        public void Encode(BinaryWriter writer)
        {
            uint crc = CRC();
            writer.Write(crc);
            writer.Write((uint)DataType);
            writer.Write(Key.Length);
            writer.Write(Value.Length);

            writer.Write(Key);
            writer.Align(8);

            writer.Write(Value);
            writer.Align(8);
        }

        public uint CRC()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(0);
            writer.Write((uint)DataType);
            writer.Write(Key.Length);
            writer.Write(Value.Length);
            writer.Write(Key);
            writer.Write(Value);
            return Utils.CRC32(((MemoryStream)writer.BaseStream).ToArray());
        }
    }
}

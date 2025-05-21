using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public class ModernBlock : BasicBlock
    {
        public BlockType Type;
        public uint Flags;
        public uint Unknown;

        public byte[] Data;
        public string DataAsString
        {
            get { return Utils.DecodeString(Data); }
            set { Data = Utils.EncodeString(value); }
        }

        public uint DataAsInt
        {
            get { return BitConverter.ToUInt32(Data, 0); }
            set { Data = BitConverter.GetBytes(value); }
        }

        public ModernBlock() { }
        public ModernBlock(string key, string value, byte[] data, BlockType type = BlockType.NAMED, uint flags = 0)
        {
            Type = type;
            Flags = flags;
            KeyAsString = key;
            ValueAsString = value;
            Data = data;
        }
        public ModernBlock(ModernBlock block)
        {
            Type = block.Type;
            Flags = block.Flags;
            Unknown = block.Unknown;
            Value = block.Value;
            Data = block.Data;
        }

        public void Encode(BinaryWriter writer)
        {
            writer.Write((uint)Type);
            writer.Write(Flags);
            writer.Write((uint)Value.Length);
            writer.Write((uint)Data.Length);
            writer.Write(Unknown);
            writer.Write(Value);
            writer.Write(Data);
        }

        public ModernBlock(BinaryReader reader)
        {
            uint type = reader.ReadUInt32();
            uint flags = reader.ReadUInt32();

            uint valueLen = reader.ReadUInt32();
            uint dataLen = reader.ReadUInt32();
            uint unk3 = reader.ReadUInt32();

            byte[] value = reader.ReadBytes((int)valueLen);
            byte[] data = reader.ReadBytes((int)dataLen);

            Type = (BlockType)type;
            Flags = flags;
            Unknown = unk3;
            Value = value;
            Data = data;
        }
    }
}

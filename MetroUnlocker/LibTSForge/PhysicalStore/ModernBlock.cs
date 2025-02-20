using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public class ModernBlock
    {
        public BlockType Type;
        public uint Flags;
        public uint Unknown;
        public byte[] Key;
        public string KeyAsStr
        {
            get
            {
                return Utils.DecodeString(Key);
            }
            set
            {
                Key = Utils.EncodeString(value);
            }
        }
        public byte[] Value;
        public string ValueAsStr
        {
            get
            {
                return Utils.DecodeString(Value);
            }
            set
            {
                Value = Utils.EncodeString(value);
            }
        }
        public uint ValueAsInt
        {
            get
            {
                return BitConverter.ToUInt32(Value, 0);
            }
            set
            {
                Value = BitConverter.GetBytes(value);
            }
        }
        public byte[] Data;
        public string DataAsStr
        {
            get
            {
                return Utils.DecodeString(Data);
            }
            set
            {
                Data = Utils.EncodeString(value);
            }
        }
        public uint DataAsInt
        {
            get
            {
                return BitConverter.ToUInt32(Data, 0);
            }
            set
            {
                Data = BitConverter.GetBytes(value);
            }
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

        public static ModernBlock Decode(BinaryReader reader)
        {
            uint type = reader.ReadUInt32();
            uint flags = reader.ReadUInt32();

            uint valueLen = reader.ReadUInt32();
            uint dataLen = reader.ReadUInt32();
            uint unk3 = reader.ReadUInt32();

            byte[] value = reader.ReadBytes((int)valueLen);
            byte[] data = reader.ReadBytes((int)dataLen);

            return new ModernBlock
            {
                Type = (BlockType)type,
                Flags = flags,
                Unknown = unk3,
                Value = value,
                Data = data,
            };
        }
    }
}

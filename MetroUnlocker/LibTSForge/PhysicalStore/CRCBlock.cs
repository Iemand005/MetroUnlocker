using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public enum CRCBlockType : uint
    {
        UINT = 1 << 0,
        STRING = 1 << 1,
        BINARY = 1 << 2
    }

    public class CRCBlock
    {
        public CRCBlockType DataType;
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

        public static CRCBlock Decode(BinaryReader reader)
        {
            uint crc = reader.ReadUInt32();
            uint type = reader.ReadUInt32();
            uint lenName = reader.ReadUInt32();
            uint lenVal = reader.ReadUInt32();

            byte[] key = reader.ReadBytes((int)lenName);

            reader.Align(8);

            byte[] value = reader.ReadBytes((int)lenVal);
            reader.Align(8);

            CRCBlock block = new CRCBlock
            {
                DataType = (CRCBlockType)type,
                Key = key,
                Value = value,
            };

            if (block.CRC() != crc)
            {
                throw new InvalidDataException("Invalid CRC in variable bag.");
            }

            return block;
        }

        public uint CRC()
        {
            BinaryWriter wtemp = new BinaryWriter(new MemoryStream());
            wtemp.Write(0);
            wtemp.Write((uint)DataType);
            wtemp.Write(Key.Length);
            wtemp.Write(Value.Length);
            wtemp.Write(Key);
            wtemp.Write(Value);
            return Utils.CRC32(((MemoryStream)wtemp.BaseStream).ToArray());
        }
    }
}

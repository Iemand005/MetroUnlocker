using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroUnlocker.LibTSForge.PhysicalStore
{
    public class BasicBlock
    {
        public byte[] Key;
        public string KeyAsString
        {
            get { return Utils.DecodeString(Key); }
            set { Key = Utils.EncodeString(value); }
        }

        public byte[] Value;
        public string ValueAsString
        {
            get { return Utils.DecodeString(Value); }
            set { Value = Utils.EncodeString(value); }
        }
        public uint ValueAsInteger
        {
            get { return BitConverter.ToUInt32(Value, 0); }
            set { Value = BitConverter.GetBytes(value); }
        }
    }
}

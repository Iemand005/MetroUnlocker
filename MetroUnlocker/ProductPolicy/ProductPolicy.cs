using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace MetroUnlocker.ProductPolicy
{
    class ProductPolicy
    {
        public ProductPolicyValue Header { get; set; }
        public string Name { get; set; }

        public byte[] Bytes { get; set; }
        public string StringValue { get { return Encoding.Unicode.GetString(Bytes); } }
        public uint DWordValue { get { return BitConverter.ToUInt32(Bytes, 0); } }

        public RegistryValueKind Type
        {
            get { return (RegistryValueKind)Header.DataType; }
        }

        private void NameFromBin(ref byte[] PolicyBlob, int offset)
        {
            Name = Encoding.Unicode.GetString(PolicyBlob, offset + Marshal.SizeOf(Header), Header.Name);
        }

        private void ValFromBin(ref byte[] PolicyBlob, int offset)
        {
            int posdata = offset + Marshal.SizeOf(Header) + Header.Name;
            Bytes = new byte[Header.Data];
            Array.Copy(PolicyBlob, posdata, Bytes, 0, Header.Data);
        }

        public void FromBin(ref byte[] PolicyBlob, int offset)
        {
            Header = ProductPolicyReader.BytesToStruct<ProductPolicyValue>(PolicyBlob, offset);
            if ((Header.Data + Header.Name + Marshal.SizeOf(Header)) > Header.Size || (offset + Header.Size) > PolicyBlob.Length)
                throw new Exception("Invalid data Header format");

            NameFromBin(ref PolicyBlob, offset);
            ValFromBin(ref PolicyBlob, offset);
        }

        public override string ToString()
        {
            switch (Type)
            {
                case RegistryValueKind.String:
                    return StringValue;
                case RegistryValueKind.DWord:
                    return DWordValue.ToString();
                default:
                    return Bytes != null ? BitConverter.ToString(Bytes) : "";
            }
        }

        public int Size() { return Bytes.Length; }
    }
}

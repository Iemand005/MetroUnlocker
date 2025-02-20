using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.Win32;


namespace MetroUnlocker.ProductPolicy
{
    //public struct ProductPolicyData
    //{
    //    public string String;
    //    public byte[] Bytes;
    //    public uint DWord;
    //};
    class ProductPolicy
    {
        public bool Modified { get; set; }
        public ProductPolicyValue Header { get; set; }
        public string Name { get; set; }
        //private ProductPolicyData _data;

        //public ProductPolicyData Data
        //{
        //    get { return _data; }
        //    set { _data = value; }
        //}

        public byte[] Bytes { get; set; }
        public string StringValue
        {
            get
            {
                return Encoding.Unicode.GetString(Bytes);
            }
            set {
                Bytes = Encoding.Unicode.GetBytes(value);
            }
        }
        public uint DWordValue
        {
            get
            {
                return BitConverter.ToUInt32(Bytes, 0);
            }
            set
            {
                Bytes = BitConverter.GetBytes(value);
            }
        }

        public RegistryValueKind Type
        {
            get
            {
                return (RegistryValueKind)Header.DataType;
            }
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
            Header = Tools.BytesToStruct<ProductPolicyValue>(PolicyBlob, typeof(ProductPolicyValue), offset);
            if ((Header.Data + Header.Name + Marshal.SizeOf(Header)) > Header.Size ||
                (offset + Header.Size) > PolicyBlob.Length)
            {
                throw new Exception("Invalid _data Header format");
            }
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

        public int Size()
        {
            //switch (Type)
            //{
            //    case RegistryValueKind.String:
            //        return StringValue.Length * 2;
            //    case RegistryValueKind.DWord:
            //        return 4;
            //    default:
            //        return Bytes.Length;
            //}
            return Bytes.Length;
        }

        //public byte[] ToBinary()
        //{
        //    switch (Type)
        //    {
        //        case RegistryValueKind.String:
        //            return Encoding.Unicode.GetBytes(_data.String);
        //        case RegistryValueKind.DWord:
        //            return Tools.StructToBytes(DWordVq);
        //        default:
        //            return Bytes;
        //    }
        //}

        public byte[] ToBin()
        {
            ProductPolicyValue value = new ProductPolicyValue();
            value.Name = (UInt16)(2 * Name.Length);
            value.Data = (UInt16)Size();
            int datablocksize = Marshal.SizeOf(Header) + value.Name + value.Data;
            int suffixLength = 4 - (datablocksize % 4);
            value.Size = (UInt16)(Marshal.SizeOf(Header) + value.Name + value.Data + suffixLength);
            value.DataType = Header.DataType;
            value.Unknown1 = Header.Unknown1;
            value.Unknown2 = Header.Unknown2;
            byte[] bytes = Tools.StructToBytes(value);

            Tools.AppendBytes(ref bytes, Encoding.Unicode.GetBytes(Name));
            Tools.AppendBytes(ref bytes, Bytes);
            Tools.PaddingAppend(ref bytes, suffixLength);

            return bytes;
        }

        // I would like to not split the data into dword and string on parsing but on getting.
        //public void SetDWordValue(uint value)
        //{
        //    Data = new ProductPolicyData { DWord = value };
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace MetroUnlocker.ProductPolicy
{
    public struct ProductPolicyHeader
    {
        public UInt32 Size, DataSize, EndMarker, Unknown1, Unknown2;
    }

    public struct ProductPolicyValue
    {
        public UInt16 Size, Name, DataType, Data;
        public UInt32 Unknown1, Unknown2;
    }

    public enum PolicyState : uint
    {
        Disabled = 0,
        Enabled = 1,
        Unknown
    }
    
    class ProductPolicyEditor
    {
        public List<ProductPolicy> PolicyValues { get; set; }
        public ProductPolicyHeader Header;
        private byte[] _policyBytes;
        private byte[] _productPolicyBlobSuffix;

        const string ProductOptionsKey = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\ProductOptions";

        public ProductPolicyEditor()
        {
            _policyBytes = GetPolicyBlob();
            PolicyValues = FromBinary();
        }

        public bool Save()
        {
            return SetPolicyBlob(ToBin());
        }

        private static byte[] GetPolicyBlob()
        {
            return (byte[])Registry.GetValue(ProductOptionsKey, "ProductPolicy", null);
        }

        private static bool SetPolicyBlob(byte[] bytes)
        {
            Registry.SetValue(ProductOptionsKey, "ProductPolicy", bytes);
            Thread.Sleep(1000);
            return Tools.ArraysEqual(GetPolicyBlob(), bytes);
        }

        public List<ProductPolicy> FromBinary()
        {
            return FromBinary(_policyBytes);
        }

        public List<ProductPolicy> FromBinary(byte[] policyBlob)
        {
            var header = Tools.BytesToStruct<ProductPolicyHeader>(policyBlob, typeof(ProductPolicyHeader));

            if (header.Size < policyBlob.Length || (header.DataSize + Marshal.SizeOf(header) + header.EndMarker) != header.Size)
                throw new Exception("Invalid Header format");

            int pos = Marshal.SizeOf(header);
            int pos_end = pos + (int)header.DataSize;

            PolicyValues = new List<ProductPolicy>();

            while (pos < pos_end)
            {
                ProductPolicy pv = new ProductPolicy();
                pv.FromBin(ref policyBlob, pos);
                PolicyValues.Add(pv);

                pos += pv.Header.Size;
            }

            if (pos < header.Size)
            {
                _productPolicyBlobSuffix = new byte[policyBlob.Length - pos];
                Array.Copy(policyBlob, pos, _productPolicyBlobSuffix, 0, _productPolicyBlobSuffix.Length);
            }
            else
            {
                _productPolicyBlobSuffix = null;
            }

            return PolicyValues;
        }

        public byte[] ToBin()
        {
            int headerSize = Marshal.SizeOf(Header);
            byte[] policyBlob = new byte[headerSize];

            foreach (ProductPolicy v in PolicyValues)
                Tools.AppendBytes(ref policyBlob, v.ToBin());

            Tools.AppendBytes(ref policyBlob, _productPolicyBlobSuffix);

            Header.Size = (UInt32)policyBlob.Length;
            Header.EndMarker = (UInt32)_productPolicyBlobSuffix.Length;
            Header.DataSize = (UInt32)(Header.Size - Header.EndMarker - headerSize);
            Array.Copy(Tools.StructToBytes(Header), 0, policyBlob, 0, headerSize);

            return policyBlob;
        }

        public ProductPolicy GetPolicyWithName(string name)
        {
            return PolicyValues.Find(value => value.Name == name);
        }

        public PolicyState GetPolicyStateByName(string name)
        {
            ProductPolicy policy = GetPolicyWithName(name);
            if (policy.Type == RegistryValueKind.DWord)
            {
                if (policy.DWordValue == 0)
                    return PolicyState.Disabled;
                else if (policy.DWordValue == 1)
                    return PolicyState.Enabled;
            }
            return PolicyState.Unknown;
        }

        public bool SetPolicyStateByName(string name, PolicyState state)
        {
            ProductPolicy policy = GetPolicyWithName(name);

            if (policy.Type == RegistryValueKind.DWord)
            {
                switch (state)
                {
                    case PolicyState.Disabled:
                        policy.DWordValue = 0;
                        break;
                    case PolicyState.Enabled:
                        policy.DWordValue = 1;
                        break;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.Win32;

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
    
    class ProductPolicyReader
    {
        public List<ProductPolicy> PolicyValues { get; set; }

        const string ProductOptionsKey = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\ProductOptions";

        public ProductPolicyReader()
        {
            PolicyValues = FromBinary(GetPolicyBlob());
        }

        private static byte[] GetPolicyBlob()
        {
            return (byte[])Registry.GetValue(ProductOptionsKey, "ProductPolicy", null);
        }

        static public T BytesToStruct<T>(byte[] bytes, int offset = 0)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, offset, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        public List<ProductPolicy> FromBinary(byte[] policyBlob)
        {
            var header = BytesToStruct<ProductPolicyHeader>(policyBlob);

            if (header.Size < policyBlob.Length || (header.DataSize + Marshal.SizeOf(header) + header.EndMarker) != header.Size)
                throw new Exception("Invalid Header format");

            int position = Marshal.SizeOf(header);
            int end = position + (int)header.DataSize;

            PolicyValues = new List<ProductPolicy>();

            while (position < end)
            {
                ProductPolicy policy = new ProductPolicy();
                policy.FromBin(ref policyBlob, position);
                PolicyValues.Add(policy);

                position += policy.Header.Size;
            }

            return PolicyValues;
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
    }
}

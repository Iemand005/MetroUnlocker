using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MetroUnlocker.ProductPolicy
{
    class Tools
    {
        public static void AppendBytes(ref byte[] b1, byte[] b2)
        {
            int length = b1.Length;
            Array.Resize(ref b1, length + b2.Length);
            Array.Copy(b2, 0, b1, length, b2.Length);
        }

        static public T BytesToStruct<T>(byte[] bytes, Type type, int offset = 0)
        {
            int size = Marshal.SizeOf(type);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, offset, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, type);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        static public byte[] StructToBytes(object structObject)
        {
            int size = Marshal.SizeOf(structObject);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObject, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static void PaddingAppend(ref byte[] b1, int paddinglen)
        {
            if (paddinglen > 0)
            {
                int l = b1.Length;
                Array.Resize(ref b1, b1.Length + paddinglen);
                for (; l < b1.Length; l++)
                    b1[l] = 0;
            }
        }

        static public bool ArraysEqual(Array a1, Array a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            System.Collections.IList list1 = a1, list2 = a2; //error CS0305: Using the generic type 'System.Collections.Generic.IList<T>' requires '1' type arguments
            for (int i = 0; i < a1.Length; i++)
            {
                if (!Object.Equals(list1[i], list2[i])) //error CS0021: Cannot apply indexing with [] to an expression of type 'IList'(x2)
                    return false;
            }
            return true;
        }
    }
}

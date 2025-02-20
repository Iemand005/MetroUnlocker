using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MetroUnlocker.LibTSForge.SPP
{
    public static class SLApi
    {
        private enum SLIDTYPE
        {
            SL_ID_APPLICATION,
            SL_ID_PRODUCT_SKU,
            SL_ID_LICENSE_FILE,
            SL_ID_LICENSE,
            SL_ID_PKEY,
            SL_ID_ALL_LICENSES,
            SL_ID_ALL_LICENSE_FILES,
            SL_ID_STORE_TOKEN,
            SL_ID_LAST
        }

        private enum SLDATATYPE
        {
            SL_DATA_NONE,
            SL_DATA_SZ,
            SL_DATA_DWORD,
            SL_DATA_BINARY,
            SL_DATA_MULTI_SZ,
            SL_DATA_SUM
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SL_LICENSING_STATUS
        {
            public Guid SkuId;
            public uint eStatus;
            public uint dwGraceTime;
            public uint dwTotalGraceDays;
            public uint hrReason;
            public ulong qwValidityExpiration;
        }

        public static readonly Guid WINDOWS_APP_ID = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f");

        [DllImport("sppc.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SLOpen(out IntPtr hSLC);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SLClose(IntPtr hSLC);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetWindowsInformationDWORD(string ValueName, ref int Value);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLInstallProofOfPurchase(IntPtr hSLC, string pwszPKeyAlgorithm, string pwszPKeyString, uint cbPKeySpecificData, byte[] pbPKeySpecificData, ref Guid PKeyId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLUninstallProofOfPurchase(IntPtr hSLC, ref Guid PKeyId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetPKeyInformation(IntPtr hSLC, ref Guid pPKeyId, string pwszValueName, out SLDATATYPE peDataType, out uint pcbValue, out IntPtr ppbValue);

        [DllImport("sppcext.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLActivateProduct(IntPtr hSLC, ref Guid pProductSkuId, byte[] cbAppSpecificData, byte[] pvAppSpecificData, byte[] pActivationInfo, string pwszProxyServer, ushort wProxyPort);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGenerateOfflineInstallationId(IntPtr hSLC, ref Guid pProductSkuId, ref string ppwszInstallationId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLDepositOfflineConfirmationId(IntPtr hSLC, ref Guid pProductSkuId, string pwszInstallationId, string pwszConfirmationId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetSLIDList(IntPtr hSLC, SLIDTYPE eQueryIdType, ref Guid pQueryId, SLIDTYPE eReturnIdType, out uint pnReturnIds, out IntPtr ppReturnIds);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SLGetLicensingStatusInformation(IntPtr hSLC, ref Guid pAppID, IntPtr pProductSkuId, string pwszRightName, out uint pnStatusCount, out IntPtr ppLicensingStatus);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetInstalledProductKeyIds(IntPtr hSLC, ref Guid pProductSkuId, out uint pnProductKeyIds, out IntPtr ppProductKeyIds);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLConsumeWindowsRight(uint unknown);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetProductSkuInformation(IntPtr hSLC, ref Guid pProductSkuId, string pwszValueName, out SLDATATYPE peDataType, out uint pcbValue, out IntPtr ppbValue);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetProductSkuInformation(IntPtr hSLC, ref Guid pProductSkuId, string pwszValueName, IntPtr peDataType, out uint pcbValue, out IntPtr ppbValue);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLGetLicense(IntPtr hSLC, ref Guid pLicenseFileId, out uint pcbLicenseFile, out IntPtr ppbLicenseFile);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLSetCurrentProductKey(IntPtr hSLC, ref Guid pProductSkuId, ref Guid pProductKeyId);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        private static extern uint SLFireEvent(IntPtr hSLC, string pwszEventId, ref Guid pApplicationId);

        public class SLContext : IDisposable
        {
            public readonly IntPtr Handle;

            public SLContext()
            {
                SLOpen(out Handle);
            }

            public void Dispose()
            {
                SLClose(Handle);
                GC.SuppressFinalize(this);
            }

            ~SLContext()
            {
                Dispose();
            }
        }

        public static Guid GetPkeyConfigFileId(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                SLDATATYPE type;
                uint len;
                IntPtr ppReturnLics;

                uint status = SLGetProductSkuInformation(sl.Handle, ref actId, "pkeyConfigLicenseId", out type, out len, out ppReturnLics);

                if (status != 0 || len == 0)
                {
                    return Guid.Empty;
                }

                Guid pkcId = new Guid(Marshal.PtrToStringAuto(ppReturnLics));
                return GetLicenseFileId(pkcId);
            }
        }

        public static Guid GetLicenseFileId(Guid licId)
        {
            using (SLContext sl = new SLContext())
            {
                uint status;
                uint count;
                IntPtr ppReturnLics;

                status = SLGetSLIDList(sl.Handle, SLIDTYPE.SL_ID_LICENSE, ref licId, SLIDTYPE.SL_ID_LICENSE_FILE, out count, out ppReturnLics);

                return (status == 0 && count != 0) ? (Guid)Marshal.PtrToStructure(ppReturnLics, typeof(Guid)) : Guid.Empty;//new Guid(Marshal.PtrToStringAuto(ppReturnLics));
            }
        }

        public static string GetLicenseContents(Guid fileId)
        {
            if (fileId == Guid.Empty) throw new ArgumentException("License contents could not be retrieved.");

            using (SLContext sl = new SLContext())
            {
                uint dataLen;
                IntPtr dataPtr;

                if (SLGetLicense(sl.Handle, ref fileId, out dataLen, out dataPtr) != 0)
                {
                    return null;
                }

                byte[] data = new byte[dataLen];
                Marshal.Copy(dataPtr, data, 0, (int)dataLen);

                data = data.Skip(Array.IndexOf(data, (byte)'<')).ToArray();
                return Encoding.UTF8.GetString(data);
            }
        }

        public static string GetMetaStr(Guid actId, string value)
        {
            using (SLContext sl = new SLContext())
            {
                uint len;
                SLDATATYPE type;
                IntPtr ppbValue;

                uint status = SLGetProductSkuInformation(sl.Handle, ref actId, value, out type, out len, out ppbValue);

                if (status != 0 || len == 0 || type != SLDATATYPE.SL_DATA_SZ)
                {
                    return null;
                }

                return Marshal.PtrToStringAuto(ppbValue);
            }
        }

        public static string GetInstallationId(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                string installationId = null;
                var status = SLGenerateOfflineInstallationId(sl.Handle, ref actId, ref installationId);

                if (status != 0)
                    throw new Exception(string.Format("Failed to get installation ID: 0x{0}. Your data.dat is probably corrupt at the moment. Try again later.", status.ToString("X")));

                return installationId;
            }
        }

        public static Guid GetInstalledPkeyId(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                uint status;
                uint count;
                IntPtr pProductKeyIds;

                status = SLGetInstalledProductKeyIds(sl.Handle, ref actId, out count, out pProductKeyIds);

                //unsafe { return *(Guid*)pProductKeyIds; }
                return (status == 0 && count != 0) ? (Guid)Marshal.PtrToStructure(pProductKeyIds, typeof(Guid)) : Guid.Empty;
            }
        }

        public static uint DepositConfirmationId(Guid actId, string installationId, string confirmationId)
        {
            using (SLContext sl = new SLContext())
                return SLDepositOfflineConfirmationId(sl.Handle, ref actId, installationId, confirmationId);
        }

        public static void RefreshLicenseStatus()
        {
            SLConsumeWindowsRight(0);
        }

        public static bool RefreshTrustedTime(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                SLDATATYPE type;
                uint count;
                IntPtr ppbValue;

                uint status = SLGetProductSkuInformation(sl.Handle, ref actId, "TrustedTime", out type, out count, out ppbValue);
                return (int)status >= 0 && status != 0xC004F012;
            }
        }

        public static void FireStateChangedEvent(Guid appId)
        {
            using (SLContext sl = new SLContext())
            {
                SLFireEvent(sl.Handle, "msft:rm/event/licensingstatechanged", ref appId);
            }
        }

        public static Guid GetAppId(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                uint count;
                IntPtr pAppIds;

                uint status = SLGetSLIDList(sl.Handle, SLIDTYPE.SL_ID_PRODUCT_SKU, ref actId, SLIDTYPE.SL_ID_APPLICATION, out count, out pAppIds);

                if (status != 0 || count == 0)
                    return Guid.Empty;

                return (Guid)Marshal.PtrToStructure(pAppIds, typeof(Guid));
            }
        }

        public static bool IsAddon(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                uint count;
                SLDATATYPE type;
                IntPtr ppbValue;

                uint status = SLGetProductSkuInformation(sl.Handle, ref actId, "DependsOn", out type, out count, out ppbValue);
                return (int)status >= 0 && status != 0xC004F012;
            }
        }

        public static uint InstallProductKey(ProductKey pkey)
        {
            using (SLContext sl = new SLContext())
            {
                Guid pkeyId = Guid.Empty;
                return SLInstallProofOfPurchase(sl.Handle, pkey.GetAlgoUri(), pkey.ToString(), 0, null, ref pkeyId);
            }
        }

        public static uint UninstallProductKey(Guid pkeyId)
        {
            using (SLContext sl = new SLContext())
                return SLUninstallProofOfPurchase(sl.Handle, ref pkeyId);
        }
    }
}

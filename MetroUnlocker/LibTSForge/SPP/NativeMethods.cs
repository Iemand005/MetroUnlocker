using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace MetroUnlocker.LibTSForge.SPP
{
    public enum SLIDType
    {
        Application,
        ProductSku,
        LicenseFile,
        License,
        ProductKey,
        AllLicenses,
        AllLicenseFiles,
        StoreToken,
        Last
    }

    public enum SLDataType
    {
        None,
        String,
        DWord,
        Binary,
        MultiString,
        Sum
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SLLicensingStatus
    {
        public Guid SkuId;
        public uint Status;
        public uint GraceTimeDWord;
        public uint TotalGraceDaysDWord;
        public uint ReasonHResult;
        public ulong ValidityExpiration;
    }

    public class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern uint GetSystemDefaultLCID();

        [DllImport("sppc.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void SLOpen(out IntPtr hSLC);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void SLClose(IntPtr hSLC);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLInstallProofOfPurchase(IntPtr hSLC, string pwszPKeyAlgorithm, string pwszPKeyString, uint cbPKeySpecificData, byte[] pbPKeySpecificData, ref Guid PKeyId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLUninstallProofOfPurchase(IntPtr hSLC, ref Guid PKeyId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLGenerateOfflineInstallationId(IntPtr hSLC, ref Guid pProductSkuId, ref string ppwszInstallationId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLDepositOfflineConfirmationId(IntPtr hSLC, ref Guid pProductSkuId, string pwszInstallationId, string pwszConfirmationId);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLGetSLIDList(IntPtr hSLC, SLIDType eQueryIdType, ref Guid pQueryId, SLIDType eReturnIdType, out uint pnReturnIds, out IntPtr ppReturnIds);

        [DllImport("sppc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLGetInstalledProductKeyIds(IntPtr hSLC, ref Guid pProductSkuId, out uint pnProductKeyIds, out IntPtr ppProductKeyIds);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLConsumeWindowsRight(uint unknown);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLGetProductSkuInformation(IntPtr hSLC, ref Guid pProductSkuId, string pwszValueName, out SLDataType peDataType, out uint pcbValue, out IntPtr ppbValue);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLGetProductSkuInformation(IntPtr hSLC, ref Guid pProductSkuId, string pwszValueName, IntPtr peDataType, out uint pcbValue, out IntPtr ppbValue);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLGetLicense(IntPtr hSLC, ref Guid pLicenseFileId, out uint pcbLicenseFile, out IntPtr ppbLicenseFile);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLSetCurrentProductKey(IntPtr hSLC, ref Guid pProductSkuId, ref Guid pProductKeyId);

        [DllImport("slc.dll", CharSet = CharSet.Unicode)]
        internal static extern uint SLFireEvent(IntPtr hSLC, string pwszEventId, ref Guid pApplicationId);
    }
}

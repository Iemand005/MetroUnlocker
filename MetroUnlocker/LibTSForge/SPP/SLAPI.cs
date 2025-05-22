using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MetroUnlocker.LibTSForge.SPP
{
    public static class SLApi
    {
        public class SLContext : IDisposable
        {
            public readonly IntPtr Handle;

            public SLContext() { NativeMethods.SLOpen(out Handle); }

            public void Dispose()
            {
                NativeMethods.SLClose(Handle);
                GC.SuppressFinalize(this);
            }

            ~SLContext() { Dispose(); }
        }

        public static Guid GetProductKeyConfigFileId(Guid activationId)
        {
            using (SLContext sl = new SLContext())
            {
                SLDataType type;
                uint length;
                IntPtr fileIdPointer;

                uint status = NativeMethods.SLGetProductSkuInformation(sl.Handle, ref activationId, "pkeyConfigLicenseId", out type, out length, out fileIdPointer);

                if (status != 0 || length == 0)
                    return Guid.Empty;

                Guid configLicenseId = new Guid(Marshal.PtrToStringAuto(fileIdPointer));
                return GetLicenseFileId(configLicenseId);
            }
        }

        public static Guid GetLicenseFileId(Guid licenseId)
        {
            using (SLContext sl = new SLContext())
            {
                uint status;
                uint count;
                IntPtr returnLicenses;

                status = NativeMethods.SLGetSLIDList(sl.Handle, SLIDType.License, ref licenseId, SLIDType.LicenseFile, out count, out returnLicenses);

                return (status == 0 && count != 0) ? (Guid)Marshal.PtrToStructure(returnLicenses, typeof(Guid)) : Guid.Empty;
            }
        }

        public static string GetLicenseContents(Guid fileId)
        {
            if (fileId == Guid.Empty) throw new ArgumentException("License contents could not be retrieved.");

            using (SLContext sl = new SLContext())
            {
                uint length;
                IntPtr dataPointer;

                if (NativeMethods.SLGetLicense(sl.Handle, ref fileId, out length, out dataPointer) != 0)
                    return null;

                byte[] data = new byte[length];
                Marshal.Copy(dataPointer, data, 0, (int)length);

                data = data.Skip(Array.IndexOf(data, (byte)'<')).ToArray();
                return Encoding.UTF8.GetString(data);
            }
        }

        public static string GetMetaStr(Guid productSkuId, string value)
        {
            using (SLContext sl = new SLContext())
            {
                uint length;
                SLDataType type;
                IntPtr binaryValue;

                uint status = NativeMethods.SLGetProductSkuInformation(sl.Handle, ref productSkuId, value, out type, out length, out binaryValue);

                if (status != 0 || length == 0 || type != SLDataType.String)
                    return null;

                return Marshal.PtrToStringAuto(binaryValue);
            }
        }

        public static string GetInstallationId(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                string installationId = null;
                var status = NativeMethods.SLGenerateOfflineInstallationId(sl.Handle, ref actId, ref installationId);

                if (status != 0)
                    throw new Exception(string.Format("Failed to get installation ID: 0x{0}. Your data.dat is probably corrupt at the moment. Try again later.", status.ToString("X")));

                return installationId;
            }
        }

        public static Guid GetInstalledProductKeyId(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                uint status;
                uint count;
                IntPtr productKeyIds;

                status = NativeMethods.SLGetInstalledProductKeyIds(sl.Handle, ref actId, out count, out productKeyIds);

                return (status == 0 && count != 0) ? (Guid)Marshal.PtrToStructure(productKeyIds, typeof(Guid)) : Guid.Empty;
            }
        }

        public static uint DepositConfirmationId(Guid actId, string installationId, string confirmationId)
        {
            using (SLContext sl = new SLContext())
                return NativeMethods.SLDepositOfflineConfirmationId(sl.Handle, ref actId, installationId, confirmationId);
        }

        public static void RefreshLicenseStatus()
        {
            NativeMethods.SLConsumeWindowsRight(0);
        }

        public static bool RefreshTrustedTime(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                SLDataType type;
                uint count;
                IntPtr ppbValue;

                uint status = NativeMethods.SLGetProductSkuInformation(sl.Handle, ref actId, "TrustedTime", out type, out count, out ppbValue);
                return (int)status >= 0 && status != 0xC004F012;
            }
        }

        public static void FireStateChangedEvent(Guid appId)
        {
            using (SLContext sl = new SLContext())
                NativeMethods.SLFireEvent(sl.Handle, "msft:rm/event/licensingstatechanged", ref appId);
        }

        public static Guid GetAppId(Guid activationId)
        {
            using (SLContext sl = new SLContext())
            {
                uint count;
                IntPtr appIdPointer;

                uint status = NativeMethods.SLGetSLIDList(sl.Handle, SLIDType.ProductSku, ref activationId, SLIDType.Application, out count, out appIdPointer);

                if (status != 0 || count == 0)
                    return Guid.Empty;

                return (Guid)Marshal.PtrToStructure(appIdPointer, typeof(Guid));
            }
        }

        public static bool IsAddon(Guid actId)
        {
            using (SLContext sl = new SLContext())
            {
                uint count;
                SLDataType type;
                IntPtr ppbValue;

                uint status = NativeMethods.SLGetProductSkuInformation(sl.Handle, ref actId, "DependsOn", out type, out count, out ppbValue);
                return (int)status >= 0 && status != 0xC004F012;
            }
        }

        public static uint InstallProductKey(ProductKey pkey)
        {
            using (SLContext sl = new SLContext())
            {
                Guid productKeyId = Guid.Empty;
                return NativeMethods.SLInstallProofOfPurchase(sl.Handle, pkey.GetAlgoUri(), pkey.ToString(), 0, null, ref productKeyId);
            }
        }

        public static uint UninstallProductKey(Guid productKeyId)
        {
            using (SLContext sl = new SLContext())
                return NativeMethods.SLUninstallProofOfPurchase(sl.Handle, ref productKeyId);
        }
    }
}

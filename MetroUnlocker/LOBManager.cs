﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MetroUnlocker.ProductPolicy;
using MetroUnlocker;
using MetroUnlocker.LibTSForge.PhysicalStore;
using MetroUnlocker.LibTSForge.Modifiers;
using MetroUnlocker.LibTSForge.TokenStore;
using MetroUnlocker.LibTSForge.SPP;

namespace MetroUnlocker
{
    public class LOBManager
    {
        private static Guid ActivationId = new Guid("ec67814b-30e6-4a50-bf7b-d55daf729d1e");

        public static bool Backup = true;

        public static void ActivateZeroCID()
        {
            PSVersion version = Utils.DetectVersion();
            bool production = Utils.DetectCurrentKey();

            if (Backup) BackupPhysicalStore();

            GenPKeyInstall.InstallGenPKey(version, production, ActivationId);

            ZeroCID.Activate(version, production, ActivationId);
        }

        public static Guid GetInstalledSideloadingKeyId()
        {
            return SLApi.GetInstalledPkeyId(ActivationId);
        }

        public static bool IsSideloadingKeyInstalled(out Guid sideloadingKeyId)
        {
            return (sideloadingKeyId = GetInstalledSideloadingKeyId()) != Guid.Empty;
        }

        public static bool UninstallSideloadingKey(Guid sideloadingKeyId)
        {
            if (Backup) BackupPhysicalStore();
            bool result = SLApi.UninstallProductKey(sideloadingKeyId) == 0;
            SLApi.RefreshLicenseStatus();
            SLApi.FireStateChangedEvent(SLApi.GetAppId(ActivationId));
            return result;
        }

        public static string GetUniqueFileName(string fileName)
        {
            string uniqueFileName = fileName;

            for (int i = 1; File.Exists(uniqueFileName); i++)
            {
                string extension = Path.GetExtension(fileName);
                string path = Path.GetFileNameWithoutExtension(fileName);
                uniqueFileName = string.Format("{0}{1}.{2}", path, extension, i);
            }

            return uniqueFileName;
        }

        public static void BackupPhysicalStore()
        {
            string backupFileName = "data.dat.bak";
            
            string physicalStore = PhysicalStore.GetPath();

            File.Copy(physicalStore, GetUniqueFileName(backupFileName), false);


            backupFileName = "tokens.dat.bak";

            physicalStore = TokenStore.GetPath();

            File.Copy(physicalStore, GetUniqueFileName(backupFileName), false);
        }

        public static bool SetPolicyState(PolicyState state)
        {
            ProductPolicyEditor policyEditor = new ProductPolicyEditor();
            policyEditor.SetPolicyStateByName("WSLicensingService-LOBSideloadingActivated", state);
            return policyEditor.Save();
        }

        public static bool Enable()
        {
            return SetPolicyState(PolicyState.Enabled);
        }

        public static bool Disable()
        {
            return SetPolicyState(PolicyState.Disabled);
        }

        public static bool IsSideloadingKeyInstalled()
        {
            Guid sideloadingKeyId;
            return IsSideloadingKeyInstalled(out sideloadingKeyId);
        }
    }
}

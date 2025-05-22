using System;

using MetroUnlocker.LibTSForge.SPP;

namespace MetroUnlocker.LibTSForge.Modifiers
{
    public static class ProductKeyInstaller
    {
        public static void InstallGeneratedProductKey(PhysicalStoreVersion version, bool production, Guid actId)
        {
            ProductKeyConfig keyConfig = new ProductKeyConfig();
            
            keyConfig.LoadConfig(actId);

            ProductConfig config;
            keyConfig.Products.TryGetValue(actId, out config);

            if (config == null) throw new ArgumentException("Activation ID " + actId + " not found in ProductKeyConfig.");

            ProductKey pkey = config.GetRandomKey();

            Guid instPkeyId = SLApi.GetInstalledProductKeyId(actId);
            if (instPkeyId != Guid.Empty) SLApi.UninstallProductKey(instPkeyId);

            if (pkey.Algorithm != ProductKeyAlgorithm.ProductKey2009)
                throw new Exception("The key algorithm isn't 2009");
            
            uint status = SLApi.InstallProductKey(pkey);

            if (status != 0)
                throw new ApplicationException("Failed to install generated product key.");

            return;
        }
    }
}

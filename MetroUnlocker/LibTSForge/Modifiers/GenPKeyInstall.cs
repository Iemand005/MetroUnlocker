using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroUnlocker.LibTSForge.Modifiers
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using MetroUnlocker.LibTSForge.PhysicalStore;
    using MetroUnlocker.LibTSForge.SPP;
    using MetroUnlocker.LibTSForge.TokenStore;

    public static class GenPKeyInstall
    {
        public static void InstallGenPKey(PSVersion version, bool production, Guid actId)
        {
            PKeyConfig pkc = new PKeyConfig();
            
            pkc.LoadConfig(actId);

            ProductConfig config;
            pkc.Products.TryGetValue(actId, out config);

            if (config == null) throw new ArgumentException("Activation ID " + actId + " not found in PKeyConfig.");

            ProductKey pkey = config.GetRandomKey();

            Guid instPkeyId = SLApi.GetInstalledPkeyId(actId);
            if (instPkeyId != Guid.Empty) SLApi.UninstallProductKey(instPkeyId);

            if (pkey.Algorithm != PKeyAlgorithm.PKEY2009)
                throw new Exception("The key algorithm isn't 2009");
            
            uint status = SLApi.InstallProductKey(pkey);

            if (status != 0)
                throw new ApplicationException("Failed to install generated product key.");

            return;
        }
    }
}

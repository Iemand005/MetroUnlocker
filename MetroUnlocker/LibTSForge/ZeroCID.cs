using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32;

using MetroUnlocker.LibTSForge.SPP;
using MetroUnlocker.LibTSForge.PhysicalStore;
using MetroUnlocker.LibTSForge.Crypto;

namespace MetroUnlocker
{

    class ZeroCID
    {
        public static void Deposit(Guid actId, string instId)
        {
            uint status = SLApi.DepositConfirmationId(actId, instId, Constants.ZeroCID);

            if (status != 0)
                throw new COMException(string.Format("Failed to deposit fake CID. Status code: 0x{0}", status.ToString("X")), (int)status);
        }

        public static void Activate(PhysicalStoreVersion version, bool production, Guid activationId)
        {
            Guid appId = SLApi.GetAppId(activationId);

            string instId = SLApi.GetInstallationId(activationId);
            Guid pkeyId = SLApi.GetInstalledProductKeyId(activationId);

            Utils.KillSPP();

            using (PhysicalStore store = new PhysicalStore(version, production))
            {
                byte[] hwidBlock = Constants.UniversalHardwareIdBlock;

                byte[] iidHash = CryptoUtils.SHA256Hash(Utils.EncodeString(instId + '\0' + Constants.ZeroCID));
                

                string key = string.Format("SPPSVC\\{0}\\{1}", appId, activationId);
                ModernBlock keyBlock = store.GetBlock(key, pkeyId.ToString());

                if (keyBlock == null)
                    throw new InvalidDataException("Failed to get product key data for activation ID: 0x" + activationId + ".");

                VariableBag keyBag = new VariableBag(keyBlock.Data);

                byte[] pkeyData = keyBag.GetBlock("SppPkeyPhoneActivationData").Value;
                
                keyBag.DeleteBlock("SppPkeyVirtual");
                store.SetBlock(key, pkeyId.ToString(), keyBag.Serialize());

                BinaryWriter writer = new BinaryWriter(new MemoryStream());
                writer.Write(0x20);
                writer.Write(iidHash);
                writer.Write(hwidBlock.Length);
                writer.Write(hwidBlock);
                byte[] tsHwidData = Utils.GetBytes(writer);

                writer = new BinaryWriter(new MemoryStream());
                writer.Write(0x20);
                writer.Write(iidHash);
                writer.Write(pkeyData.Length);
                writer.Write(pkeyData);
                byte[] tsPkeyInfoData = Utils.GetBytes(writer);

                string path = "msft:Windows/7.0/Phone/Cached/";

                store.AddBlocks(new ModernBlock[] {
                    new ModernBlock(key, path + "HwidBlock/" + pkeyId, tsHwidData),
                    new ModernBlock(key, path + "PKeyInfo/" + pkeyId, tsPkeyInfoData)
                });
            }

            Deposit(activationId, instId);

            SLApi.RefreshLicenseStatus();
            SLApi.FireStateChangedEvent(appId);
        }
    }
}
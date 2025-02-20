using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
                throw new InvalidOperationException(string.Format("Failed to deposit fake CID. Status code: 0x{0}", status.ToString("X")));
        }

        public static void Activate(PSVersion version, bool production, Guid actId)
        {
            Guid appId = SLApi.GetAppId(actId);

            string instId = SLApi.GetInstallationId(actId);
            Guid pkeyId = SLApi.GetInstalledPkeyId(actId);

            Utils.KillSPP();

            using (PhysicalStore store = new PhysicalStore(version, production))
            {
                byte[] hwidBlock = Constants.UniversalHWIDBlock;

                byte[] iidHash = CryptoUtils.SHA256Hash(Utils.EncodeString(instId + '\0' + Constants.ZeroCID));
                

                string key = string.Format("SPPSVC\\{0}\\{1}", appId, actId);
                ModernBlock keyBlock = store.GetBlock(key, pkeyId.ToString());

                if (keyBlock == null)
                {
                    throw new InvalidDataException("Failed to get product key data for activation ID: 0x" + actId + ".");
                }

                VariableBag pkb = new VariableBag(keyBlock.Data);

                byte[] pkeyData = pkb.GetBlock("SppPkeyPhoneActivationData").Value;
                
                pkb.DeleteBlock("SppPkeyVirtual");
                store.SetBlock(key, pkeyId.ToString(), pkb.Serialize());

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

                store.AddBlocks(new ModernBlock[] {
                    new ModernBlock
                    {
                        Type = BlockType.NAMED,
                        Flags = 0,
                        KeyAsStr = key,
                        ValueAsStr = "msft:Windows/7.0/Phone/Cached/HwidBlock/" + pkeyId,
                        Data = tsHwidData
                    }, 
                    new ModernBlock
                    {
                        Type = BlockType.NAMED,
                        Flags = 0,
                        KeyAsStr = key,
                        ValueAsStr = "msft:Windows/7.0/Phone/Cached/PKeyInfo/" + pkeyId,
                        Data = tsPkeyInfoData
                    }
                });
            }

            Deposit(actId, instId);

            SLApi.RefreshLicenseStatus();
            SLApi.FireStateChangedEvent(appId);
        }
    }
}
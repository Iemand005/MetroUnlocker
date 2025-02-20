using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MetroUnlocker.LibTSForge;

namespace MetroUnlocker.LibTSForge.Crypto
{
    public static class PhysicalStoreCrypto
    {
        public static byte[] DecryptPhysicalStore(byte[] data, bool production)
        {
            byte[] rsaKey = production ? Keys.PRODUCTION : Keys.TEST;
            BinaryReader br = new BinaryReader(new MemoryStream(data));
            br.BaseStream.Seek(0x10, SeekOrigin.Begin);
            byte[] aesKeySig = br.ReadBytes(0x80);
            byte[] encAesKey = br.ReadBytes(0x80);

            if (!CryptoUtils.RSAVerifySignature(rsaKey, encAesKey, aesKeySig))
                throw new Exception("Failed to decrypt physical store.");

            byte[] aesKey = CryptoUtils.RSADecrypt(rsaKey, encAesKey);
            byte[] decData = CryptoUtils.AESDecrypt(br.ReadBytes((int)br.BaseStream.Length - 0x110), aesKey);
            byte[] hmacKey = decData.Take(0x10).ToArray();
            byte[] hmacSig = decData.Skip(0x10).Take(0x14).ToArray();
            byte[] psData = decData.Skip(0x28).ToArray();

            if (!CryptoUtils.HMACVerify(hmacKey, psData, hmacSig))
                throw new Exception("Failed to verify HMAC. Physical store is either corrupt or in Vista format.");

            return psData;
        }

        public static byte[] EncryptPhysicalStore(byte[] data, bool production, PSVersion version)
        {
            Dictionary<PSVersion, int> versionTable = new Dictionary<PSVersion, int>
            {
                {PSVersion.Win8, 1},
                {PSVersion.WinBlue, 2},
                {PSVersion.WinModern, 3}
            };

            byte[] rsaKey = production ? Keys.PRODUCTION : Keys.TEST;

            byte[] aesKey = Encoding.UTF8.GetBytes("Boop Foxyz nose!");
            byte[] hmacKey = CryptoUtils.GenerateRandomKey(0x10);

            byte[] encAesKey = CryptoUtils.RSAEncrypt(rsaKey, aesKey);
            byte[] aesKeySig = CryptoUtils.RSASign(rsaKey, encAesKey);
            byte[] hmacSig = CryptoUtils.HMACSign(hmacKey, data);

            byte[] decData = new byte[] { };
            decData = decData.Concat(hmacKey).Concat(hmacSig).Concat(BitConverter.GetBytes(0)).Concat(data).ToArray();
            byte[] encData = CryptoUtils.AESEncrypt(decData, aesKey);

            BinaryWriter bw = new BinaryWriter(new MemoryStream());
            bw.Write(versionTable[version]);
            bw.Write(Encoding.UTF8.GetBytes("UNTRUSTSTORE"));
            bw.Write(aesKeySig);
            bw.Write(encAesKey);
            bw.Write(encData);

            return bw.GetBytes();
        }
    }
}

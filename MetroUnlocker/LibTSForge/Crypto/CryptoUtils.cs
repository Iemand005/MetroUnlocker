using System;
using System.Linq;
using System.Security.Cryptography;

namespace MetroUnlocker.LibTSForge.Crypto
{
    public static class CryptoUtils
    {
        public static byte[] GenerateRandomKey(int len)
        {
            byte[] rand = new byte[len];
            Random r = new Random();
            r.NextBytes(rand);

            return rand;
        }

        public static byte[] AESEncrypt(byte[] data, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, Enumerable.Repeat((byte)0, 16).ToArray());
                byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
                return encryptedData;
            }
        }

        public static byte[] AESDecrypt(byte[] data, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, Enumerable.Repeat((byte)0, 16).ToArray());
                byte[] decryptedData = decryptor.TransformFinalBlock(data, 0, data.Length);
                return decryptedData;
            }
        }

        public static byte[] RSADecrypt(byte[] rsaKey, byte[] data)
        {

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(rsaKey);
                return rsa.Decrypt(data, false);
            }
        }

        public static byte[] RSAEncrypt(byte[] rsaKey, byte[] data)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(rsaKey);
                return rsa.Encrypt(data, false);
            }
        }

        public static byte[] RSASign(byte[] rsaKey, byte[] data)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(rsaKey);
                RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(rsa);
                formatter.SetHashAlgorithm("SHA1");

                byte[] hash;
                using (SHA1 sha1 = SHA1.Create())
                {
                    hash = sha1.ComputeHash(data);
                }

                return formatter.CreateSignature(hash);
            }
        }

        public static bool RSAVerifySignature(byte[] rsaKey, byte[] data, byte[] signature)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(rsaKey);
                RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
                deformatter.SetHashAlgorithm("SHA1");

                byte[] hash;
                using (SHA1 sha1 = SHA1.Create())
                {
                    hash = sha1.ComputeHash(data);
                }

                return deformatter.VerifySignature(hash, signature);
            }
        }

        public static byte[] HMACSign(byte[] key, byte[] data)
        {
            HMACSHA1 hmac = new HMACSHA1(key);
            return hmac.ComputeHash(data);
        }

        public static bool HMACVerify(byte[] key, byte[] data, byte[] signature)
        {
            HMACSHA1 hmac = new HMACSHA1(key);
            return Enumerable.SequenceEqual(signature, HMACSign(key, data));
        }

        public static byte[] SHA256Hash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }
    }
}

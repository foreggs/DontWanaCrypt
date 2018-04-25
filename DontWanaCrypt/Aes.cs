using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DontWanaCrypt
{
    class Aes
    {
        public static byte[] defaultSalt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static byte[] Encrypt(byte[] toEncrypt, byte[] password)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = defaultSalt;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(password, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(toEncrypt, 0, toEncrypt.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        public static byte[] Decrypt(byte[] toDecrypt, byte[] password)
        {
            byte[] decryptedBytes = null;
            byte[] saltBytes = defaultSalt;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(password, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(toDecrypt, 0, toDecrypt.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Xnlab.SQLMon
{
    internal class AES
    {
        private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        private const string key = "_XnlabSQLMonitor";

        internal static string Encrypt(string Content)
        {
            return Encrypt(Content, key);
        }

        internal static string Encrypt(string Content, string Key)
        {
            using (SymmetricAlgorithm des = Rijndael.Create())
            {
                byte[] input = Encoding.UTF8.GetBytes(Content);
                des.Key = Encoding.UTF8.GetBytes(Key);
                des.IV = _key1;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(input, 0, input.Length);
                        cs.FlushFinalBlock();
                        byte[] result = ms.ToArray();
                        cs.Close();
                        ms.Close();
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        internal static string Decrypt(string Content)
        {
            return Decrypt(Content, key);
        }

        internal static string Decrypt(string Content, string Key)
        {
            using (SymmetricAlgorithm des = Rijndael.Create())
            {
                des.Key = Encoding.UTF8.GetBytes(Key);
                des.IV = _key1;
                byte[] input = Convert.FromBase64String(Content);
                byte[] result = new byte[input.Length];
                using (MemoryStream ms = new MemoryStream(input))
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        cs.Read(result, 0, result.Length);
                        cs.Close();
                        ms.Close();
                        return Encoding.UTF8.GetString(result).TrimEnd('\0');
                    }
                }
            }
        }
    }
}

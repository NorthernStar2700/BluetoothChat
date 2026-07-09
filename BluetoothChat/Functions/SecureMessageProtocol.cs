using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public static class SecureMessageProtocol
    {
        public static async Task<byte[]> EncryptAsync(string message, byte[] aesKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream memory = new MemoryStream())
                using (CryptoStream crypt = new CryptoStream(memory, encryptor, CryptoStreamMode.Write))
                using (StreamWriter writer = new StreamWriter(crypt))
                {
                    await writer.WriteAsync(message);
                    return memory.ToArray();
                }
            }
        }

        public static async Task<string> DecryptAsync(string message, byte[] aesKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();

                byte[] dataToDecrypt = Convert.FromBase64String(message);

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream memory = new MemoryStream(dataToDecrypt))
                using (CryptoStream crypt = new CryptoStream(memory, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(crypt))
                {
                    string result = await reader.ReadToEndAsync();
                    return result;
                }
            }
        }
    }
}

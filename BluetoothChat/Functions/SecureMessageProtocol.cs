using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public static class SecureMessageProtocol
    {
        public static async Task<byte[]> EncryptAsync(string message, byte[] aesKey)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new IOException("Message content is missing or empty.");
            }
            if (aesKey == null || aesKey.Length == 0)
            {
                throw new IOException("AES key is missing or empty.");
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                byte[] iv = aes.IV;
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] encryptedMessageBytes;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream memory = new MemoryStream())
                using (CryptoStream crypt = new CryptoStream(memory, encryptor, CryptoStreamMode.Write))
                {
                    // Finish writing everything before returning the result
                    await crypt.WriteAsync(messageBytes, 0, messageBytes.Length);
                    crypt.FlushFinalBlock();

                    encryptedMessageBytes = memory.ToArray();
                }

                // Use the same initialization vector
                byte[] result = Combine(iv, encryptedMessageBytes);
                return result;
            }
        }

        public static async Task<string> DecryptAsync(string message, byte[] aesKey)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new IOException("Message content is missing or empty.");

            }
            if (aesKey == null || aesKey.Length == 0)
            {
                throw new IOException("AES key is missing or empty.");
            }

            byte[] dataToDecrypt = Convert.FromBase64String(message);

            using (Aes aes = Aes.Create())
            {
                int ivLength = aes.BlockSize / 8;
                byte[] iv = new byte[ivLength];
                byte[] decryptedText = new byte[dataToDecrypt.Length - ivLength];

                aes.Key = aesKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = iv;

                // IV byte array now has data (IV = 0 to IV.Length)
                Buffer.BlockCopy(dataToDecrypt, 0, iv, 0, ivLength);

                // Text byte array now has data (Message = IV.Length to decryptedText.Length)
                Buffer.BlockCopy(dataToDecrypt, ivLength, decryptedText, 0, decryptedText.Length);

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream memory = new MemoryStream(dataToDecrypt))
                using (CryptoStream crypt = new CryptoStream(memory, decryptor, CryptoStreamMode.Read))
                using (MemoryStream output = new MemoryStream())
                {
                    await crypt.CopyToAsync(output);
                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
        }

        private static byte[] Combine(byte[] array1, byte[] array2)
        {
            byte[] combinedBytes = new byte[array1.Length + array2.Length];

            // Copy array 1's contents to the new array
            Buffer.BlockCopy(array1, 0, combinedBytes, 0, array1.Length);

            // Copy array 2's contents to the new array starting at the end of array 1's length
            Buffer.BlockCopy(array2, 0, combinedBytes, array1.Length, array2.Length);

            return combinedBytes;
        }
    }
}

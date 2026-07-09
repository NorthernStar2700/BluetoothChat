using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public static class CryptoKeyGenerator
    {
        public static byte[] GenerateBytes(int byteCount)
        {
            byte[] bytes = new byte[byteCount];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        public static byte[] GenerateAesKey()
        {
            return GenerateBytes(32);
        }
    }
}

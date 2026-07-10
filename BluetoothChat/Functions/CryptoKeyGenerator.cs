using System.Security.Cryptography;


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

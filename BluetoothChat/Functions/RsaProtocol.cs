using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.IO;

namespace BluetoothChat.Functions
{
    public static class RsaProtocol
    {
        private static RsaKeyParameters rsaPublicKey;
        private static RsaKeyParameters rsaPrivateKey;

        public static void CreateRsaKeys()
        {
            RsaKeyPairGenerator rsaKeyPairGen = new RsaKeyPairGenerator();
            KeyGenerationParameters keyGenParams = new KeyGenerationParameters(new SecureRandom(), 1024);
            rsaKeyPairGen.Init(keyGenParams);
            AsymmetricCipherKeyPair keyPair = rsaKeyPairGen.GenerateKeyPair();

            rsaPublicKey = (RsaKeyParameters)keyPair.Public;
            rsaPrivateKey = (RsaKeyParameters)keyPair.Private;
        }

        public static string GetPublicKeyString()
        {
            if (rsaPublicKey == null)
            {
                throw new CryptoException("Public key has not been generated yet.");
            }

            using (StringWriter writer = new StringWriter())
            using (PemWriter pemWriter = new PemWriter(writer))
            {
                pemWriter.WriteObject(rsaPublicKey);
                pemWriter.Writer.Flush();

                return writer.ToString();
            }
        }

        public static RsaKeyParameters GetPrivateKey()
        {
            if (rsaPrivateKey == null)
            {
                throw new CryptoException("Private key has not been generated yet.");
            }

            return rsaPrivateKey;
        }

        public static RsaKeyParameters ReadPublicKey(string rsaPublicKey)
        {
            if (string.IsNullOrWhiteSpace(rsaPublicKey))
            {
                throw new CryptoException("Public key is missing or invalid");
            }

            using (StringReader reader = new StringReader(rsaPublicKey))
            using (PemReader pemReader = new PemReader(reader))
            {
                AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pemReader.ReadObject();
                return (RsaKeyParameters)publicKey;
            }
        }
    }
}

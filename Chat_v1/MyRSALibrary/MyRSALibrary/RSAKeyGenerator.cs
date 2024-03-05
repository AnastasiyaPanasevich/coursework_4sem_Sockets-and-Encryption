using System;
using System.Security.Cryptography;
using System.Text;

namespace MyRSALibrary
{
    public class RSAEncryption
    {
        private RSACryptoServiceProvider cryptoServiceProvider;

        public RSAEncryption()
        {
            cryptoServiceProvider = new RSACryptoServiceProvider(512);
        }

        public string GetPublicKey()
        {
            RSAParameters publicKey = cryptoServiceProvider.ExportParameters(false);
            return GetKeyString(publicKey);
        }

        public string GetPrivateKey()
        {
            RSAParameters privateKey = cryptoServiceProvider.ExportParameters(true);
            return GetKeyString(privateKey);
        }

        public string Encrypt(string textToEncrypt, string publicKeyString)
        {
            byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(publicKeyString);
                    byte[] encryptedData = rsa.Encrypt(bytesToEncrypt, true);
                    string base64Encrypted = Convert.ToBase64String(encryptedData);
                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public string Decrypt(string textToDecrypt, string privateKeyString)
        {
            byte[] bytesToDecrypt = Convert.FromBase64String(textToDecrypt);

            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(privateKeyString);
                    byte[] decryptedBytes = rsa.Decrypt(bytesToDecrypt, true);
                    string decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        private string GetKeyString(RSAParameters publicKey)
        {
            var stringWriter = new System.IO.StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, publicKey);
            return stringWriter.ToString();
        }
    }
}

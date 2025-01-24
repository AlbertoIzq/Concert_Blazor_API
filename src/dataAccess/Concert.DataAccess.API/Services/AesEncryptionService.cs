using Concert.DataAccess.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Concert.DataAccess.API.Repositories
{
    public class AesEncryptionService : IEncryptionService
    {
        // It must be a 32-character string
        private readonly string _encryptionKey;
        // AES block size is 16 bytes (128 bits)
        // IV value is all zeros for simplicity
        private readonly byte[] fixedIV = new byte[16];

        public AesEncryptionService(string encriptionKey)
        {
            _encryptionKey = encriptionKey;
        }

        public string Encrypt(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText))
            {
                throw new ArgumentNullException(nameof(sourceText), "Source text cannot be null or empty.");
            }

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_encryptionKey);
                // Use a fixed IV (constant IV for deterministic encryption)
                var iv = fixedIV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var memoryStream = new MemoryStream())
                {
                    // Prepend the IV to the encrypted content
                    memoryStream.Write(iv, 0, iv.Length);

                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(sourceText);
                    }

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText), "Encrypted text cannot be null or empty.");

            var fullEncryptedText = Convert.FromBase64String(encryptedText);

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_encryptionKey);
                var iv = fixedIV;

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var memoryStream = new MemoryStream(fullEncryptedText, iv.Length, fullEncryptedText.Length - iv.Length))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Helper method to create a secret key
        /// </summary>
        /// <returns></returns>
        private string CreateSecretKey()
        {
            byte[] keyBytes = RandomNumberGenerator.GetBytes(32); // 32 bytes for AES-256
            return Convert.ToBase64String(keyBytes);
        }
    }
}
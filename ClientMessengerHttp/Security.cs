using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ClientMessengerHttp
{
    internal class Security
    {
        private static readonly Aes _aes;

        static Security()
        {
            _aes = Aes.Create();
            _aes.GenerateIV();
            _aes.GenerateKey();
        }

        internal static async Task SendAes(ClientWebSocket server, JsonElement root)
        {
            _ = Logger.LogAsync("Sending aes");
            var publicKey = new RSAParameters()
            {
                Modulus = root.GetProperty("modulus").GetBytesFromBase64(),
                Exponent = root.GetProperty("exponent").GetBytesFromBase64()
            };

            var payload = new
            {
                code = OpCode.SendAes,
                key = Convert.ToBase64String(_aes.Key),
                iv = Convert.ToBase64String(_aes.IV),
            };
            var jsonString = JsonSerializer.Serialize(payload);
            await Client.SendPayloadAsync(server, jsonString, publicKey);
        }

        #region Encryption

        internal static byte[] EncryptAes(string dataToEncrypt)
        {
            using (_aes)
            {
                ICryptoTransform encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using var sw = new StreamWriter(cs);
                {
                    sw.Write(dataToEncrypt);
                    sw.Flush();
                }

                return ms.ToArray();
            }
        }

        internal static byte[] EncryptRSA(RSAParameters key, string dataToEncrypt)
        {
            ArgumentNullException.ThrowIfNull(key);
            byte[] encryptedData;

            using (var rsa = RSA.Create())
            {
                rsa.ImportParameters(key);
                var dataBytes = Encoding.UTF8.GetBytes(dataToEncrypt);
                encryptedData = rsa.Encrypt(dataBytes, RSAEncryptionPadding.Pkcs1);
            }

            return encryptedData;
        }


        #endregion

        #region Decryption

        internal static JsonElement? DecyrptMessage(string messageToDecyrpt)
        {
            var dataAsBytes = Convert.FromBase64String(messageToDecyrpt);
            try
            {
                return JsonDocument.Parse(dataAsBytes).RootElement;
            }
            catch (Exception)
            {
                return DecryptAes(dataAsBytes);
            }
        }

        private static JsonElement? DecryptAes(byte[] dataToDecrypt)
        {
            try
            {
                using ICryptoTransform decryptor = _aes.CreateDecryptor();
                {
                    byte[] decryptedData;

                    using var memoryStream = new MemoryStream(dataToDecrypt);
                    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    using var resultStream = new MemoryStream();
                    {
                        cryptoStream.CopyTo(resultStream);
                        decryptedData = resultStream.ToArray();
                    }
                    return JsonSerializer.Deserialize<JsonElement>(decryptedData);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;

            }
        }
    }

    #endregion
}

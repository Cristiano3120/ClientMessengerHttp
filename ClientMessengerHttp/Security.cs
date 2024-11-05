using System.Net.WebSockets;
using System.Security.Cryptography;
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
            var payload = new
            {
                code = OpCode.SendAes,
                key = Convert.ToBase64String(_aes.Key),
                iv = Convert.ToBase64String(_aes.IV),
            };
            var jsonString = JsonSerializer.Serialize(payload);
            await Client.SendPayloadAsync(server, jsonString);

        }

        #region Encryption

        internal static void EncryptRSA()
        {

        }

        #endregion
    }
}

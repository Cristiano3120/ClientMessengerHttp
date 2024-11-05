using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
namespace ClientMessengerHttp
{
    internal static class Client
    {
        internal static async Task Start()
        {
            _ = Logger.LogAsync("Starting!");
            CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Connecting:
            var server = new ClientWebSocket();
            try
            {
                throw new NotImplementedException("Mach das die file paths dynamisch sind!");
                await server.ConnectAsync(GetServerAdress(), cancellationToken);
                _ = Logger.LogAsync("Connected sucessfully to the server!");
                _ = Task.Run(() => ReceiveMessages(server, cancellationToken));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                _ = Logger.LogAsync("Retrying in 3 seconds");
                await Task.Delay(3000);
                goto Connecting;
            }
        }

        private static async Task ReceiveMessages(ClientWebSocket server, CancellationToken cancellationToken)
        {
            var buffer = new byte[65536];
            var completeMessage = new StringBuilder();
            while (server.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult receivedData = await server.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    var receivedDataAsString = Encoding.UTF8.GetString(buffer, 0, receivedData.Count);
                    completeMessage.Append(receivedDataAsString);
                    JsonElement message;

                    if (receivedData.EndOfMessage)
                    {
                        message = JsonDocument.Parse(completeMessage.ToString()).RootElement;
                        _ = Logger.LogAsync(message.ToString());
                        completeMessage.Clear();
                        OpCode opCode = message.GetProperty("code").GetOpCode();
                        _ = HandleMessage(server, opCode, message);
                    }
                    else
                    {
                        _ = Logger.LogAsync("The message is being sent in parts. Waiting for the next part");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }  
            }
        }

        private static async Task HandleMessage(ClientWebSocket server, OpCode opCode, JsonElement root)
        {
            try
            {
                _ = Logger.LogAsync($"Received opCode {opCode}");
                switch (opCode)
                {
                    case OpCode.ReceiveRSA:
                        await Security.SendAes(server, root);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        internal static async Task SendPayloadAsync(WebSocket server, string payload, EncryptionMode encryptionMode = EncryptionMode.Aes, bool endOfMessage = true)
        {
            ArgumentNullException.ThrowIfNull(payload);
            if (server.State != WebSocketState.Open)
            {
                return;
            }

            _ = Logger.LogAsync($"Sending: {payload}");

            var buffer = encryptionMode == EncryptionMode.RSA
                ? Encoding.UTF8.GetBytes(payload)
                : throw new NotImplementedException("Encrypt data with Aes");

            await server.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, endOfMessage, CancellationToken.None);
        }

        internal static Uri GetServerAdress()
        {
            var streamReader = new StreamReader("C:\\Users\\Crist\\source\\repos\\ClientMessengerHttp\\ClientMessengerHttp\\NeededFiles\\ServerAdress.txt");
            return new Uri(streamReader.ReadToEnd());
        }
    }
}

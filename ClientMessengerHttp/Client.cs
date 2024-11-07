using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
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
                //throw new NotImplementedException("Mach das die file paths dynamisch sind!");
                await server.ConnectAsync(GetServerAdress(true), cancellationToken);
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
                    _ = Logger.LogAsync($"RECEIVED: The received payload is {receivedData.Count} bytes long");
                    var receivedDataAsString = Convert.ToBase64String(buffer, 0, receivedData.Count);
                    completeMessage.Append(receivedDataAsString);
                    JsonElement message;

                    if (receivedData.EndOfMessage)
                    {
                        message = Security.DecyrptMessage(completeMessage.ToString());
                        completeMessage.Clear();
                        _ = Logger.LogAsync($"RECEIVED: {message}");
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
            await Restart();
        }

        private static async Task HandleMessage(ClientWebSocket server, OpCode opCode, JsonElement root)
        {
            try
            {
                _ = Logger.LogAsync($"RECEIVED: Received opCode {opCode}");
                switch (opCode)
                {
                    case OpCode.ReceiveRSA:
                        await Security.SendAes(server, root);
                        break;
                    case OpCode.ServerReadyToReceive:
                        Console.WriteLine("READY");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        /// <summary>
        /// Called when connection to the Server is lost
        /// </summary>
        private static async Task Restart()
        {
            await Task.Delay(3000);
            _ = Logger.LogAsync("Lost connection to the Server. Restarting");
            _ = Start();
        }

        /// <summary>
        /// Encrypts with aes
        /// </summary>
        internal static async Task SendPayloadAsync(WebSocket server, string payload)
        {
            _ = Logger.LogAsync($"SENDING(Aes): {payload}");
            ArgumentNullException.ThrowIfNull(payload);
            if (server.State != WebSocketState.Open)
            {
                return;
            }

            var buffer = Security.EncryptAes(payload);

            var bufferLengthOfServer = 65536;
            var parts = (int)Math.Ceiling((double)buffer.Length / bufferLengthOfServer);

            if (parts > 1)
            {
                var partedBuffer = buffer.Chunk(bufferLengthOfServer).ToArray();
                for (var i = 0; i < partedBuffer.Length; i++)
                {
                    var item = partedBuffer[i];
                    var endOfMessage = i == partedBuffer.Length - 1;

                    await server.SendAsync(new ArraySegment<byte>(item), WebSocketMessageType.Binary, endOfMessage, CancellationToken.None);
                }
            }
            else
            {
                await server.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }


        /// <summary>
        /// Encrypts with RSA
        /// </summary>
        /// <param name="rsaKey">The public key needed to encrypt</param>
        internal static async Task SendPayloadAsync(WebSocket server, string payload, RSAParameters rsaKey)
        {
            _ = Logger.LogAsync($"SENDING(RSA): {payload}");
            ArgumentNullException.ThrowIfNull(payload);
            if (server.State != WebSocketState.Open)
            {
                return;
            }

            var buffer = Security.EncryptRSA(rsaKey, payload);

            var bufferLengthOfServer = 65536;
            var parts = (int)Math.Ceiling((double)buffer.Length / bufferLengthOfServer);

            if (parts > 1)
            {
                var partedBuffer = buffer.Chunk(bufferLengthOfServer).ToArray();
                for (var i = 0; i < partedBuffer.Length; i++)
                {
                    var item = partedBuffer[i];
                    var endOfMessage = i == partedBuffer.Length - 1;

                    await server.SendAsync(new ArraySegment<byte>(item), WebSocketMessageType.Binary, endOfMessage, CancellationToken.None);
                }
            }
            else
            {
                await server.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }

        internal static Uri GetServerAdress(bool testing)
        {
            if (testing)
            {
                return new Uri("ws://127.0.0.1:5000/");
            }
            else
            {
                var streamReader = new StreamReader("C:\\Users\\Crist\\source\\repos\\ClientMessengerHttp\\ClientMessengerHttp\\NeededFiles\\ServerAdress.txt");
                return new Uri(streamReader.ReadToEnd());
            }
        }
    }
}

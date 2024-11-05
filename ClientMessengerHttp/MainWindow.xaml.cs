using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace ClientMessengerHttp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            _ = Client.Start();
        }

        //private static async Task Test()
        //{
        //    using (ClientWebSocket client = new())
        //    {
        //        try
        //        {
        //            string serverUrl = GetserverWebAdress();
        //            // Verbindung zum WebSocket-server herstellen
        //            await client.ConnectAsync(new Uri(serverUrl), CancellationToken.None);
        //            Console.WriteLine("Verbunden mit dem server!");

        //            // Erstelle die JSON-Nachricht
        //            var jsonMessage = "{\"Code\": 0}";
        //            var messageBuffer = Encoding.UTF8.GetBytes(jsonMessage);

        //            // Sende die JSON-Nachricht
        //            await client.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        //            Console.WriteLine("Nachricht gesendet: " + jsonMessage);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Fehler: " + ex.Message);
        //        }
        //    }
        //}


        //private static async Task SendJsonAsync(string url, string json)
        //{
        //    // Erstelle den StringContent für die POST-Anfrage
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    try
        //    {
        //        // Sende die POST-Anfrage
        //        HttpResponseMessage response = await httpClient.PostAsync(url, content);

        //        // Überprüfe den Statuscode der Antwort
        //        if (response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine("Die JSON-Nachricht wurde erfolgreich gesendet.");
        //            var responseContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine("Antwort vom server: " + responseContent);
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Fehler beim Senden der JSON-Nachricht. Statuscode: {response.StatusCode}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
        //    }
        //}
    }
}
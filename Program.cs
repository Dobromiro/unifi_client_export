using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UniFiClientExport
{
    class Program
    {
        private static readonly string site = "default";
        private static readonly HttpClient client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true });

        static async Task Main(string[] args)
        {
            Console.Write("Podaj adres IP kontrolera (np. https://192.168.230.234:8443): ");
            string controllerUrl = Console.ReadLine();

            Console.Write("Podaj nazwę użytkownika: ");
            string username = Console.ReadLine();

            Console.Write("Podaj hasło: ");
            string password = Console.ReadLine();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Logowanie
            var loginData = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            var loginResponse = await client.PostAsync($"{controllerUrl}/api/login", new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json"));
            loginResponse.EnsureSuccessStatusCode();

            // Pobieranie listy klientów
            var clientsResponse = await client.GetAsync($"{controllerUrl}/api/s/{site}/stat/sta");
            clientsResponse.EnsureSuccessStatusCode();

            var responseBody = await clientsResponse.Content.ReadAsStringAsync();
            var clients = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseBody)["data"];

            // Zapisywanie do pliku CSV
            using (var file = new StreamWriter("clients.csv"))
            {
                file.WriteLine("MAC Address,IP Address,Hostname");

                foreach (var client in clients)
                {
                    string mac = client["mac"];
                    string ip = client["ip"];
                    string hostname = client.ContainsKey("hostname") ? client["hostname"] : "";
                    file.WriteLine($"{mac},{ip},{hostname}");
                }
            }

            Console.WriteLine("Lista użytkowników została wyeksportowana do pliku clients.csv");
        }
    }
}

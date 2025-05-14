using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace McpTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MCP Server Test Client");
            Console.WriteLine("=====================");

            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3001/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Test Echo
            Console.WriteLine("\nTesting Echo...");
            await TestEcho(client);

            // Test Initialize
            Console.WriteLine("\nTesting Initialize...");
            await TestInitialize(client);

            Console.WriteLine("\nTests completed.");
        }

        static async Task TestEcho(HttpClient client)
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "echo",
                    @params = new
                    {
                        message = "Hello from .NET test client!"
                    },
                    id = 1
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Success! Response: {responseBody}");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        static async Task TestInitialize(HttpClient client)
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "initialize",
                    @params = new
                    {
                        connectionName = "DefaultConnection"
                    },
                    id = 2
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Success! Response: {responseBody}");
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}

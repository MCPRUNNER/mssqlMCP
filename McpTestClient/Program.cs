using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

            // Test GetTableMetadata - All schemas
            Console.WriteLine("\nTesting GetTableMetadata (all schemas)...");
            await TestGetTableMetadata(client);

            // Test GetTableMetadata - Specific schema (dbo)
            Console.WriteLine("\nTesting GetTableMetadata with 'dbo' schema filter...");
            await TestGetTableMetadataWithSchema(client, "dbo");

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

        private static async Task TestGetTableMetadata(HttpClient client)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "getTableMetadata",
                @params = new
                {
                    connectionName = "DefaultConnection"
                },
                id = 3
            };

            var response = await SendRequest(client, request);
            if (response != null)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(response.result.ToString());
                Console.WriteLine($"Success! Retrieved metadata for {result.EnumerateArray().Count()} tables.");

                // Display schema information
                var schemas = new HashSet<string>();
                foreach (var table in result.EnumerateArray())
                {
                    if (table.TryGetProperty("Schema", out JsonElement schemaElement))
                    {
                        schemas.Add(schemaElement.GetString() ?? "Unknown");
                    }
                }

                Console.WriteLine($"Schemas found: {string.Join(", ", schemas)}");
            }
        }

        private static async Task TestGetTableMetadataWithSchema(HttpClient client, string schema)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "getTableMetadata",
                @params = new
                {
                    connectionName = "DefaultConnection",
                    schema = schema
                },
                id = 4
            };

            var response = await SendRequest(client, request);
            if (response != null)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(response.result.ToString());
                Console.WriteLine($"Success! Retrieved metadata for {result.EnumerateArray().Count()} tables in '{schema}' schema.");

                // Display some table names
                var tableNames = new List<string>();
                foreach (var table in result.EnumerateArray().Take(5))
                {
                    if (table.TryGetProperty("Name", out JsonElement nameElement))
                    {
                        tableNames.Add(nameElement.GetString() ?? "Unknown");
                    }
                }

                Console.WriteLine($"Sample tables: {string.Join(", ", tableNames)}");
            }
        }
        private static async Task<dynamic?> SendRequest(HttpClient client, object request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<dynamic>(responseBody);
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

            return null;
        }
    }
}

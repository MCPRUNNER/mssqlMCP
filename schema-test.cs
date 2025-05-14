using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

// Test client for the MCP server with schema filtering capability
Console.WriteLine("MCP Server Test Client - Schema Filtering Test");
Console.WriteLine("===============================================");

using var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:3001/");
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

// Test GetTableMetadata - All schemas
Console.WriteLine("\nTesting GetTableMetadata (all schemas)...");
await TestGetTableMetadata(client);

// Test GetTableMetadata - Specific schema (dbo)
Console.WriteLine("\nTesting GetTableMetadata with 'dbo' schema filter...");
await TestGetTableMetadataWithSchema(client, "dbo");

Console.WriteLine("\nTests completed.");

static async Task TestGetTableMetadata(HttpClient client)
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
        var jsonElement = JsonDocument.Parse(response.GetProperty("result").ToString()).RootElement;
        int tableCount = 0;

        // Count tables
        foreach (var _ in jsonElement.EnumerateArray())
        {
            tableCount++;
        }

        Console.WriteLine($"Success! Retrieved metadata for {tableCount} tables.");

        // Display schema information
        var schemas = new HashSet<string>();
        foreach (var table in jsonElement.EnumerateArray())
        {
            if (table.TryGetProperty("Schema", out JsonElement schemaElement))
            {
                schemas.Add(schemaElement.GetString() ?? "Unknown");
            }
        }

        Console.WriteLine($"Schemas found: {string.Join(", ", schemas)}");
    }
}

static async Task TestGetTableMetadataWithSchema(HttpClient client, string schema)
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
        var jsonElement = JsonDocument.Parse(response.GetProperty("result").ToString()).RootElement;
        int tableCount = 0;

        // Count tables
        foreach (var _ in jsonElement.EnumerateArray())
        {
            tableCount++;
        }

        Console.WriteLine($"Success! Retrieved metadata for {tableCount} tables in '{schema}' schema.");

        // Display some table names
        var tableNames = new List<string>();
        int count = 0;
        foreach (var table in jsonElement.EnumerateArray())
        {
            if (count >= 5) break;

            if (table.TryGetProperty("Name", out JsonElement nameElement))
            {
                tableNames.Add(nameElement.GetString() ?? "Unknown");
                count++;
            }
        }

        Console.WriteLine($"Sample tables: {string.Join(", ", tableNames)}");
    }
}

static async Task<JsonElement?> SendRequest(HttpClient client, object request)
{
    try
    {
        var jsonRequest = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseBody).RootElement;

        if (jsonResponse.TryGetProperty("error", out JsonElement errorElement))
        {
            Console.WriteLine($"Error: {errorElement}");
            return null;
        }

        return jsonResponse;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Request failed: {ex.Message}");
        return null;
    }
}

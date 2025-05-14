using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchemaFilteringExample
{
    class Program
    {
        /// <summary>
        /// Example demonstrating how to use schema filtering in the SQL MCP Server
        /// </summary>
        static async Task Main(string[] args)
        {
            Console.WriteLine("SQL MCP Server Schema Filtering Examples");
            Console.WriteLine("========================================\n");

            try
            {
                // Example 1: Get metadata for all schemas
                Console.WriteLine("Example 1: Get metadata for all schemas");
                var allSchemasMetadata = await GetTableMetadata();
                Console.WriteLine($"Retrieved metadata for all schemas: {allSchemasMetadata.Length} bytes\n");

                // Example 2: Get metadata for a specific database connection
                Console.WriteLine("Example 2: Get metadata for a specific database connection");
                var adventureWorksMetadata = await GetTableMetadata("AdventureWorks");
                Console.WriteLine($"Retrieved metadata for AdventureWorks: {adventureWorksMetadata.Length} bytes\n");

                // Example 3: Get metadata for a specific schema
                Console.WriteLine("Example 3: Get metadata for a specific schema (dbo)");
                var dboSchemaMetadata = await GetTableMetadata("DefaultConnection", "dbo");
                Console.WriteLine($"Retrieved metadata for dbo schema: {dboSchemaMetadata.Length} bytes\n");

                // Example 4: Get metadata for a specific schema in a specific database
                Console.WriteLine("Example 4: Get metadata for a specific schema in AdventureWorks (Sales)");
                var salesSchemaMetadata = await GetTableMetadata("AdventureWorks", "Sales");
                Console.WriteLine($"Retrieved metadata for Sales schema in AdventureWorks: {salesSchemaMetadata.Length} bytes\n");

                // Example 5: Format and display filtered metadata
                Console.WriteLine("Example 5: Format and display filtered metadata");
                var tableInfo = JsonSerializer.Deserialize<dynamic>(dboSchemaMetadata);
                Console.WriteLine("Tables in dbo schema:");
                foreach (var table in tableInfo)
                {
                    Console.WriteLine($"- {table.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mock implementation of GetTableMetadata that simulates the MCP Server function
        /// In a real application, this would be an MCP call to the server
        /// </summary>
        /// <param name="connectionName">The connection name to use</param>
        /// <param name="schema">Optional schema name to filter by</param>
        /// <returns>JSON string containing table metadata</returns>
        static async Task<string> GetTableMetadata(string connectionName = "DefaultConnection", string schema = null)
        {
            // In a real application, this would call the MCP server
            Console.WriteLine($"  > Calling GetTableMetadata({connectionName}, {schema ?? "null"})");

            // Simulate network delay
            await Task.Delay(500);

            // Return a simulated response based on the parameters
            if (schema == null)
            {
                return "{\"tables\":[{\"Schema\":\"dbo\",\"Name\":\"Customer\"},{\"Schema\":\"dbo\",\"Name\":\"Product\"},{\"Schema\":\"sales\",\"Name\":\"Order\"},{\"Schema\":\"hr\",\"Name\":\"Employee\"}]}";
            }
            else
            {
                // Filter by schema
                if (schema.Equals("dbo", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\"tables\":[{\"Schema\":\"dbo\",\"Name\":\"Customer\"},{\"Schema\":\"dbo\",\"Name\":\"Product\"}]}";
                }
                else if (schema.Equals("sales", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\"tables\":[{\"Schema\":\"sales\",\"Name\":\"Order\"}]}";
                }
                else if (schema.Equals("hr", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\"tables\":[{\"Schema\":\"hr\",\"Name\":\"Employee\"}]}";
                }
                else
                {
                    return "{\"tables\":[]}";
                }
            }
        }
    }
}

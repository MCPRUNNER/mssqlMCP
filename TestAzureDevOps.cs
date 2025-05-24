using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mssqlMCP.Interfaces;
using mssqlMCP.Services;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mssqlMCP.Tests;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Azure DevOps MCP functionality...");

        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Configure services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSerilog());
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<IDatabaseMetadataProvider, DatabaseMetadataProvider>();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Get the database metadata provider
            var metadataProvider = serviceProvider.GetRequiredService<IDatabaseMetadataProvider>();
            // Test the P330_AzureDevOps_RSYSLAB connection
            string? connectionString = configuration.GetConnectionString("P330_AzureDevOps_RSYSLAB");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ERROR: P330_AzureDevOps_RSYSLAB connection string not found");
                return;
            }

            Console.WriteLine($"Using connection: {connectionString}");

            // Create a new provider instance with the specific connection string
            var azureDevOpsProvider = new DatabaseMetadataProvider(connectionString, serviceProvider.GetRequiredService<ILogger<DatabaseMetadataProvider>>());

            Console.WriteLine("Testing Azure DevOps data retrieval...");
            var azureDevOpsInfo = await azureDevOpsProvider.GetAzureDevOpsInfoAsync();

            if (azureDevOpsInfo != null)
            {
                Console.WriteLine("SUCCESS: Azure DevOps information retrieved!");
                Console.WriteLine($"Projects found: {azureDevOpsInfo.Projects.Count}");
                Console.WriteLine($"Repositories found: {azureDevOpsInfo.Repositories.Count}");
                Console.WriteLine($"Build definitions: {azureDevOpsInfo.BuildDefinitionCount}");
                Console.WriteLine($"Work items: {azureDevOpsInfo.WorkItemCount}");

                // Show first few projects
                if (azureDevOpsInfo.Projects.Count > 0)
                {
                    Console.WriteLine("\nFirst few projects:");
                    foreach (var project in azureDevOpsInfo.Projects.Take(5))
                    {
                        Console.WriteLine($"  - {project.Name} (ID: {project.ProjectId})");
                    }
                }

                // Show first few repositories
                if (azureDevOpsInfo.Repositories.Count > 0)
                {
                    Console.WriteLine("\nFirst few repositories:");
                    foreach (var repo in azureDevOpsInfo.Repositories.Take(5))
                    {
                        Console.WriteLine($"  - {repo.Name} in {repo.ProjectName} (Branches: {repo.BranchCount}, Commits: {repo.CommitCount})");
                    }
                }
            }
            else
            {
                Console.WriteLine("ERROR: Could not retrieve Azure DevOps information");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            serviceProvider.Dispose();
            Log.CloseAndFlush();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}

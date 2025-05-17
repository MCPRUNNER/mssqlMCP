# Migrate-To-Encrypted.ps1
# Script to migrate unencrypted connection strings to encrypted format

Write-Host "SQL Server MCP - Migration to Encrypted Connections" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green
Write-Host ""

# Verify if current key is set
if (-not $env:MSSQL_MCP_KEY) {
    Write-Host "WARNING: Encryption key (MSSQL_MCP_KEY) is not set in the environment." -ForegroundColor Yellow
    Write-Host "Connections will be encrypted with a default insecure key." -ForegroundColor Yellow
    Write-Host "For better security, set a strong encryption key:" -ForegroundColor Yellow
    Write-Host '$env:MSSQL_MCP_KEY = "your-strong-random-key"' -ForegroundColor Cyan
    
    $proceed = Read-Host "Do you want to proceed without a secure key? (y/n)"
    if ($proceed -ne 'y') {
        Write-Host "Operation cancelled." -ForegroundColor Red
        exit 0
    }
}

Write-Host "This operation will encrypt any unencrypted connection strings in the database." -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Do you want to proceed? (y/n)"
if ($confirmation -ne 'y') {
    Write-Host "Operation cancelled." -ForegroundColor Red
    exit 0
}

# Check if we're in a publish folder or development environment
$dllPath = ""
if (Test-Path "./mssqlMCP.dll") {
    # Published version
    $dllPath = "./mssqlMCP.dll"
}
elseif (Test-Path "./bin/Debug/net9.0/mssqlMCP.dll") {
    # Development version
    $dllPath = "./bin/Debug/net9.0/mssqlMCP.dll"
}
else {
    Write-Host "ERROR: Could not find mssqlMCP.dll. Make sure you're running this script from the correct directory." -ForegroundColor Red
    exit 1
}

try {
    Write-Host "Starting migration process..." -ForegroundColor Green
    
    # Build a small C# program to run the migration
    $tempScriptPath = [System.IO.Path]::GetTempFileName() + ".cs"
    
    $scriptContent = @"
using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

// The tool assembly namespace
namespace MigrationTool
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            string dllPath = args[0];
            
            try
            {
                // Load the MCP assembly dynamically
                var assembly = Assembly.LoadFrom(dllPath);
                
                // Get the KeyRotationService type
                var keyRotationServiceType = assembly.GetType("mssqlMCP.Services.KeyRotationService");
                var encryptionServiceType = assembly.GetType("mssqlMCP.Services.EncryptionService");
                var connectionRepoType = assembly.GetType("mssqlMCP.Services.SqliteConnectionRepository");
                var encryptionInterfaceType = assembly.GetType("mssqlMCP.Services.IEncryptionService");
                var repoInterfaceType = assembly.GetType("mssqlMCP.Services.IConnectionRepository");
                
                if (keyRotationServiceType == null || encryptionServiceType == null || 
                    connectionRepoType == null || encryptionInterfaceType == null || repoInterfaceType == null)
                {
                    Console.WriteLine("Error: Could not find required types in assembly");
                    return 1;
                }
                
                // Create a ServiceCollection and register required services
                var services = new ServiceCollection();
                
                // Add logging
                services.AddLogging(configure => configure.AddConsole());
                
                // Register encryption service
                services.AddSingleton(encryptionInterfaceType, encryptionServiceType);
                
                // Register connection repository
                services.AddSingleton(repoInterfaceType, connectionRepoType);
                
                // Register key rotation service
                var keyRotationInterfaceType = assembly.GetType("mssqlMCP.Services.IKeyRotationService");
                services.AddSingleton(keyRotationInterfaceType, keyRotationServiceType);
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Get the key rotation service
                var keyRotationService = serviceProvider.GetService(keyRotationInterfaceType);
                
                if (keyRotationService == null)
                {
                    Console.WriteLine("Error: Failed to create key rotation service");
                    return 1;
                }
                
                // Get the MigrateUnencryptedConnectionsAsync method
                var migrateMethod = keyRotationServiceType.GetMethod("MigrateUnencryptedConnectionsAsync");
                if (migrateMethod == null)
                {
                    Console.WriteLine("Error: Could not find MigrateUnencryptedConnectionsAsync method");
                    return 1;
                }
                
                // Invoke the method
                var task = (Task)migrateMethod.Invoke(keyRotationService, new object[] { });
                await task;
                
                // Get the result (count of migrated connections)
                var resultProperty = task.GetType().GetProperty("Result");
                var count = (int)resultProperty.GetValue(task);
                
                Console.WriteLine($"Successfully encrypted {count} connection strings");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}
"@
    
    # Save the script
    [System.IO.File]::WriteAllText($tempScriptPath, $scriptContent)
    
    # Compile the script
    dotnet new console -o MigrationTemp --no-restore
    dotnet add MigrationTemp package Microsoft.Extensions.DependencyInjection
    dotnet add MigrationTemp package Microsoft.Extensions.Logging
    dotnet add MigrationTemp package Microsoft.Extensions.Logging.Console
    Copy-Item $tempScriptPath MigrationTemp/Program.cs
    dotnet build MigrationTemp -o MigrationTemp/bin
    
    # Run the compiled assembly
    dotnet MigrationTemp/bin/MigrationTemp.dll $dllPath
    
    # Clean up
    Remove-Item -Recurse -Force MigrationTemp
    Remove-Item $tempScriptPath
}
catch {
    Write-Host "ERROR: An error occurred during migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

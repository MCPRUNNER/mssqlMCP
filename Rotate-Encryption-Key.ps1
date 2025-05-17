# Rotate-Encryption-Key.ps1
# Script to rotate the encryption key for SQL Server MCP connection strings

param (
    [Parameter(Mandatory = $false)]
    [string]$NewKey
)

Write-Host "SQL Server MCP - Encryption Key Rotation Tool" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host ""

# Verify if current key is set
if (-not $env:MSSQL_MCP_KEY) {
    Write-Host "ERROR: Current encryption key (MSSQL_MCP_KEY) is not set in the environment." -ForegroundColor Red
    Write-Host "Please set the current encryption key first:" -ForegroundColor Yellow
    Write-Host '$env:MSSQL_MCP_KEY = "your-current-key"' -ForegroundColor Cyan
    exit 1
}

# Generate a random key if not provided
if (-not $NewKey) {
    $keyLength = 32
    $randomBytes = New-Object byte[] $keyLength
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($randomBytes)
    $NewKey = [Convert]::ToBase64String($randomBytes)
    
    Write-Host "Generated a new random encryption key." -ForegroundColor Yellow
}

Write-Host "Current key: [PROTECTED]"
Write-Host "New key: $NewKey" -ForegroundColor Cyan
Write-Host ""
Write-Host "This operation will re-encrypt all connection strings with the new key." -ForegroundColor Yellow
Write-Host "The application must be restarted with the new key set as MSSQL_MCP_KEY after this operation." -ForegroundColor Yellow
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
    Write-Host "Starting key rotation process..." -ForegroundColor Green
    
    # Build a small C# program to run the key rotation
    $tempScriptPath = [System.IO.Path]::GetTempFileName() + ".cs"
    $tempDllPath = [System.IO.Path]::GetTempFileName() + ".dll"
    
    $scriptContent = @"
using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

// The tool assembly namespace
namespace KeyRotationTool
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Error: New key argument is required");
                return 1;
            }

            string newKey = args[0];
            string dllPath = args[1];
            
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
                
                // Get the RotateKeyAsync method
                var rotateKeyMethod = keyRotationServiceType.GetMethod("RotateKeyAsync");
                if (rotateKeyMethod == null)
                {
                    Console.WriteLine("Error: Could not find RotateKeyAsync method");
                    return 1;
                }
                
                // Invoke the method
                var task = (Task)rotateKeyMethod.Invoke(keyRotationService, new object[] { newKey });
                await task;
                
                // Get the result (count of rotated connections)
                var resultProperty = task.GetType().GetProperty("Result");
                var count = (int)resultProperty.GetValue(task);
                
                Console.WriteLine($"Successfully rotated encryption key for {count} connection strings");
                Console.WriteLine();
                Console.WriteLine("IMPORTANT: You must now set the new encryption key in your environment:");
                Console.WriteLine($"$env:MSSQL_MCP_KEY = \"{newKey}\"");
                Console.WriteLine();
                Console.WriteLine("Then restart the MCP server with the new key.");
                
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
    dotnet new console -o KeyRotationTemp --no-restore
    dotnet add KeyRotationTemp package Microsoft.Extensions.DependencyInjection
    dotnet add KeyRotationTemp package Microsoft.Extensions.Logging
    dotnet add KeyRotationTemp package Microsoft.Extensions.Logging.Console
    Copy-Item $tempScriptPath KeyRotationTemp/Program.cs
    dotnet build KeyRotationTemp -o KeyRotationTemp/bin
    
    # Run the compiled assembly
    dotnet KeyRotationTemp/bin/KeyRotationTemp.dll $NewKey $dllPath
    
    # Clean up
    Remove-Item -Recurse -Force KeyRotationTemp
    Remove-Item $tempScriptPath
}
catch {
    Write-Host "ERROR: An error occurred during key rotation:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

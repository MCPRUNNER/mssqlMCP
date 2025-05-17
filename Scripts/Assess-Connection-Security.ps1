# Assess-Connection-Security.ps1
# Script to assess security status of SQL Server MCP connections

Write-Host "SQL Server MCP - Connection Security Assessment" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Helper function for MCP API calls
function Invoke-McpApi {
    param (
        [string]$Method,
        [object]$Params = @{}
    )
    
    try {
        $body = @{
            jsonrpc = "2.0"
            id      = [guid]::NewGuid().ToString()
            method  = $Method
            params  = $Params
        } | ConvertTo-Json -Depth 10
        
        $headers = @{
            "Content-Type" = "application/json"
            "Accept"       = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "http://localhost:3001/" -Method Post -Body $body -Headers $headers
        return $response
    }
    catch {
        Write-Host "Error calling MCP API: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseText = $reader.ReadToEnd()
            Write-Host "Response content: $responseText" -ForegroundColor Red
        }
        throw
    }
}

# Assess current connections
try {
    Write-Host "Checking connection security status..." -ForegroundColor Cyan
    
    # Build a small C# program to assess connections
    $tempScriptPath = [System.IO.Path]::GetTempFileName() + ".cs"
    $tempDllPath = [System.IO.Path]::GetTempFileName() + ".dll"
    
    $scriptContent = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;

// The assessment tool namespace
namespace SecurityAssessment
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
                
                // Get required types
                var encryptionServiceType = assembly.GetType("mssqlMCP.Services.EncryptionService");
                var connectionRepoType = assembly.GetType("mssqlMCP.Services.SqliteConnectionRepository");
                var encryptionInterfaceType = assembly.GetType("mssqlMCP.Services.IEncryptionService");
                var repoInterfaceType = assembly.GetType("mssqlMCP.Services.IConnectionRepository");
                
                if (encryptionServiceType == null || connectionRepoType == null || 
                    encryptionInterfaceType == null || repoInterfaceType == null)
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
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Get the connection repository
                var connectionRepository = serviceProvider.GetService(repoInterfaceType);
                if (connectionRepository == null)
                {
                    Console.WriteLine("Error: Failed to create connection repository");
                    return 1;
                }
                
                // Get the IsEncrypted method from encryption service
                var encryptionService = serviceProvider.GetService(encryptionInterfaceType);
                var isEncryptedMethod = encryptionServiceType.GetMethod("IsEncrypted");
                
                if (encryptionService == null || isEncryptedMethod == null)
                {
                    Console.WriteLine("Error: Failed to access encryption service");
                    return 1;
                }
                
                // Get repository methods
                var getRawConnectionsMethod = connectionRepoType.GetMethod("GetAllConnectionsRawAsync");
                if (getRawConnectionsMethod == null)
                {
                    Console.WriteLine("Error: Could not find GetAllConnectionsRawAsync method");
                    return 1;
                }
                
                // Get all connections without decryption
                var task = (Task)getRawConnectionsMethod.Invoke(connectionRepository, new object[] { });
                await task;
                
                // Get the result (list of connections)
                var resultProperty = task.GetType().GetProperty("Result");
                var connections = (IEnumerable<dynamic>)resultProperty.GetValue(task);
                
                int total = 0;
                int encrypted = 0;
                int unencrypted = 0;
                var unencryptedList = new List<string>();
                
                // Assess each connection
                foreach (var connection in connections)
                {
                    total++;
                    var connectionString = (string)connection.GetType().GetProperty("ConnectionString").GetValue(connection);
                    var name = (string)connection.GetType().GetProperty("Name").GetValue(connection);
                    
                    bool isEncrypted = (bool)isEncryptedMethod.Invoke(encryptionService, new object[] { connectionString });
                    
                    if (isEncrypted)
                    {
                        encrypted++;
                    }
                    else
                    {
                        unencrypted++;
                        unencryptedList.Add(name);
                    }
                }
                
                // Output results
                Console.WriteLine($"Total connections: {total}");
                Console.WriteLine($"Encrypted: {encrypted}");
                Console.WriteLine($"Unencrypted: {unencrypted}");
                
                if (unencrypted > 0)
                {
                    Console.WriteLine("\\nUnencrypted connections:");
                    foreach (var name in unencryptedList)
                    {
                        Console.WriteLine($"- {name}");
                    }
                    
                    Console.WriteLine("\\nTo encrypt these connections, run:");
                    Console.WriteLine("./Migrate-To-Encrypted.ps1");
                }
                else
                {
                    Console.WriteLine("\\nAll connections are properly encrypted.");
                }
                
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
    
    # Save the script
    [System.IO.File]::WriteAllText($tempScriptPath, $scriptContent)
    
    # Compile the script
    dotnet new console -o AssessmentTemp --no-restore
    dotnet add AssessmentTemp package Microsoft.Extensions.DependencyInjection
    dotnet add AssessmentTemp package Microsoft.Extensions.Logging
    dotnet add AssessmentTemp package Microsoft.Extensions.Logging.Console
    Copy-Item $tempScriptPath AssessmentTemp/Program.cs
    dotnet build AssessmentTemp -o AssessmentTemp/bin
    
    # Run the compiled assembly
    dotnet AssessmentTemp/bin/AssessmentTemp.dll $dllPath
    
    # Clean up
    Remove-Item -Recurse -Force AssessmentTemp
    Remove-Item $tempScriptPath
    
    # Provide key status
    Write-Host "`nEncryption Key Status:" -ForegroundColor Cyan
    if ($env:MSSQL_MCP_KEY) {
        Write-Host "✅ MSSQL_MCP_KEY is set in the current environment" -ForegroundColor Green
    }
    else {
        Write-Host "❌ MSSQL_MCP_KEY is NOT set in the current environment" -ForegroundColor Red
        Write-Host "   Using default insecure key! Set a strong encryption key with:" -ForegroundColor Red
        Write-Host '   $env:MSSQL_MCP_KEY = "your-strong-random-key"' -ForegroundColor Cyan
    }
    
    # Generate a key option
    Write-Host "`nWould you like to generate a new secure encryption key? (y/n)" -ForegroundColor Cyan
    $genKey = Read-Host
    if ($genKey -eq "y") {
        try {
            $response = Invoke-McpApi -Method "security.generateSecureKey"
            $newKey = $response.result.key
            
            Write-Host "`nGenerated new secure key:" -ForegroundColor Green
            Write-Host $newKey -ForegroundColor Cyan
            Write-Host "`nTo use this key, set the environment variable:" -ForegroundColor Yellow
            Write-Host '$env:MSSQL_MCP_KEY = "' + $newKey + '"' -ForegroundColor Cyan
            
            $setNow = Read-Host "Would you like to set this key in the current environment? (y/n)"
            if ($setNow -eq "y") {
                $env:MSSQL_MCP_KEY = $newKey
                Write-Host "Key set in current environment. Remember to restart the MCP server." -ForegroundColor Green
            }
        }
        catch {
            Write-Host "Failed to generate key via MCP API. Server may not be running." -ForegroundColor Red
            Write-Host "You can still generate a key using the SecurityTool directly or use Start-MCP-Encrypted.ps1" -ForegroundColor Yellow
        }
    }
    
    Write-Host "`nSecurity assessment completed." -ForegroundColor Green
}
catch {
    Write-Host "ERROR: An error occurred during assessment:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Start-MCP-Encrypted-Local.ps1
# Script to start the SQL Server MCP server with connection string encryption and API security enabled

Write-Host "Starting SQL Server MCP with encrypted connection strings and API security..." -ForegroundColor Green

# Generate a random encryption key if not provided
if (-not $env:MSSQL_MCP_KEY) {
    $keyLength = 32
    $randomBytes = New-Object byte[] $keyLength
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($randomBytes)
    $encryptionKey = [Convert]::ToBase64String($randomBytes)
    
    Write-Host "Generated a random encryption key." -ForegroundColor Yellow
    Write-Host "IMPORTANT: To ensure persistent encryption/decryption across restarts, save this key and set it as MSSQL_MCP_KEY in your environment." -ForegroundColor Yellow
    Write-Host "You can add this to your environment variables with: " -ForegroundColor Yellow
    Write-Host "`$env:MSSQL_MCP_KEY = `"$encryptionKey`"" -ForegroundColor Cyan
    Write-Host "For production use, set this permanently in your system environment variables." -ForegroundColor Yellow
    
    # Set for the current session
    $env:MSSQL_MCP_KEY = $encryptionKey
}
else {
    Write-Host "Using existing MSSQL_MCP_KEY from environment." -ForegroundColor Cyan
}

# Set up API key if not provided
if (-not $env:MSSQL_MCP_API_KEY) {
    $apiKeyLength = 48
    $apiRandomBytes = New-Object byte[] $apiKeyLength
    $apiRng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $apiRng.GetBytes($apiRandomBytes)
    $apiKey = [Convert]::ToBase64String($apiRandomBytes)
    
    Write-Host "`nGenerated a random API key." -ForegroundColor Yellow
    Write-Host "IMPORTANT: This key will be required for all API requests to the server." -ForegroundColor Yellow
    Write-Host "You can add this to your environment variables with: " -ForegroundColor Yellow
    Write-Host "`$env:MSSQL_MCP_API_KEY = `"$apiKey`"" -ForegroundColor Cyan
    
    # Set for the current session
    $env:MSSQL_MCP_API_KEY = $apiKey
    
    # Update appsettings.json if it exists
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    $rootPath = Split-Path -Parent $scriptPath
    $appSettingsPath = Join-Path $rootPath "appsettings.json"
    
    if (Test-Path $appSettingsPath) {
        try {
            $appSettings = Get-Content -Path $appSettingsPath -Raw | ConvertFrom-Json
            
            # Ensure ApiSecurity section exists
            if (-not $appSettings.ApiSecurity) {
                $appSettings | Add-Member -MemberType NoteProperty -Name "ApiSecurity" -Value ([PSCustomObject]@{})
            }
            
            # Set or update the ApiKey property
            if ($appSettings.ApiSecurity.PSObject.Properties.Name -contains "ApiKey") {
                $appSettings.ApiSecurity.ApiKey = $apiKey
            }
            else {
                $appSettings.ApiSecurity | Add-Member -MemberType NoteProperty -Name "ApiKey" -Value $apiKey
            }
            
            # Set or update the HeaderName property if it doesn't exist
            if (-not ($appSettings.ApiSecurity.PSObject.Properties.Name -contains "HeaderName")) {
                $appSettings.ApiSecurity | Add-Member -MemberType NoteProperty -Name "HeaderName" -Value "X-API-Key"
            }
            
            # Save the updated settings
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content -Path $appSettingsPath
            Write-Host "API key saved to appsettings.json" -ForegroundColor Green
        }
        catch {
            Write-Host "Warning: Could not update appsettings.json - $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host "The API key is still set in the environment variable for the current session." -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "Using existing MSSQL_MCP_API_KEY from environment." -ForegroundColor Cyan
}

# Check if we're in a publish folder or development environment
if (Test-Path "./mssqlMCP.dll") {
    # Published version
    Write-Host "Starting published version..." -ForegroundColor Green
    dotnet mssqlMCP.dll
}
else {
    # Development version
    Write-Host "Starting development version..." -ForegroundColor Green
    dotnet run
}

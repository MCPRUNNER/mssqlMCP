# Set-Api-Key.ps1
# Script to generate and set an API key for SQL Server MCP server

Write-Host "SQL Server MCP - API Key Generator" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Get the path to the appsettings.json file
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$appSettingsPath = Join-Path $rootPath "appsettings.json"

# Helper function to set and configure the API key
function SetAndConfigureKey {
    param (
        [string]$ApiKey
    )
    
    # Set the environment variable for the current session
    $env:MSSQL_MCP_API_KEY = $ApiKey
    Write-Host "API key set in current environment variable MSSQL_MCP_API_KEY" -ForegroundColor Green
    
    # Update appsettings.json if it exists
    if (Test-Path $appSettingsPath) {
        try {
            $appSettings = Get-Content -Path $appSettingsPath -Raw | ConvertFrom-Json
            
            # Ensure ApiSecurity section exists
            if (-not $appSettings.ApiSecurity) {
                $appSettings | Add-Member -MemberType NoteProperty -Name "ApiSecurity" -Value ([PSCustomObject]@{})
            }
            
            # Set or update the ApiKey property
            if ($appSettings.ApiSecurity.PSObject.Properties.Name -contains "ApiKey") {
                $appSettings.ApiSecurity.ApiKey = $ApiKey
            }
            else {
                $appSettings.ApiSecurity | Add-Member -MemberType NoteProperty -Name "ApiKey" -Value $ApiKey
            }
            
            # Set or update the HeaderName property if it doesn't exist
            if (-not ($appSettings.ApiSecurity.PSObject.Properties.Name -contains "HeaderName")) {
                $appSettings.ApiSecurity | Add-Member -MemberType NoteProperty -Name "HeaderName" -Value "X-API-Key"
            }
            
            # Save the updated settings
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content -Path $appSettingsPath
            Write-Host "API key also saved to appsettings.json" -ForegroundColor Green
        }
        catch {
            Write-Host "Warning: Could not update appsettings.json - $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host "The API key is still set in the environment variable for the current session." -ForegroundColor Yellow
        }
    }
    
    Write-Host "`nTo set this key permanently in your user environment variables:" -ForegroundColor Yellow
    Write-Host "[System.Environment]::SetEnvironmentVariable('MSSQL_MCP_API_KEY', '$ApiKey', 'User')" -ForegroundColor Cyan
}

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

# Check if an API key is already set
$currentKey = $env:MSSQL_MCP_API_KEY
if ($currentKey) {
    Write-Host "Current API key is set in environment: [HIDDEN]" -ForegroundColor Yellow
    $resetKey = Read-Host "Do you want to generate a new API key? (y/n)"
    if ($resetKey -ne "y") {
        Write-Host "Keeping existing API key." -ForegroundColor Green
        exit
    }
}

# Generate a new API key
try {
    $response = Invoke-McpApi -Method "security.generateSecureKey" -Params @{ length = 48 }
    $newKey = $response.result.key
    
    Write-Host "`nGenerated new API key:" -ForegroundColor Green
    Write-Host $newKey -ForegroundColor Cyan
    
    # Set the key in the current session
    SetAndConfigureKey $newKey
}
catch {
    Write-Host "Failed to generate API key via MCP API. Server may not be running." -ForegroundColor Red
    
    # Generate a key using .NET cryptography directly as fallback
    $byteLength = 32
    $randomBytes = New-Object byte[] $byteLength
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($randomBytes)
    $apiKey = [Convert]::ToBase64String($randomBytes)
    
    Write-Host "`nGenerated new API key using local generation:" -ForegroundColor Green
    Write-Host $apiKey -ForegroundColor Cyan
    
    # Set the key in the current session
    SetAndConfigureKey $apiKey
}

# Print usage examples
Write-Host "`nAPI Request Examples:" -ForegroundColor Cyan
Write-Host "1. PowerShell:" -ForegroundColor White
$displayKey = if ($env:MSSQL_MCP_API_KEY) { $env:MSSQL_MCP_API_KEY } else { "YOUR-API-KEY" }
Write-Host "Invoke-RestMethod -Uri `"http://localhost:3001/`" -Method Post -Headers @{`"X-API-Key`" = `"$displayKey`"; `"Content-Type`" = `"application/json`"} -Body '{`"jsonrpc`": `"2.0`", `"id`": 1, `"method`": `"echo`", `"params`": {`"message`": `"Hello`"}}'" -ForegroundColor Gray

Write-Host "`n2. curl:" -ForegroundColor White
Write-Host "curl -X POST http://localhost:3001/ -H `"X-API-Key: $displayKey`" -H `"Content-Type: application/json`" -d '{`"jsonrpc`": `"2.0`", `"id`": 1, `"method`": `"echo`", `"params`": {`"message`": `"Hello`"}}'" -ForegroundColor Gray

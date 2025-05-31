# Generate-MCP-Key.ps1
# Script to generate and set an API key for SQL Server MCP server

Write-Host "SQL Server MCP - Key Generator" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Get the path to the appsettings.json file
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$appSettingsPath = Join-Path $rootPath "appsettings.json"

# Helper function to set and configure the API key
 

 

# Check if an API key is already set
$currentKey = $env:MSSQL_MCP_KEY
if ($currentKey) {
    Write-Host "Current Encryption key is set in environment: [HIDDEN]" -ForegroundColor Yellow
    $resetKey = Read-Host "Do you want to generate a new Encryption key? (y/n)"
    if ($resetKey -ne "y") {
        Write-Host "Keeping existing Encryption key." -ForegroundColor Green
        exit
    }
}

# Generate a new Encryption key
try {
    # Generate a key using .NET cryptography directly  
    $byteLength = 32
    $randomBytes = New-Object byte[] $byteLength
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($randomBytes)
    $enctryptionKey = [Convert]::ToBase64String($randomBytes)

    Write-Host "`nGenerated new Encryption key using local generation:" -ForegroundColor Green
    Write-Host $enctryptionKey -ForegroundColor Cyan
    

}
catch {
    Write-Host "Failed to generate Encryption key via MCP." -ForegroundColor Red
    exit

}

# Print usage examples
Write-Host "`nSet via PowerShell:" -ForegroundColor Cyan

Write-Host " `$env:MSSQL_MCP_KEY` = `"$enctryptionKey`"; " -ForegroundColor Gray

Write-Host " " -ForegroundColor White
 
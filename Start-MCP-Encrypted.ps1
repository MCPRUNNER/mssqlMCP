# Start-MCP-Encrypted.ps1
# Script to start the SQL Server MCP server with connection string encryption enabled

Write-Host "Starting SQL Server MCP with encrypted connection strings..." -ForegroundColor Green

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

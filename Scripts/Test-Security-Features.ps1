# Test-Security-Features.ps1
# Script to test the security features of SQL Server MCP server

Write-Host "Testing SQL Server MCP Security Features..." -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
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
        
        # Write-Host "Making MCP call: $Method" -ForegroundColor Cyan
        # Write-Host "Parameters: $($Params | ConvertTo-Json)" -ForegroundColor Cyan
        
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

# 1. Testing connection with encryption
Write-Host "1. Adding a test connection with encryption" -ForegroundColor Cyan
$response = Invoke-McpApi -Method "connectionManager.addConnection" -Params @{
    name             = "TestSecureConnection"
    connectionString = "Server=localhost;Database=master;Trusted_Connection=True;Encrypt=True;"
    description      = "Test connection with encryption"
}

Write-Host "Response: $($response.result | ConvertTo-Json)" -ForegroundColor Green
Write-Host ""

# 2. Listing connections to verify
Write-Host "2. Listing connections to verify encryption" -ForegroundColor Cyan
$response = Invoke-McpApi -Method "connectionManager.listConnections"

Write-Host "Connections:" -ForegroundColor Green
$response.result.connections | ForEach-Object {
    Write-Host "- $($_.name): $($_.description)" -ForegroundColor White
}
Write-Host ""

# 3. Test migrating connections to encrypted format
Write-Host "3. Migrating unencrypted connections to encrypted format" -ForegroundColor Cyan
$response = Invoke-McpApi -Method "security.migrateConnectionsToEncrypted"

Write-Host "Response: $($response.result | ConvertTo-Json)" -ForegroundColor Green
Write-Host ""

# 4. Verify connection works after encryption
Write-Host "4. Testing connection after encryption/migration" -ForegroundColor Cyan
$testResult = Invoke-McpApi -Method "connectionManager.testConnection" -Params @{
    name = "TestSecureConnection" 
}

Write-Host "Connection test result: $($testResult.result.Success)" -ForegroundColor $(if ($testResult.result.Success) { "Green" } else { "Red" })
if (-not $testResult.result.Success) {
    Write-Host "Error message: $($testResult.result.Message)" -ForegroundColor Red
}
Write-Host ""

# 5. Test key rotation (if user wants to test it)
$testKeyRotation = Read-Host "Do you want to test key rotation? This will require restarting the MCP server (y/n)"
if ($testKeyRotation -eq "y") {
    Write-Host "5. Testing key rotation" -ForegroundColor Cyan
    
    # Generate a new key
    $keyLength = 32
    $randomBytes = New-Object byte[] $keyLength
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($randomBytes)
    $newKey = [Convert]::ToBase64String($randomBytes)
    
    Write-Host "Generated new key: $newKey" -ForegroundColor Yellow
    
    # Rotate the key
    $response = Invoke-McpApi -Method "security.rotateKey" -Params @{
        newKey = $newKey
    }

    Write-Host "Response: $($response.result | ConvertTo-Json)" -ForegroundColor Green
    Write-Host ""
    
    # Save the current key as a backup
    $originalKey = $env:MSSQL_MCP_KEY
    
    # Ask user if they want to restart the server with the new key
    $restart = Read-Host "Do you want to restart the server with the new key to verify rotation? (y/n)"
    if ($restart -eq "y") {
        Write-Host "Stopping current MCP server..." -ForegroundColor Yellow
        # Close the current server - this assumes it's running in the current console
        # In a real scenario, you'd need to stop the actual process
        Write-Host "Please manually stop the current MCP server and press Enter when done"
        Read-Host | Out-Null
        
        # Start with new key
        Write-Host "Starting server with new key..." -ForegroundColor Yellow
        $env:MSSQL_MCP_KEY = $newKey
        
        # Start in a new window
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$pwd'; $env:MSSQL_MCP_KEY = '$newKey'; Write-Host 'Using new encryption key: [PROTECTED]' -ForegroundColor Green; ./Start-MCP-Encrypted.ps1"
        
        # Wait for server to start
        Write-Host "Waiting for server to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        
        # Test connection again
        Write-Host "Testing connection with new key..." -ForegroundColor Cyan
        try {
            $testResult = Invoke-McpApi -Method "connectionManager.testConnection" -Params @{
                name = "TestSecureConnection" 
            }

            Write-Host "Connection test result: $($testResult.result.Success)" -ForegroundColor $(if ($testResult.result.Success) { "Green" } else { "Red" })
            if (-not $testResult.result.Success) {
                Write-Host "Error message: $($testResult.result.Message)" -ForegroundColor Red
            }
        }
        catch {
            Write-Host "Error testing connection with new key. The server may not be ready yet." -ForegroundColor Red
        }
        
        # Restore original key for this script session
        $env:MSSQL_MCP_KEY = $originalKey
    }
    else {
        Write-Host "You can start the server manually with the new key using:" -ForegroundColor Yellow
        Write-Host '$env:MSSQL_MCP_KEY = "' + $newKey + '"' -ForegroundColor Cyan
        Write-Host "./Start-MCP-Encrypted.ps1" -ForegroundColor Cyan
    }
}
else {
    Write-Host "Skipping key rotation test." -ForegroundColor Yellow
}

# 6. Cleanup
Write-Host "6. Cleaning up test connection" -ForegroundColor Cyan
$response = Invoke-McpApi -Method "connectionManager.removeConnection" -Params @{
    name = "TestSecureConnection"
}

Write-Host "Response: $($response.result | ConvertTo-Json)" -ForegroundColor Green
Write-Host ""

Write-Host "Security features testing completed!" -ForegroundColor Green
Write-Host "For more information on security features, see Documentation/Security.md" -ForegroundColor Cyan

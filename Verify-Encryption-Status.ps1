# Verify-Encryption-Status.ps1
# Script to verify encryption status of connection strings and fix any issues

Write-Host "SQL Server MCP - Encryption Status Verification" -ForegroundColor Green
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

function Test-ServerConnection {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:3001/api/test" -Method GET
        return $true
    }
    catch {
        return $false
    }
}

# Check if the server is running
if (-not (Test-ServerConnection)) {
    Write-Host "The SQL Server MCP server does not appear to be running on http://localhost:3001." -ForegroundColor Red
    Write-Host "Please start the server before running this script." -ForegroundColor Yellow
    exit 1
}

# 1. Encryption Key Status
Write-Host "1. Checking Encryption Key Status" -ForegroundColor Cyan
if ($env:MSSQL_MCP_KEY) {
    Write-Host "✅ MSSQL_MCP_KEY is set in the current environment" -ForegroundColor Green
    # Check if it's the default key by using a safe hash comparison
    $keyBytes = [System.Text.Encoding]::UTF8.GetBytes($env:MSSQL_MCP_KEY)
    $hasher = [System.Security.Cryptography.SHA256]::Create()
    $keyHash = [Convert]::ToBase64String($hasher.ComputeHash($keyBytes))
    
    $defaultKeyBytes = [System.Text.Encoding]::UTF8.GetBytes("DefaultInsecureKey_DoNotUseInProduction!")
    $defaultKeyHash = [Convert]::ToBase64String($hasher.ComputeHash($defaultKeyBytes))
    
    if ($keyHash -eq $defaultKeyHash) {
        Write-Host "⚠️ WARNING: You are using the default insecure key!" -ForegroundColor Red
        Write-Host "   This is not recommended for production use." -ForegroundColor Red
        Write-Host "   Generate a secure key with: ./Assess-Connection-Security.ps1" -ForegroundColor Yellow
    }
}
else {
    Write-Host "❌ MSSQL_MCP_KEY is NOT set in the current environment" -ForegroundColor Red
    Write-Host "   Using default insecure key! Set a strong encryption key with:" -ForegroundColor Red
    Write-Host '   $env:MSSQL_MCP_KEY = "your-strong-random-key"' -ForegroundColor Cyan
    
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
                
                $restart = Read-Host "Would you like to restart the server now with the new key? (y/n)"
                if ($restart -eq "y") {
                    Write-Host "Please manually stop the current MCP server and press Enter when done"
                    Read-Host | Out-Null
                    
                    # Start in a new window
                    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$pwd'; $env:MSSQL_MCP_KEY = '$newKey'; Write-Host 'Using new encryption key: [PROTECTED]' -ForegroundColor Green; ./Start-MCP-Encrypted.ps1"
                    
                    Write-Host "Server restarting with new key... waiting for it to start up" -ForegroundColor Yellow
                    Start-Sleep -Seconds 5
                    
                    # Wait for the server to be available
                    $maxRetries = 10
                    $retryCount = 0
                    $serverRunning = $false
                    
                    while (-not $serverRunning -and $retryCount -lt $maxRetries) {
                        $serverRunning = Test-ServerConnection
                        if (-not $serverRunning) {
                            Write-Host "Waiting for server to start... ($retryCount/$maxRetries)" -ForegroundColor Yellow
                            Start-Sleep -Seconds 2
                            $retryCount++
                        }
                    }
                    
                    if ($serverRunning) {
                        Write-Host "Server successfully restarted with the new key." -ForegroundColor Green
                    }
                    else {
                        Write-Host "Server did not start in the expected time. Please check for errors." -ForegroundColor Red
                        Write-Host "You can continue, but some functions may not work properly." -ForegroundColor Yellow
                    }
                }
            }
        }
        catch {
            Write-Host "Failed to generate key via MCP API. Server may not be running." -ForegroundColor Red
            Write-Host "You can still generate a key using the SecurityTool directly or use Start-MCP-Encrypted.ps1" -ForegroundColor Yellow
        }
    }
}
Write-Host ""

# 2. List and Check Connections
Write-Host "2. Checking connection strings encryption status" -ForegroundColor Cyan
try {
    $response = Invoke-McpApi -Method "connectionManager.listConnections"
    
    $totalConnections = $response.result.connections.Count
    
    if ($totalConnections -eq 0) {
        Write-Host "No connections found in the database." -ForegroundColor Yellow
    }
    else {
        Write-Host "Found $totalConnections connections in the database." -ForegroundColor Green
        
        # Run migration to encrypt any unencrypted connections
        Write-Host "`nWould you like to ensure all connections are encrypted? (y/n)" -ForegroundColor Cyan
        $migrate = Read-Host
        if ($migrate -eq "y") {
            try {
                $migrateResponse = Invoke-McpApi -Method "security.migrateConnectionsToEncrypted"
                $migratedCount = $migrateResponse.result.count
                
                if ($migratedCount -gt 0) {
                    Write-Host "✅ Successfully encrypted $migratedCount previously unencrypted connection(s)" -ForegroundColor Green
                }
                else {
                    Write-Host "✅ All connections were already encrypted" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "❌ Failed to migrate unencrypted connections: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}
catch {
    Write-Host "❌ Failed to list connections: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 3. Key Rotation Check
Write-Host "3. Key Rotation Status Check" -ForegroundColor Cyan

# Check if there's a log entry indicating when the key was last rotated
$logDir = Join-Path (Get-Location) "Logs"
$keyRotationFound = $false

if (Test-Path $logDir) {
    $logFiles = Get-ChildItem -Path $logDir -Filter "mssqlMCP-*.log" | Sort-Object LastWriteTime -Descending
    
    foreach ($logFile in $logFiles) {
        $keyRotationEntries = Select-String -Path $logFile.FullName -Pattern "Key rotation completed" -ErrorAction SilentlyContinue
        
        if ($keyRotationEntries -and $keyRotationEntries.Count -gt 0) {
            $lastRotation = $keyRotationEntries[0]
            $lastRotationDate = [datetime]::ParseExact($lastRotation.Line.Substring(0, 19), "yyyy-MM-dd HH:mm:ss", $null)
            $daysSinceRotation = (Get-Date) - $lastRotationDate
            
            Write-Host "Last key rotation was performed on $($lastRotationDate.ToString('yyyy-MM-dd'))" -ForegroundColor Green
            Write-Host "That was $([math]::Floor($daysSinceRotation.TotalDays)) days ago" -ForegroundColor Cyan
            
            if ($daysSinceRotation.TotalDays -gt 90) {
                Write-Host "⚠️ WARNING: It's recommended to rotate your encryption key every 90 days" -ForegroundColor Yellow
                Write-Host "   Consider rotating your key with: ./Rotate-Encryption-Key.ps1" -ForegroundColor Yellow
            }
            else {
                Write-Host "✅ Your encryption key is within the recommended 90-day rotation period" -ForegroundColor Green
            }
            
            $keyRotationFound = $true
            break
        }
    }
}

if (-not $keyRotationFound) {
    Write-Host "No record of key rotation found in logs" -ForegroundColor Yellow
    Write-Host "Consider establishing a key rotation schedule (recommended: every 90 days)" -ForegroundColor Yellow
    Write-Host "You can rotate your key with: ./Rotate-Encryption-Key.ps1" -ForegroundColor Cyan
}

# 4. Recommendations
Write-Host "`n4. Security Recommendations" -ForegroundColor Cyan

Write-Host "Based on the verification results, here are the recommended actions:" -ForegroundColor White

if (-not $env:MSSQL_MCP_KEY -or ($keyHash -eq $defaultKeyHash)) {
    Write-Host "✅ ESTABLISH A SECURE KEY: Generate and set a strong encryption key" -ForegroundColor Yellow
    Write-Host "   Run ./Assess-Connection-Security.ps1 and follow the prompts" -ForegroundColor Cyan
}

if (-not $keyRotationFound -or ($keyRotationFound -and $daysSinceRotation.TotalDays -gt 90)) {
    Write-Host "✅ SET UP KEY ROTATION: Implement regular key rotation" -ForegroundColor Yellow
    Write-Host "   Run ./Rotate-Encryption-Key.ps1 every 90 days" -ForegroundColor Cyan
}

Write-Host "✅ VERIFY ENCRYPTION: Regularly check that all connections are encrypted" -ForegroundColor Yellow
Write-Host "   Run ./Migrate-To-Encrypted.ps1 periodically" -ForegroundColor Cyan

Write-Host "✅ TEST SECURITY FEATURES: Validate security implementation" -ForegroundColor Yellow
Write-Host "   Run ./Test-Security-Features.ps1 after major changes" -ForegroundColor Cyan

Write-Host "`nVerification completed!" -ForegroundColor Green

#!/usr/bin/env pwsh
# PowerShell script to test the MCP server

$uri = "http://localhost:3001"
$headers = @{
    'Content-Type' = 'application/json'
    'Accept'       = 'application/json'
}

Write-Host "Testing Echo functionality..." -ForegroundColor Green
$echoBody = @{
    jsonrpc = "2.0"
    method  = "echo"
    params  = @{
        message = "Hello from PowerShell test!"
    }
    id      = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $echoBody
    Write-Host "Echo response: $($response | ConvertTo-Json -Depth 5)" -ForegroundColor Cyan
}
catch {
    Write-Host "Error calling Echo: $_" -ForegroundColor Red
}

Write-Host "`nTesting Initialize functionality..." -ForegroundColor Green
$initBody = @{
    jsonrpc = "2.0"
    method  = "initialize"
    params  = @{
        connectionName = "DefaultConnection"
    }
    id      = 2
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $initBody
    Write-Host "Initialize response: $($response | ConvertTo-Json -Depth 5)" -ForegroundColor Cyan
}
catch {
    Write-Host "Error calling Initialize: $_" -ForegroundColor Red
}

Write-Host "`nTest completed." -ForegroundColor Green

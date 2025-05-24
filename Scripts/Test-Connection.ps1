# Test-Connection.ps1
# PowerShell script to test a SQL Server connection string
param(
    [Parameter(Mandatory = $true)]
    [string]$ConnectionString
)

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $connection.Open()
    Write-Host "Connection successful!"
    $connection.Close()
    exit 0
}
catch {
    Write-Host "Connection failed: $($_.Exception.Message)"
    exit 1
}

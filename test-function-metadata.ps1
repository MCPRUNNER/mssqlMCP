# Test Functions Metadata Script
# This script demonstrates fetching SQL function metadata using the MCP server

Write-Host "Testing SQL Function Metadata Retrieval..." -ForegroundColor Green

# Define connection variables
$connectionName = "DefaultConnection"
$schema = $null  # Set to a specific schema or leave null for all schemas

# Initialize the connection
$result = Invoke-MCP SqlServerTools.Initialize -Args @($connectionName)
Write-Host "Connection result: $result" -ForegroundColor Cyan

# Get metadata for all database objects
Write-Host "`nRetrieving metadata for ALL objects..." -ForegroundColor Yellow
$allObjects = Invoke-MCP SqlServerTools.GetDatabaseObjectsMetadata -Args @($connectionName, $schema, "ALL")

# Get metadata specifically for functions
Write-Host "`nRetrieving metadata for FUNCTIONS only..." -ForegroundColor Yellow
$functionObjects = Invoke-MCP SqlServerTools.GetDatabaseObjectsMetadata -Args @($connectionName, $schema, "FUNCTION")

# Show number of functions found
$functionData = $functionObjects | ConvertFrom-Json
Write-Host "Found $($functionData.Count) functions" -ForegroundColor Green

# Display function details
if ($functionData.Count -gt 0) {
    foreach ($function in $functionData) {
        Write-Host "`nFunction: $($function.Schema).$($function.Name)" -ForegroundColor Magenta
        
        # Display function type and return type if available
        if ($function.Properties) {
            Write-Host "Function Type: $($function.Properties.FunctionType)" -ForegroundColor Cyan
            Write-Host "Return Type: $($function.Properties.ReturnType)" -ForegroundColor Cyan
        }
        
        # Display parameters
        if ($function.Columns.Count -gt 0) {
            Write-Host "`nParameters:" -ForegroundColor Yellow
            foreach ($param in $function.Columns) {
                Write-Host "  $($param.Name): $($param.DataType) $($param.Description)" -ForegroundColor White
            }
        }
        else {
            Write-Host "`nNo parameters" -ForegroundColor Yellow
        }
        
        # Display definition (first 100 characters only for brevity)
        if ($function.Definition) {
            $previewLength = [Math]::Min(100, $function.Definition.Length)
            $preview = $function.Definition.Substring(0, $previewLength)
            Write-Host "`nDefinition (preview): $preview..." -ForegroundColor Gray
        }
        
        Write-Host "------------------------------------------------"
    }
}
else {
    Write-Host "No functions found in the database." -ForegroundColor Yellow
}

Write-Host "`nFunction metadata test completed!" -ForegroundColor Green

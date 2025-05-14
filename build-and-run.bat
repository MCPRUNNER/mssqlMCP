@echo off
echo Building and running SQL Server MCP Server...

REM Clean previous builds
dotnet clean

REM Restore packages
echo.
echo Restoring packages...
dotnet restore

REM Build the project
echo.
echo Building project...
dotnet build

REM Run the server
echo.
echo Starting MCP Server...
echo Press Ctrl+C to stop the server
dotnet run

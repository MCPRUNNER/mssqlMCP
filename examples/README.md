# SQL Server MCP Examples

This folder contains examples of how to use the SQL Server MCP server from different programming environments.

## Available Examples

- **Node.js Example**: Shows how to connect to the MCP server from a Node.js application
- **Python Example**: Demonstrates using the MCP server from Python
- **VS Code Extension Example**: Guide on creating a VS Code extension that uses the SQL Server MCP

## Running the Examples

### Prerequisites

- The SQL Server MCP server is running on http://localhost:3001
- A SQL Server instance is accessible with the connection strings configured in `appsettings.json`

### Node.js Example

1. Install dependencies:

   ```
   npm install
   ```

2. Run the example:
   ```
   node node-example.js
   ```

### Python Example

1. Install dependencies:

   ```
   pip install -r requirements.txt
   ```

2. Run the example:
   ```
   python python-example.py
   ```

### VS Code Extension Example

See the instructions in `vscode-extension-example.md`.

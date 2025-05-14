# SQL Server Explorer VS Code Extension

This folder demonstrates how to create a VS Code extension that uses the SQL Server MCP for database connectivity.

## Project Setup

1. Create a new extension project:

   ```
   yo code
   ```

2. Install dependencies:

   ```
   npm install @vscode/mcp-node-client
   ```

3. Configure MCP client in your extension:

```typescript
// src/extension.ts
import * as vscode from "vscode";
import { MCPClient } from "@vscode/mcp-node-client";

export async function activate(context: vscode.ExtensionContext) {
  // Initialize the MCP client
  const mcpClient = new MCPClient({
    endpoint: "http://localhost:3001",
  });

  // Register commands
  let disposable = vscode.commands.registerCommand(
    "sqlexplorer.queryDatabase",
    async () => {
      // Initialize the connection first
      try {
        await mcpClient.invoke("initialize");
        vscode.window.showInformationMessage("Connected to SQL Server!");

        // Get input from the user
        const query = await vscode.window.showInputBox({
          prompt: "Enter your SQL query",
          placeHolder: "SELECT * FROM YourTable",
        });

        if (query) {
          // Execute the query
          const result = await mcpClient.invoke("executeQuery", { query });

          // Show the results in a new document
          const doc = await vscode.workspace.openTextDocument({
            content: JSON.stringify(JSON.parse(result), null, 2),
            language: "json",
          });

          await vscode.window.showTextDocument(doc);
        }
      } catch (error) {
        vscode.window.showErrorMessage(`Error: ${error.message}`);
      }
    }
  );

  context.subscriptions.push(disposable);
}

export function deactivate() {}
```

## Building and Using the Extension

1. Build the extension:

   ```
   npm run compile
   ```

2. Press F5 to launch a new VS Code instance with the extension

3. Open the Command Palette (Ctrl+Shift+P) and run "SQL Explorer: Query Database"

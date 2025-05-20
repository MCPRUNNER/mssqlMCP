# Docker Hub

This guide provides detailed instructions for deploying the SQL Server MCP (Model Context Protocol) Server using Docker.

## Overview

This image is pushed to Docker Hub as [7045khz/mssqlmcp](https://hub.docker.com/repository/docker/7045khz/mssqlmcp)

## Running Docker Image

Recommend using persistent storage for Data directory and secure method of replacing Variable values.

```bash

# MINGW64 Bash on Windows

MSYS_NO_PATHCONV=1  docker run -d \
  --name mssqlmcp \
  -p 3001:3001 \
  -e MSSQL_MCP_KEY="StrongEncryptionKeyForEncryptionOfYourConnectionStrings" \
  -e MSSQL_MCP_API_KEY="StrongApiKeyYourConnections" \
  -v "$(pwd)/data:/app/Data" \
  -v "$(pwd)/logs:/app/Logs" \
  7045khz/mssqlmcp;

```

Starting with standard bash is almost identical

```bash

# Standard Bash

docker run -d \
  --name mssqlmcp \
  -p 3001:3001 \
  -e MSSQL_MCP_KEY="StrongEncryptionKeyForEncryptionOfYourConnectionStrings" \
  -e MSSQL_MCP_API_KEY="StrongApiKeyYourConnections" \
  -v "$(pwd)/data:/app/Data" \
  -v "$(pwd)/logs:/app/Logs" \
  7045khz/mssqlmcp;

```

## Visual Studio Code Setup

Update or create your .vscode/mcp.json file.

```json
{
  "inputs": [
    {
      "id": "mssql-server-mcp-api-key",
      "type": "promptString",
      "description": "Enter your SQL Server MCP API Key",
      "password": true
    }
  ],
  "servers": {
    "sql-server-mcp": {
      "url": "http://localhost:3001",
      "headers": {
        "X-API-Key": "${input:mssql-server-mcp-api-key}",
        "Content-Type": "application/json"
      }
    }
  }
}
```

## Starting in VSCode

Once added you should be able to hover over the "sql-server-mcp" and see "Start". Select Start and enter in the same API Key you started the docker image with.

## Quick Examples of Usage and Architecture Overview

- [What can I do with this?](https://github.com/7045kHz/mssqlMCP/blob/master/Documentation/EXAMPLE_USAGE.md)
- [Architecture Documentation](https://github.com/7045kHz/mssqlMCP/blob/master/Documentation/Architecture.md)
- [Project README.md](https://github.com/7045kHz/mssqlMCP/blob/master/README.md)

## Source

[https://github.com/7045kHz/mssqlMCP](https://github.com/7045kHz/mssqlMCP)

## Troubleshooting

If you encounter issues:

1. Check the container logs:

   ```bash
   docker logs mssqlmcp;
   ```

2. Verify environment variables are set correctly:

   ```bash
   docker inspect mssqlmcp | grep -A 10 "Env";
   ```

3. Ensure volume mounts have appropriate permissions:
   ```bash
   ls -la ./data ./logs;
   ```

## Further Information

For more details on using the SQL Server MCP Server, refer to the main README.md and QUICK_INSTALL.md files in [https://github.com/7045kHz/mssqlMCP](https://github.com/7045kHz/mssqlMCP).

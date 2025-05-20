# Docker Setup for SQL Server MCP Server

This guide provides detailed instructions for deploying the SQL Server MCP (Model Context Protocol) Server using Docker.

## Overview

The SQL Server MCP Server enables AI assistants like GitHub Copilot to interact with SQL Server databases through the Model Context Protocol. This containerized approach simplifies deployment and allows for better environment isolation.

## Prerequisites

- Docker installed on your system
- Docker Compose (optional, but recommended)
- Basic understanding of Docker and container technology

## Quick Start

### Using Docker Compose (Recommended)

1. Clone this repository
2. Configure environment variables in the docker-compose.yml file
3. Run the container:

```bash
docker-compose up -d;
```

### Using Docker CLI

```bash
docker build -t mssqlmcp:latest .;

docker run -d \
  --name mssqlmcp \
  -p 3001:3001 \
  -e MSSQL_MCP_KEY="your-secure-encryption-key" \
  -e MSSQL_MCP_API_KEY="your-secure-api-key" \
  -v "$(pwd)/data:/app/Data" \
  -v "$(pwd)/logs:/app/Logs" \
  mssqlmcp:latest;
```

## Configuration Options

### Required Environment Variables

- `MSSQL_MCP_KEY`: Encryption key for securing connection strings
- `MSSQL_MCP_API_KEY`: API key for authenticating client requests

### Optional Environment Variables

- `MSSQL_MCP_DATA`: Override the default data directory location
- `ASPNETCORE_URLS`: Configure the listening URL (default: "http://+:3001")

## Data Directory Configuration

The SQL Server MCP Server stores connection information in an SQLite database. By default, this database is located at `/app/Data` in the container. You can override this location using the `MSSQL_MCP_DATA` environment variable.

### Example with Custom Data Directory

```bash
docker run -d \
  --name mssqlmcp \
  -p 3001:3001 \
  -e MSSQL_MCP_KEY="your-secure-encryption-key" \
  -e MSSQL_MCP_API_KEY="your-secure-api-key" \
  -e MSSQL_MCP_DATA="/custom/data/path" \
  -v "/host/path/to/data:/custom/data/path" \
  -v "$(pwd)/logs:/app/Logs" \
  mssqlmcp:latest;
```

### Docker Compose Example with Custom Data Directory

```yaml
version: "3.8"

services:
  mssqlmcp:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "3001:3001"
    environment:
      - MSSQL_MCP_KEY=your-secure-encryption-key
      - MSSQL_MCP_API_KEY=your-secure-api-key
      - MSSQL_MCP_DATA=/custom/data/path
    volumes:
      - ./custom-data:/custom/data/path
      - ./logs:/app/Logs
    restart: unless-stopped
```

## Volume Mounts

- **Data Volume**: Mount a volume to persist database connections between container restarts
- **Logs Volume**: Mount a volume to access server logs from the host

## Security Considerations

- Use strong, randomly generated values for both `MSSQL_MCP_KEY` and `MSSQL_MCP_API_KEY`
- In production environments, consider using Docker secrets or a secure vault solution
- Always use HTTPS in production by configuring a reverse proxy
- Ensure your SQL Server instances are properly secured and only accessible to authorized clients

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

For more details on using the SQL Server MCP Server, refer to the main README.md and QUICK_INSTALL.md files.

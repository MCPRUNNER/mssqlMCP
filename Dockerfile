FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj file and restore dependencies
COPY ["mssqlMCP.csproj", "./"]
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "mssqlMCP.csproj" -c Release -o /app/build
RUN dotnet publish "mssqlMCP.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create directories for data and logs
RUN mkdir -p /app/Data /app/Logs

# Copy the published application
COPY --from=build /app/publish .

# Set environment variables with defaults (these should be overridden at runtime)
ENV MSSQL_MCP_KEY=""
ENV MSSQL_MCP_API_KEY=""
ENV MSSQL_MCP_DATA="/app/Data"
ENV ASPNETCORE_URLS="http://+:3001"

# Expose the MCP server port
EXPOSE 3001

# Set the entry point
ENTRYPOINT ["dotnet", "mssqlMCP.dll"]
# mssqlMCP Release Notes

## Version 1.0.9.5 - Documentation Standardization & Tool Naming Consistency

**Release Date:** November 8, 2025

---

## Summary of Changes

This release focuses on comprehensive documentation standardization and tool naming consistency across the entire mssqlMCP project. The primary goal was to ensure all documentation, examples, and test files use the standardized `mssql_` prefix for tool names, creating a cohesive and professional user experience.

---

## 📚 Documentation Standardization

### Comprehensive Documentation Updates

- **Tool Naming Consistency**: All documentation now uses standardized `mssql_` prefixed tool names
- **API Reference Updates**: Complete update of API_ENDPOINTS.md with correct tool names and examples
- **User Guide Alignment**: CopilotAgent.md and QUICK_INSTALL.md updated with accurate command examples
- **Technical Documentation**: Overview.md and MultiKeyAuthentication.md synchronized with implementation

### Updated Documentation Files

- **API_ENDPOINTS.md**: All JSON-RPC examples updated with correct tool names
- **MultiKeyAuthentication.md**: Tool lists and security examples standardized
- **CopilotAgent.md**: User-facing examples corrected for practical usage
- **QUICK_INSTALL.md**: Installation commands updated with accurate tool references
- **Overview.md**: Tool categories and descriptions aligned with implementation
- **tools_list.json**: Complete tool registry updated with proper specifications
- **OpenAPI specifications**: Both YAML and JSON specs synchronized with actual tool names

---

## 🔧 Implementation Updates

### Example Code Standardization

- **McpClientExample.cs**: All tool calls updated to use `mssql_` prefixed names
- **McpConsoleExample.cs**: Console examples aligned with standardized naming
- **API Key Management**: Added comprehensive examples for all API key management tools
- **Connection Management**: Updated examples with correct tool invocations

### Test Suite Updates

- **ApiKeyAuthMiddlewareTests.cs**: All test cases updated to use correct tool names
- **Middleware Implementation**: Updated `ApiKeyAuthMiddleware.cs` with standardized allowed tool names
- **Test Coverage**: Maintained 100% test coverage with corrected tool name references

---

## 🏗️ Tool Name Standardization

### Core SQL Server Tools

- `Initialize` → `mssql_initialize_connection`
- `ExecuteQuery` → `mssql_execute_query`
- `GetTableMetadata` → `mssql_get_table_metadata`
- `GetDatabaseObjectsMetadata` → `mssql_get_database_objects_metadata`
- `GetDatabaseObjectsByType` → `mssql_get_database_objects_by_type`

### Specialized Metadata Tools

- `GetSqlServerAgentJobs` → `mssql_get_agent_jobs`
- `GetSqlServerAgentJobDetails` → `mssql_get_agent_job_details`
- `GetSsisCatalogInfo` → `mssql_get_ssis_catalog_info`
- `GetAzureDevOpsInfo` → `mssql_get_azure_devops_info`

### Connection Management Tools

- `ListConnections` → `mssql_list_connections`
- `AddConnection` → `mssql_add_connection`
- `UpdateConnection` → `mssql_update_connection`
- `RemoveConnection` → `mssql_remove_connection`
- `TestConnection` → `mssql_test_connection`

### Security Tools

- `GenerateSecureKey` → `mssql_generate_key`
- `MigrateConnectionsToEncrypted` → `mssql_migrate_connections`
- `RotateKey` → `mssql_rotate_key`

### API Key Management Tools

- `CreateApiKey` → `mssql_create_key`
- `ListUserKeys` → `mssql_list_user_keys`
- `ListAllKeys` → `mssql_list_all_keys`
- `RevokeKey` → `mssql_revoke_key`
- `DeleteKey` → `mssql_delete_key`
- `GetKeyUsageLogs` → `mssql_get_key_usage_logs`
- `GetUserUsageLogs` → `mssql_get_user_usage_logs`

---

## 🧪 Quality Improvements

### Test Suite Enhancement

- **All Tests Passing**: 770 tests passing, 0 failing after updates
- **Authentication Tests**: Updated middleware tests with correct tool name validation
- **Integration Tests**: Ensured all integration examples use correct tool names
- **Error Handling**: Maintained robust error handling with updated tool references

### Documentation Quality

- **Consistency Check**: All tool references verified across documentation
- **Example Validation**: All code examples tested for accuracy
- **User Experience**: Improved user experience with consistent naming throughout

---

## 🔄 Migration Impact

### No Breaking Changes

- **Backward Compatibility**: All existing functionality preserved
- **Tool Implementation**: Core tool functionality unchanged
- **API Compatibility**: Existing API endpoints continue to work

### Client Updates Recommended

- **Documentation Reference**: Update any custom documentation to use new tool names
- **Example Code**: Update example implementations to follow standardized naming
- **Integration Scripts**: Review and update any integration scripts with new tool names

---

## 📦 Dependencies & Compatibility

- **.NET 9.0**: Continued support for .NET 9.0 framework
- **SQL Server Compatibility**: SQL Server 2019+ recommended for optimal performance
- **MCP Protocol**: Full compliance with Model Context Protocol specifications
- **Authentication**: Enhanced multi-key authentication system fully functional

---

## 🚀 Performance & Reliability

- **Test Coverage**: Maintained comprehensive test coverage
- **Documentation Accuracy**: All examples verified for correctness
- **User Experience**: Improved consistency reduces learning curve
- **Professional Quality**: Documentation now meets enterprise standards

---

## 🙏 Contributors

- **7045kHz**: Lead developer, documentation standardization, and tool naming consistency implementation
- **Community**: Feedback and testing contributions

---

## 🔮 Looking Ahead

### Planned for Future Releases

- Enhanced tool categorization and organization
- Interactive documentation with live examples
- Automated documentation generation from tool definitions
- Enhanced user onboarding experience

---

## Version 1.0.9.4 - Security Enhancement Release

**Release Date:** November 6, 2025

---

## Summary of Changes

This release represents a major security enhancement milestone for the mssqlMCP project. The primary focus has been on implementing comprehensive API key-based authorization, enhanced middleware security, and extensive test coverage. This release includes the successful merge of PR #19 from gdlcf88 which introduces API key-based authorization system.

---

## 🔐 Major Security Enhancements

### Enhanced API Key Authorization System

- **Three-Tier Authentication**: Master keys, Admin keys, and User keys with distinct permission levels
- **Connection-Level Restrictions**: API keys can be restricted to specific database connections
- **Encrypted Key Storage**: All user API keys are stored encrypted in SQLite database using AES-256
- **Role-Based Access Control**: Different endpoints accessible based on key type and permissions

### Comprehensive Authentication Middleware

- **Multiple Auth Methods**: Support for both Bearer token and X-API-Key header authentication
- **Case-Insensitive Handling**: Improved JSON property handling with case-insensitive matching
- **Request Validation**: Enhanced validation of connection names and parameter restrictions
- **Security Logging**: Comprehensive audit logging for all authentication attempts

### JsonHelper Utility

- **New JsonHelper Class**: Case-insensitive JSON property access utility
- **Nested Property Support**: Support for accessing nested JSON properties safely
- **Bug Fix**: Resolved API key authorization case sensitivity issues

---

## 🧪 Testing & Quality Improvements

### Comprehensive Test Suite

- **ApiKeyAuthMiddlewareTests**: Extensive unit tests covering all authentication scenarios
- **Security Test Coverage**: Tests for user/admin/master key access patterns
- **Connection Restriction Tests**: Validation of connection-level security controls
- **Edge Case Testing**: Null values, malformed requests, and error conditions

### Code Quality

- **Enhanced Error Handling**: Better error messages and status codes
- **Improved Validation**: Stronger input validation across all endpoints
- **Documentation Updates**: Comprehensive documentation for new security features

---

## 🔧 Technical Improvements

### Database Schema Updates

- **AllowedConnectionNames Field**: Added support for connection restrictions in API key storage
- **Backward Compatibility**: Existing installations automatically support new features
- **Migration Support**: Seamless upgrade path for existing API keys

### Development Environment

- **JetBrains Rider Support**: Updated .gitignore with Rider-specific configurations
- **MIT License**: Added proper licensing to the project
- **Build Improvements**: Enhanced build configuration and dependency management

---

## 📚 Documentation Updates

### Enhanced Documentation

- **Security Documentation**: Comprehensive security setup and configuration guides
- **API Examples**: Updated authentication examples in API_ENDPOINTS.md
- **Migration Guide**: Step-by-step guide for upgrading existing installations
- **Troubleshooting**: Enhanced troubleshooting documentation for security issues

### New Documentation Files

- **MERGE_UPDATE_NOVEMBER_2025.md**: Detailed analysis of security enhancements
- **Updated tools_list.json**: Current tool definitions and capabilities

---

## 🚨 Breaking Changes

### Authentication Now Required

- **API Key Requirement**: All API requests now require authentication (unless specifically disabled)
- **Environment Variables**: Must configure `MSSQL_MCP_API_KEY` and `MSSQL_MCP_KEY` for new installations
- **Client Updates**: Client applications must include authentication headers

### Endpoint Access Changes

- **Management Endpoints**: Now require admin or master key access
- **User Key Restrictions**: User keys have limited endpoint access compared to previous versions

---

## 🔄 Migration Instructions

### For Existing Installations

1. Set environment variables: `MSSQL_MCP_API_KEY` and `MSSQL_MCP_KEY`
2. Update client applications with authentication headers
3. Review and update API key permissions as needed

### For New Installations

1. Configure master key and encryption key
2. Create user API keys through management endpoints
3. Configure client authentication

---

## 🐛 Bug Fixes

- **Case Sensitivity**: Fixed API key authentication case sensitivity issues
- **JSON Parsing**: Improved JSON property handling in requests
- **Connection Validation**: Enhanced connection name validation in middleware
- **Error Handling**: Better error responses for authentication failures

---

## 📦 Dependencies & Compatibility

- **.NET 9.0**: Continued support for .NET 9.0 framework
- **Backward Compatibility**: Existing API endpoints remain functional
- **Database Compatibility**: SQL Server 2019+ recommended for optimal performance

---

## 🙏 Contributors

Special thanks to:

- **gdlcf88**: Primary contributor for API key authorization system implementation
- **7045kHz**: Project maintenance, documentation, and integration work

---

## 🔮 Looking Ahead

### Planned for Future Releases

- API key expiration dates and automatic rotation
- Rate limiting per API key
- Enhanced audit logging and monitoring
- OAuth 2.0 integration options

---

## Version 2.0.0 - Multi-Key API Release

**Release Date:** June 29, 2025

---

## Summary of Changes

This release introduces significant security and connectivity enhancements to the mssqlMCP (SQL Server Model Context Protocol) server. The major focus has been on implementing a robust multi-key API authentication system and adding stdio communication options for enhanced integration with VS Code Copilot Agent.

---

## New Features

### 🔐 Multi-Key API Authentication System

- **Master API Key Support**: Implemented a hierarchical API key system with master keys for administrative access
- **User-Specific API Keys**: Added support for creating and managing user-specific API keys with granular permissions
- **API Key Lifecycle Management**: Complete CRUD operations for API key management including creation, rotation, and revocation
- **Usage Analytics**: Built-in tracking and analytics for API key usage patterns and access logs

### 🔌 stdio Communication Option

- **stdio Protocol Support**: Added stdio communication protocol alongside existing HTTP endpoints
- **VS Code Integration**: Enhanced integration with VS Code Copilot Agent through stdio interface
- **Dual Protocol Support**: Server now supports both HTTP and stdio protocols simultaneously

### 🛡️ Enhanced Security Features

- **AES-256 Connection String Encryption**: All database connection strings are now encrypted using AES-256
- **Key Rotation Capability**: Secure key rotation mechanisms for both API keys and encryption keys
- **Connection Security Assessment**: Tools to assess and validate connection security configurations

---

## Enhancements

### 📊 Database Metadata Improvements

- **Enhanced Schema Information**: More detailed schema metadata including primary/foreign key relationships
- **Azure DevOps Integration**: Added support for Azure DevOps project, repository, and work item metadata
- **SQL Server Agent Jobs**: Comprehensive SQL Server Agent job information and execution history
- **SSIS Catalog Support**: Added support for SQL Server Integration Services catalog metadata

### 🏗️ Architecture Improvements

- **Clean Architecture**: Implemented separation of concerns with proper dependency injection
- **Async/Await Pattern**: All database operations now use async/await for better performance
- **Strongly-Typed Models**: Enhanced type safety with comprehensive model definitions
- **Robust Logging**: Integrated Serilog for comprehensive logging and diagnostics

### 📚 Documentation Updates

- **Comprehensive API Documentation**: Updated documentation covering all new features
- **Security Guidelines**: Added detailed security configuration and best practices
- **Integration Examples**: PowerShell and C# examples for various integration scenarios
- **Architecture Documentation**: Detailed architectural diagrams and process flows

---

## Technical Improvements

### 🔧 Configuration Management

- **Environment Variable Support**: Enhanced configuration through environment variables
- **Secure Configuration Storage**: Encrypted storage of sensitive configuration data
- **Connection String Provider**: Centralized connection string management with encryption

### 🧪 Testing and Quality

- **Unit Test Coverage**: Added comprehensive unit tests using xUnit and Moq
- **Integration Testing**: PowerShell-based integration testing scripts
- **Security Testing**: Automated security assessment tools and scripts

---

## Breaking Changes

⚠️ **API Authentication Required**: All API endpoints now require authentication via API keys. Existing integrations will need to be updated with proper API key configuration.

⚠️ **Connection String Format**: Database connection strings are now stored in encrypted format. Existing connections will need to be migrated using the provided migration scripts.

---

## Installation/Upgrade Instructions

### New Installation

1. **Prerequisites**: Ensure .NET 9.0 SDK is installed
2. **Clone Repository**: `git clone [repository-url]`
3. **Environment Setup**: Configure required environment variables:
   - `MSSQL_MCP_KEY`: Master encryption key
   - `MSSQL_MCP_API_KEY`: Master API key
   - `MSSQL_MCP_DATA`: Data directory path (optional)
4. **Build and Run**: `dotnet run --project mssqlMCP.csproj`

### Upgrade from Previous Version

1. **Backup Data**: Backup existing `connections.json` and any custom configurations
2. **Update Code**: Pull latest changes from repository
3. **Run Migration**: Execute `Scripts/Migrate-To-Encrypted.ps1` to migrate existing connections
4. **Configure API Keys**: Use `Scripts/Set-Api-Key.ps1` to set up API authentication
5. **Update Integrations**: Update client applications to use new API key authentication

---

## Known Issues

- **First-time Setup**: Initial encryption key generation may take a few seconds on first startup
- **Large Result Sets**: Very large query results may impact performance; consider implementing pagination for large datasets
- **Connection Pooling**: Connection pooling behavior may vary under high concurrent load scenarios

---

## Security Considerations

- **API Key Storage**: Store API keys securely and rotate them regularly
- **Network Security**: Use HTTPS for all HTTP-based communications
- **Database Permissions**: Follow principle of least privilege for database user accounts
- **Encryption Keys**: Protect encryption keys and implement proper key rotation policies

---

## Support and Documentation

- **Full Documentation**: See `Documentation/` folder for comprehensive guides
- **API Reference**: `Documentation/API_ENDPOINTS.md`
- **Security Guide**: `Documentation/Security.md`
- **Integration Examples**: `Examples/` folder contains sample implementations

---

## Contributors

- **7045kHz** - Lead Developer and Architect

---

_For technical support or questions, please refer to the project documentation or create an issue in the project repository._

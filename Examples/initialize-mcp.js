// Sample initialization and usage of the SQL Server MCP in VS Code
// This example demonstrates all 9 available MCP tools for SQL Server database connectivity

// ===== CORE SQL SERVER TOOLS =====


// Improvements needed in code
// Initialize a connection
async function initializeDatabase(connectionName = "DefaultConnection") {
    try {
        const response = await f1e_Initialize({ connectionName: connectionName });
        console.log("Initialization successful:", response);
        return response;
    } catch (error) {
        console.error("Error initializing database:", error);
        throw error;
    }
}

// Execute SQL queries and get results as JSON
async function executeQuery(query, connectionName = "DefaultConnection") {
    try {
        const result = await f1e_ExecuteQuery({
            query: query,
            connectionName: connectionName
        });
        console.log("Query result:", result);
        return result;
    } catch (error) {
        console.error("Error executing query:", error);
        throw error;
    }
}

// Get detailed table metadata with columns, primary keys, and foreign keys
async function getTableMetadata(connectionName = "DefaultConnection", schema = "dbo") {
    try {
        const metadata = await f1e_GetTableMetadata({
            connectionName: connectionName,
            schema: schema
        });
        console.log("Table metadata:", metadata);
        return metadata;
    } catch (error) {
        console.error("Error getting table metadata:", error);
        throw error;
    }
}

// Get all database objects including tables and views with optional view filtering
async function getDatabaseObjectsMetadata(connectionName = "DefaultConnection", includeViews = true, schema = null) {
    try {
        const params = {
            connectionName: connectionName,
            includeViews: includeViews
        };
        if (schema) {
            params.schema = schema;
        }
        
        const metadata = await f1e_GetDatabaseObjectsMetadata(params);
        console.log("Database objects metadata:", metadata);
        return metadata;
    } catch (error) {
        console.error("Error getting database objects metadata:", error);
        throw error;
    }
}

// Get database objects filtered by specific type
async function getDatabaseObjectsByType(objectType = "ALL", connectionName = "DefaultConnection", schema = null) {
    try {
        const params = {
            objectType: objectType,  // Options: TABLE, VIEW, PROCEDURE, FUNCTION, ALL
            connectionName: connectionName
        };
        if (schema) {
            params.schema = schema;
        }
        
        const metadata = await f1e_GetDatabaseObjectsByType(params);
        console.log(`${objectType} objects:`, metadata);
        return metadata;
    } catch (error) {
        console.error(`Error getting ${objectType} objects:`, error);
        throw error;
    }
}

// ===== SPECIALIZED METADATA TOOLS =====

// Get SQL Server Agent jobs with status and metadata
async function getSqlServerAgentJobs(connectionName = "DefaultConnection") {
    try {
        const jobs = await f1e_GetSqlServerAgentJobs({
            connectionName: connectionName
        });
        console.log("SQL Server Agent jobs:", jobs);
        return jobs;
    } catch (error) {
        console.error("Error getting SQL Server Agent jobs:", error);
        throw error;
    }
}

// Get detailed information for a specific SQL Server Agent job
async function getSqlServerAgentJobDetails(jobName, connectionName = "DefaultConnection") {
    try {
        const jobDetails = await f1e_GetSqlServerAgentJobDetails({
            jobName: jobName,
            connectionName: connectionName
        });
        console.log(`Job details for '${jobName}':`, jobDetails);
        return jobDetails;
    } catch (error) {
        console.error(`Error getting job details for '${jobName}':`, error);
        throw error;
    }
}

// Get SSIS catalog information including projects and packages
async function getSsisCatalogInfo(connectionName = "DefaultConnection") {
    try {
        const ssisInfo = await f1e_GetSsisCatalogInfo({
            connectionName: connectionName
        });
        console.log("SSIS catalog information:", ssisInfo);
        return ssisInfo;
    } catch (error) {
        console.error("Error getting SSIS catalog information:", error);
        throw error;
    }
}

// Get Azure DevOps analytics and information
async function getAzureDevOpsInfo(connectionName = "DefaultConnection") {
    try {
        const azureDevOpsInfo = await f1e_GetAzureDevOpsInfo({
            connectionName: connectionName
        });
        console.log("Azure DevOps information:", azureDevOpsInfo);
        return azureDevOpsInfo;
    } catch (error) {
        console.error("Error getting Azure DevOps information:", error);
        throw error;
    }
}

// ===== CONNECTION MANAGEMENT & SECURITY TOOLS =====

// Add a new database connection
async function addConnection(name, connectionString, description = null) {
    try {
        const result = await f1e_AddConnection({
            request: {
                name: name,
                connectionString: connectionString,
                description: description
            }
        });
        console.log("Connection added:", result);
        return result;
    } catch (error) {
        console.error("Error adding connection:", error);
        throw error;
    }
}

// Update an existing database connection
async function updateConnection(name, connectionString, description = null) {
    try {
        const result = await f1e_UpdateConnection({
            request: {
                name: name,
                connectionString: connectionString,
                description: description
            }
        });
        console.log("Connection updated:", result);
        return result;
    } catch (error) {
        console.error("Error updating connection:", error);
        throw error;
    }
}

// Remove a database connection
async function removeConnection(name) {
    try {
        const result = await f1e_RemoveConnection({
            request: {
                name: name
            }
        });
        console.log("Connection removed:", result);
        return result;
    } catch (error) {
        console.error("Error removing connection:", error);
        throw error;
    }
}

// List all available database connections
async function listConnections() {
    try {
        const connections = await f1e_ListConnections({});
        console.log("Available connections:", connections);
        return connections;
    } catch (error) {
        console.error("Error listing connections:", error);
        throw error;
    }
}

// Test a database connection
async function testConnection(connectionString) {
    try {
        const result = await f1e_TestConnection({
            request: {
                connectionString: connectionString
            }
        });
        console.log("Connection test result:", result);
        return result;
    } catch (error) {
        console.error("Error testing connection:", error);
        throw error;
    }
}

// Generate a secure encryption key
async function generateSecureKey(length = 32) {
    try {
        const key = await f1e_GenerateSecureKey({
            length: length
        });
        console.log("Generated secure key:", key);
        return key;
    } catch (error) {
        console.error("Error generating secure key:", error);
        throw error;
    }
}

// Migrate connections to encrypted format
async function migrateConnectionsToEncrypted() {
    try {
        const result = await f1e_MigrateConnectionsToEncrypted({});
        console.log("Migration result:", result);
        return result;
    } catch (error) {
        console.error("Error migrating connections:", error);
        throw error;
    }
}

// Rotate encryption key
async function rotateKey(newKey) {
    try {
        const result = await f1e_RotateKey({
            newKey: newKey
        });
        console.log("Key rotation result:", result);
        return result;
    } catch (error) {
        console.error("Error rotating key:", error);
        throw error;
    }
}

// ===== COMPREHENSIVE EXAMPLES =====

// Example: Complete database analysis workflow
async function analyzeDatabaseComprehensively(connectionName = "DefaultConnection") {
    try {
        console.log("=== Starting comprehensive database analysis ===");
        
        // 1. Initialize connection
        await initializeDatabase(connectionName);
        
        // 2. Get basic database information
        const databases = await executeQuery("SELECT name, database_id, create_date FROM sys.databases", connectionName);
        console.log("Available databases:", databases);
        
        // 3. Get all database objects overview
        const allObjects = await getDatabaseObjectsMetadata(connectionName, true);
        console.log("Total objects found:", allObjects?.length || 0);
        
        // 4. Get specific object types
        const tables = await getDatabaseObjectsByType("TABLE", connectionName);
        const views = await getDatabaseObjectsByType("VIEW", connectionName);
        const procedures = await getDatabaseObjectsByType("PROCEDURE", connectionName);
        const functions = await getDatabaseObjectsByType("FUNCTION", connectionName);
        
        console.log(`Database contains: ${tables?.length || 0} tables, ${views?.length || 0} views, ${procedures?.length || 0} procedures, ${functions?.length || 0} functions`);
        
        // 5. Get detailed table metadata for main schema
        const tableMetadata = await getTableMetadata(connectionName, "dbo");
        
        // 6. Check SQL Server Agent jobs (if available)
        try {
            const agentJobs = await getSqlServerAgentJobs(connectionName);
            console.log(`Found ${agentJobs?.length || 0} SQL Server Agent jobs`);
        } catch (error) {
            console.log("SQL Server Agent not available or no jobs found");
        }
        
        // 7. Check SSIS catalog (if available)
        try {
            const ssisInfo = await getSsisCatalogInfo(connectionName);
            console.log("SSIS catalog information retrieved");
        } catch (error) {
            console.log("SSIS catalog not available");
        }
        
        // 8. Check Azure DevOps integration (if available)
        try {
            const azureDevOpsInfo = await getAzureDevOpsInfo(connectionName);
            console.log("Azure DevOps information retrieved");
        } catch (error) {
            console.log("Azure DevOps integration not available");
        }
        
        console.log("=== Database analysis completed ===");
        
        return {
            databases,
            allObjects,
            tables,
            views,
            procedures,
            functions,
            tableMetadata
        };
        
    } catch (error) {
        console.error("Error in comprehensive analysis:", error);
        throw error;
    }
}

// Example: SQL Server Agent job monitoring
async function monitorSqlServerAgentJobs(connectionName = "DefaultConnection") {
    try {
        console.log("=== Monitoring SQL Server Agent Jobs ===");
        
        // Get all jobs
        const jobs = await getSqlServerAgentJobs(connectionName);
        
        if (!jobs || jobs.length === 0) {
            console.log("No SQL Server Agent jobs found");
            return;
        }
        
        console.log(`Found ${jobs.length} jobs`);
        
        // Get details for each job
        for (const job of jobs) {
            if (job.name) {
                try {
                    const details = await getSqlServerAgentJobDetails(job.name, connectionName);
                    console.log(`Job '${job.name}' has ${details?.steps?.length || 0} steps and ${details?.schedules?.length || 0} schedules`);
                } catch (error) {
                    console.log(`Could not get details for job '${job.name}':`, error.message);
                }
            }
        }
        
        return jobs;
        
    } catch (error) {
        console.error("Error monitoring SQL Server Agent jobs:", error);
        throw error;
    }
}

// Example: Connection management workflow
async function demonstrateConnectionManagement() {
    try {
        console.log("=== Connection Management Demo ===");
        
        // List existing connections
        const existingConnections = await listConnections();
        console.log("Existing connections:", existingConnections);
        
        // Test connection string
        const testConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;TrustServerCertificate=true;";
        const testResult = await testConnection(testConnectionString);
        console.log("Connection test:", testResult);
        
        // Generate a secure key for encryption
        const secureKey = await generateSecureKey(32);
        console.log("Generated secure key length:", secureKey?.length || 0);
        
        // Add a new connection (example - adjust connection string as needed)
        // const newConnection = await addConnection("TestConnection", testConnectionString, "Test database connection");
        
        // Update connection
        // const updatedConnection = await updateConnection("TestConnection", testConnectionString, "Updated test database connection");
        
        // Remove connection
        // const removeResult = await removeConnection("TestConnection");
        
        return {
            existingConnections,
            testResult,
            secureKey
        };
        
    } catch (error) {
        console.error("Error in connection management demo:", error);
        throw error;
    }
}

// ===== QUICK START EXAMPLES =====

// Simple usage for Copilot integration
async function quickStart() {
    try {
        // Initialize and get basic information
        await initializeDatabase();
        
        // Get database structure overview
        const objects = await getDatabaseObjectsMetadata();
        console.log(`Database contains ${objects?.length || 0} objects`);
        
        // Get tables in the main schema
        const tables = await getDatabaseObjectsByType("TABLE");
        console.log(`Found ${tables?.length || 0} tables`);
        
        // Execute a simple query
        const result = await executeQuery("SELECT @@VERSION as SqlServerVersion");
        console.log("SQL Server Version:", result);
        
        return { objects, tables, result };
    } catch (error) {
        console.error("Quick start failed:", error);
        throw error;
    }
}

// Export all functions for use in Copilot and other applications
module.exports = {
    // Core SQL Server Tools
    initializeDatabase,
    executeQuery,
    getTableMetadata,
    getDatabaseObjectsMetadata,
    getDatabaseObjectsByType,
    
    // Specialized Metadata Tools
    getSqlServerAgentJobs,
    getSqlServerAgentJobDetails,
    getSsisCatalogInfo,
    getAzureDevOpsInfo,
    
    // Connection Management & Security Tools
    addConnection,
    updateConnection,
    removeConnection,
    listConnections,
    testConnection,
    generateSecureKey,
    migrateConnectionsToEncrypted,
    rotateKey,
    
    // Comprehensive Examples
    analyzeDatabaseComprehensively,
    monitorSqlServerAgentJobs,
    demonstrateConnectionManagement,
    quickStart
};

// ===== USAGE NOTES =====
/*
All MCP tools are available with the f1e_ prefix:

CORE SQL SERVER TOOLS:
- f1e_Initialize: Initialize database connection
- f1e_ExecuteQuery: Execute SQL queries with JSON results
- f1e_GetTableMetadata: Get detailed table metadata with relationships
- f1e_GetDatabaseObjectsMetadata: Get all database objects with optional view filtering
- f1e_GetDatabaseObjectsByType: Get objects filtered by type (TABLE, VIEW, PROCEDURE, FUNCTION, ALL)

SPECIALIZED METADATA TOOLS:
- f1e_GetSqlServerAgentJobs: Get SQL Server Agent job metadata
- f1e_GetSqlServerAgentJobDetails: Get detailed job information including steps and schedules
- f1e_GetSsisCatalogInfo: Get SSIS catalog projects and packages
- f1e_GetAzureDevOpsInfo: Get Azure DevOps analytics and information

CONNECTION MANAGEMENT & SECURITY TOOLS:
- f1e_AddConnection: Add new database connection
- f1e_UpdateConnection: Update existing connection
- f1e_RemoveConnection: Remove database connection
- f1e_ListConnections: List all available connections
- f1e_TestConnection: Test connection string validity
- f1e_GenerateSecureKey: Generate secure encryption keys
- f1e_MigrateConnectionsToEncrypted: Migrate to encrypted connection strings
- f1e_RotateKey: Rotate encryption keys for security

EXAMPLE USAGE IN COPILOT:
1. Quick database overview: quickStart()
2. Full analysis: analyzeDatabaseComprehensively()
3. Job monitoring: monitorSqlServerAgentJobs()
4. Connection setup: demonstrateConnectionManagement()

All functions use async/await and include comprehensive error handling.
Connection management includes encryption and security features.
Specialized tools provide enterprise-level database insights.
*/

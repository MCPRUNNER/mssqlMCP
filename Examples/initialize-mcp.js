// Sample initialization and usage of the SQL Server MCP in VS Code

// Initialize a connection
async function initializeDatabase() {
    try {
        // Initialize with default connection
        const response = await f1e_initialize({ connectionName: "DefaultConnection" });
        console.log("Initialization successful:", response);
        return response;
    } catch (error) {
        console.error("Error initializing database:", error);
        throw error;
    }
}

// Execute a simple query
async function executeSimpleQuery(connectionName = "DefaultConnection") {
    try {
        const query = "SELECT name, database_id, create_date FROM sys.databases";
        const result = await f1e_executeQuery({
            connectionName: connectionName,
            query: query
        });
        console.log("Query result:", result);
        return result;
    } catch (error) {
        console.error("Error executing query:", error);
        throw error;
    }
}

// Get metadata for tables in a specific schema
async function getSchemaMetadata(connectionName = "DefaultConnection", schema = "dbo") {
    try {
        const metadata = await f1e_getTableMetadata({
            connectionName: connectionName,
            schema: schema
        });
        console.log("Table metadata:", metadata);
        return metadata;
    } catch (error) {
        console.error("Error getting metadata:", error);
        throw error;
    }
}

// Get metadata for specific object types including views
async function getDatabaseObjects(connectionName = "DefaultConnection", objectType = "ALL") {
    try {
        const metadata = await f1e_getDatabaseObjectsMetadata({
            connectionName: connectionName,
            objectType: objectType,
            includeViews: true
        });
        console.log("Database objects:", metadata);
        return metadata;
    } catch (error) {
        console.error("Error getting database objects:", error);
        throw error;
    }
}

// Add a new connection
async function addConnection(name, connectionString, description = "") {
    try {
        const result = await f1e_connectionManager_add({
            Name: name,
            ConnectionString: connectionString,
            Description: description
        });
        console.log("Connection added:", result);
        return result;
    } catch (error) {
        console.error("Error adding connection:", error);
        throw error;
    }
}

// Example usage flow:
// 1. Initialize database connection
// 2. Get metadata to understand the database structure
// 3. Execute a query based on the metadata

// initializeDatabase()
//     .then(() => getSchemaMetadata("DefaultConnection", "dbo"))
//     .then(() => executeSimpleQuery("DefaultConnection"))
//     .catch(error => console.error("Error in workflow:", error));

// Export functions for use in Copilot
module.exports = {
    initializeDatabase,
    executeSimpleQuery,
    getSchemaMetadata,
    getDatabaseObjects,
    addConnection
};

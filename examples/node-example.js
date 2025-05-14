// SQL Server MCP Example in Node.js
// This demonstrates how to connect to the MCP server from a Node.js application

const fetch = require('node-fetch');
require('dotenv').config();

// MCP Server URL
const MCP_URL = 'http://localhost:3001/mcp';

// Function to call the MCP server
async function callMcp(tool, params = {}) {
  try {
    const response = await fetch(MCP_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tool,
        params,
      }),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error(`Error calling MCP tool ${tool}:`, error);
    throw error;
  }
}

async function runExample() {
  try {
    // Initialize the connection
    console.log('Initializing SQL Server connection...');
    const initResult = await callMcp('initialize');
    console.log('Initialization result:', initResult);

    // Echo test
    console.log('\nTesting echo functionality...');
    const echoResult = await callMcp('echo', { message: 'Hello from Node.js!' });
    console.log('Echo result:', echoResult);

    // Get table metadata
    console.log('\nRetrieving database metadata...');
    const metadataResult = await callMcp('getTableMetadata');
    console.log('Database has', JSON.parse(metadataResult).length, 'tables');
    
    // Execute a simple SQL query
    console.log('\nExecuting SQL query...');
    const queryResult = await callMcp('executeQuery', { 
      query: 'SELECT TOP 5 * FROM sys.tables' 
    });
    console.log('Query result:', queryResult);

    console.log('\nExample completed successfully!');
  } catch (error) {
    console.error('Example failed:', error);
  }
}

// Run the example
runExample();

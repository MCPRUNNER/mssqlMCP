#!/usr/bin/env python3
"""
SQL Server MCP Example in Python
This demonstrates how to connect to the MCP server from a Python application
"""

import json
import requests
from typing import Any, Dict, Optional

# MCP Server URL
MCP_URL = "http://localhost:3001/mcp"

def call_mcp(tool: str, params: Optional[Dict[str, Any]] = None) -> Any:
    """Call an MCP tool with the given parameters"""
    if params is None:
        params = {}
        
    try:
        response = requests.post(
            MCP_URL,
            json={
                "tool": tool,
                "params": params
            },
            headers={"Content-Type": "application/json"}
        )
        
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error calling MCP tool {tool}: {e}")
        raise

def run_example():
    """Run the example to demonstrate MCP usage"""
    try:
        # Initialize the connection
        print("Initializing SQL Server connection...")
        init_result = call_mcp("initialize")
        print(f"Initialization result: {init_result}")

        # Echo test
        print("\nTesting echo functionality...")
        echo_result = call_mcp("echo", {"message": "Hello from Python!"})
        print(f"Echo result: {echo_result}")

        # Get table metadata
        print("\nRetrieving database metadata...")
        metadata_result = call_mcp("getTableMetadata")
        tables = json.loads(metadata_result)
        print(f"Database has {len(tables)} tables")
        
        # Execute a simple SQL query
        print("\nExecuting SQL query...")
        query_result = call_mcp("executeQuery", {
            "query": "SELECT TOP 5 * FROM sys.tables"
        })
        print(f"Query result: {query_result}")

        print("\nExample completed successfully!")
    except Exception as e:
        print(f"Example failed: {e}")

if __name__ == "__main__":
    run_example()

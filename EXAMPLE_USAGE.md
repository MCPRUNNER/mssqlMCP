# SQL Database Schema Analysis and Visualization

Based on the database schema retrieved from `#GetTableMetadata connectionName=PROTO`, I'll provide a comprehensive analysis and visualization of the database structure.

## Copilot Prompt used to create this
Recommend making sure you have something like `Markdown Preview Mermaid Support` installed, so you can view the graphs.
```
summarize data from #GetTableMetadata connectionName=PROTO and create mermaid graphs for all sql objects discovered

```

## Database Overview

The PROTO database contains 18 tables distributed across 5 schemas:

- **dbo** (6 tables): Cross-database tracking and SSIS package information
- **ETL** (2 tables): Event tracking for Ansible and Kafka
- **Holler** (4 tables): API configuration and execution monitoring
- **SSIS** (5 tables): SQL Server Integration Services metadata
- **temp** (1 table): Test/temporary tables

## Schema Diagrams

### dbo Schema

```mermaid
erDiagram
    cross_db_databases {
        int database_id
        nvarchar database_name
        int state
        nvarchar IsActive
    }
    cross_db_objects {
        nvarchar object_servername
        nvarchar object_servicename
        nvarchar object_database
        int object_id
        nvarchar object_schema
        nvarchar object_name
        nvarchar object_type
        nvarchar object_desc
        datetime LoadDate
    }
    cross_db_reference {
        nvarchar referencing_servername
        nvarchar referencing_servicename
        nvarchar referencing_database
        int referencing_id
        nvarchar referencing_schema
        nvarchar referencing_object_name
        nvarchar referencing_object_type
        nvarchar referenced_server
        nvarchar referenced_database
        int referenced_id
        nvarchar referenced_schema
        nvarchar referenced_object_name
        nvarchar referenced_object_type
        datetime LoadDate
    }
    cross_db_server_databases {
        nvarchar server_name
        nvarchar database_name
        nvarchar etl_connecton_string
        nvarchar IsActive
    }
    dtsx_info {
        datetime scan_date
        int dtsx_id
        nvarchar dtsx_path
        varchar dtsx_name
        int item_id
        varchar item_type
        int field_id
        varchar field_name
        varchar value
        varchar linked_item_type
    }
    T {
        int Id PK
        nvarchar t1
        nvarchar t2
        int n1
        int n2
        datetime d1
        datetime d2
    }
```

### ETL Schema

```mermaid
erDiagram
    AnsibleEvents {
        int Id PK
        nvarchar Topic
        int Partition
        int Offset
        datetime Timestamp
        nvarchar Key
        nvarchar Value
        datetime LoadDate
    }
    KafkaEvents {
        int Id PK
        nvarchar Topic
        int Partition
        int Offset
        datetime Timestamp
        nvarchar Key
        nvarchar Value
        datetime LoadDate
    }
```

### Holler Schema (with Relationships)

```mermaid
erDiagram
    ApiConfigurations {
        int Id PK
        nvarchar Name
        nvarchar Description
        nvarchar Category
        nvarchar Url
        nvarchar HttpMethod
        nvarchar HeadersJson
        nvarchar AuthenticationMethod
        varbinary EncryptedAuthenticationValue
    }
    DatasetConfigurations {
        int Id PK
        nvarchar Name
        nvarchar Description
        nvarchar Category
        nvarchar Query
    }
    ExecutionRuns {
        uniqueidentifier ExecutionId PK
        datetime2 RunTimestamp
        int ApiConfigurationId FK
        int DatasetConfigurationId FK
        nvarchar RunnerName
        int ParallelCount
        int ParallelSleepMs
        int RerunCount
        int RerunSleepMs
    }
    ExecutionResults {
        bigint ResultId PK
        uniqueidentifier ExecutionId FK
        nvarchar RequestUrl
        nvarchar RequestBody
        nvarchar HttpMethod
        int StatusCode
        nvarchar Content
        datetime2 StartTimeUtc
        datetime2 EndTimeUtc
        nvarchar ErrorMessage
    }

    ApiConfigurations ||--o{ ExecutionRuns : "has"
    DatasetConfigurations ||--o{ ExecutionRuns : "has"
    ExecutionRuns ||--o{ ExecutionResults : "has"
```

### SSIS Schema

```mermaid
erDiagram
    DTSX_Attributes {
        int Id PK
        nvarchar CreationName
        nvarchar Description
        nvarchar Filename
        nvarchar Package
        nvarchar ParentNodeDtsId
        nvarchar ParentNodeName
        nvarchar ParentNodeType
        nvarchar ParentGUID
        nvarchar GUID
        nvarchar ParentRefId
        nvarchar RefId
        nvarchar XPath
        nvarchar ElementXPath
        nvarchar AttributeName
        nvarchar AttributeType
        nvarchar AttributeValue
        datetime LoadDate
    }
    DTSX_Elements {
        int Id PK
        nvarchar CreationName
        nvarchar Description
        nvarchar Filename
        nvarchar Package
        nvarchar ParentNodeDtsId
        nvarchar ParentNodeName
        nvarchar ParentNodeType
        nvarchar ParentGUID
        nvarchar GUID
        nvarchar ParentRefId
        nvarchar RefId
        nvarchar XPath
        nvarchar DtsId
        nvarchar Name
        nvarchar NodeType
        nvarchar Value
        nvarchar XmlType
        datetime LoadDate
    }
    DTSX_Mapper {
        int Id PK
        nvarchar Description
        nvarchar Package
        nvarchar RefId
        nvarchar SqlStatement
        nvarchar ConnectionString
        nvarchar ConnectionName
        nvarchar ConnectionDtsId
        nvarchar ConnectionType
        nvarchar ConnectionRefId
        nvarchar Name
        nvarchar ComponentType
        datetime LoadDate
    }
    DTSX_Variables {
        int Id PK
        nvarchar CreationName
        nvarchar Description
        nvarchar Filename
        nvarchar Package
        nvarchar ParentNodeDtsId
        nvarchar ParentNodeName
        nvarchar ParentNodeType
        nvarchar ParentGUID
        nvarchar GUID
        nvarchar ParentRefId
        nvarchar RefId
        nvarchar XPath
        nvarchar EvaluateAsExpression
        nvarchar IncludeInDebugDump
        nvarchar VariableDataType
        nvarchar VariableDtsxId
        nvarchar VariableExpression
        nvarchar VariableName
        nvarchar VariableNameSpace
        nvarchar VariableValue
        datetime LoadDate
    }
    VarConnect {
        varchar Name
    }
```

### temp Schema

```mermaid
erDiagram
    Department {
        int DepartmentID
        nvarchar Name
        nvarchar GroupName
        datetime ModifiedDate
    }
```

## Cross-Schema Relationships Diagram

```mermaid
flowchart TD
    subgraph dbo
        cross_db_databases
        cross_db_objects
        cross_db_reference
        cross_db_server_databases
        dtsx_info
        T
    end

    subgraph ETL
        AnsibleEvents
        KafkaEvents
    end

    subgraph Holler
        ApiConfigurations
        DatasetConfigurations
        ExecutionRuns
        ExecutionResults
    end

    subgraph SSIS
        DTSX_Attributes
        DTSX_Elements
        DTSX_Mapper
        DTSX_Variables
        VarConnect
    end

    subgraph temp
        Department
    end

    ExecutionRuns -->|FK_ApiConfigId| ApiConfigurations
    ExecutionRuns -->|FK_DatasetConfigId| DatasetConfigurations
    ExecutionResults -->|FK_ExecutionId| ExecutionRuns

    dtsx_info -.->|logical relationship| DTSX_Elements
    dtsx_info -.->|logical relationship| DTSX_Attributes
    dtsx_info -.->|logical relationship| DTSX_Mapper
    dtsx_info -.->|logical relationship| DTSX_Variables

    cross_db_databases -.->|logical relationship| cross_db_objects
    cross_db_databases -.->|logical relationship| cross_db_reference
    cross_db_reference -.->|logical relationship| cross_db_objects
```

## Data Flow Diagram

```mermaid
flowchart LR
    ETL_Sources[External ETL Sources] -->|events| ETL.KafkaEvents
    ETL_Sources -->|events| ETL.AnsibleEvents

    API_Consumer[API Consumers] -->|calls| Holler.ApiConfigurations
    Holler.DatasetConfigurations -->|queries data for| Holler.ExecutionRuns
    Holler.ApiConfigurations -->|used by| Holler.ExecutionRuns
    Holler.ExecutionRuns -->|produces| Holler.ExecutionResults

    SSIS_Packages[SSIS Packages] -->|metadata| SSIS.DTSX_Elements
    SSIS_Packages -->|metadata| SSIS.DTSX_Attributes
    SSIS_Packages -->|mapping info| SSIS.DTSX_Mapper
    SSIS_Packages -->|variables| SSIS.DTSX_Variables
    SSIS_Packages -->|package info| dbo.dtsx_info

    DB_Sources[External Databases] -->|metadata| dbo.cross_db_databases
    DB_Sources -->|object info| dbo.cross_db_objects
    DB_Sources -->|references| dbo.cross_db_reference
    dbo.cross_db_server_databases -->|connection info| DB_Sources
```

## Key Insights

1. **Holler Schema**: Implements a robust API orchestration system with proper foreign key relationships between tables.

2. **ETL System**: Tracks events from both Kafka and Ansible with identical table structures, suggesting a unified event processing approach.

3. **Cross-Database Tracking**: The dbo schema contains tables for tracking database objects across multiple databases, indicating a centralized metadata repository.

4. **SSIS Metadata**: Extensive tracking of SSIS package components across multiple tables, likely for auditing or documentation purposes.

5. **Testing Area**: The temp schema with a single Department table suggests an area for testing or development.

6. **Sample Table**: The table T in dbo schema contains various data types and is the only table in dbo with a primary key, suggesting it may be used for testing or as a template.

The database appears to implement a comprehensive system for tracking cross-database dependencies, monitoring API executions, and documenting ETL processes with SSIS, with proper separation of concerns across different schemas.

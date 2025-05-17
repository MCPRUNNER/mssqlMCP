using Xunit;
using mssqlMCP.Models;
using System.Collections.Generic;

namespace mssqlMCP.Tests.Models
{
    public class TableInfoTests
    {
        [Fact]
        public void TableInfo_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var tableInfo = new TableInfo();

            // Assert
            Assert.Equal(string.Empty, tableInfo.Schema);
            Assert.Equal(string.Empty, tableInfo.Name);
            Assert.Equal("BASE TABLE", tableInfo.ObjectType);
            Assert.Null(tableInfo.Definition);
            Assert.Null(tableInfo.Properties);
            Assert.NotNull(tableInfo.Columns);
            Assert.Empty(tableInfo.Columns);
            Assert.NotNull(tableInfo.PrimaryKeys);
            Assert.Empty(tableInfo.PrimaryKeys);
            Assert.NotNull(tableInfo.ForeignKeys);
            Assert.Empty(tableInfo.ForeignKeys);
        }

        [Fact]
        public void TableInfo_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var schema = "dbo";
            var name = "Customers";
            var objectType = "BASE TABLE";
            var definition = "CREATE TABLE [dbo].[Customers]...";
            var properties = new Dictionary<string, string>
            {
                { "TableRowCount", "1500" },
                { "IndexCount", "3" }
            };

            var columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "CustomerId", DataType = "int", IsPrimaryKey = true },
                new ColumnInfo { Name = "CustomerName", DataType = "nvarchar" }
            };

            var primaryKeys = new List<string> { "CustomerId" };

            var foreignKeys = new List<ForeignKeyInfo>
            {
                new ForeignKeyInfo
                {
                    Name = "FK_Orders_Customers",
                    Column = "CustomerId",
                    ReferencedSchema = "dbo",
                    ReferencedTable = "Orders",
                    ReferencedColumn = "CustomerId"
                }
            };

            // Act
            var tableInfo = new TableInfo
            {
                Schema = schema,
                Name = name,
                ObjectType = objectType,
                Definition = definition,
                Properties = properties,
                Columns = columns,
                PrimaryKeys = primaryKeys,
                ForeignKeys = foreignKeys
            };

            // Assert
            Assert.Equal(schema, tableInfo.Schema);
            Assert.Equal(name, tableInfo.Name);
            Assert.Equal(objectType, tableInfo.ObjectType);
            Assert.Equal(definition, tableInfo.Definition);
            Assert.Equal(properties, tableInfo.Properties);
            Assert.Equal(columns, tableInfo.Columns);
            Assert.Equal(2, tableInfo.Columns.Count);
            Assert.Equal(primaryKeys, tableInfo.PrimaryKeys);
            Assert.Single(tableInfo.PrimaryKeys);
            Assert.Equal(foreignKeys, tableInfo.ForeignKeys);
            Assert.Single(tableInfo.ForeignKeys);
        }
    }
}

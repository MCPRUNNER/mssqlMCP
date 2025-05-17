using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class ForeignKeyInfoTests
    {
        [Fact]
        public void ForeignKeyInfo_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var foreignKey = new ForeignKeyInfo();

            // Assert
            Assert.Equal(string.Empty, foreignKey.Name);
            Assert.Equal(string.Empty, foreignKey.Column);
            Assert.Equal(string.Empty, foreignKey.ReferencedSchema);
            Assert.Equal(string.Empty, foreignKey.ReferencedTable);
            Assert.Equal(string.Empty, foreignKey.ReferencedColumn);
        }

        [Fact]
        public void ForeignKeyInfo_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "FK_Orders_Customers";
            var column = "CustomerID";
            var referencedSchema = "dbo";
            var referencedTable = "Customers";
            var referencedColumn = "CustomerID";

            // Act
            var foreignKey = new ForeignKeyInfo
            {
                Name = name,
                Column = column,
                ReferencedSchema = referencedSchema,
                ReferencedTable = referencedTable,
                ReferencedColumn = referencedColumn
            };

            // Assert
            Assert.Equal(name, foreignKey.Name);
            Assert.Equal(column, foreignKey.Column);
            Assert.Equal(referencedSchema, foreignKey.ReferencedSchema);
            Assert.Equal(referencedTable, foreignKey.ReferencedTable);
            Assert.Equal(referencedColumn, foreignKey.ReferencedColumn);
        }
    }
}

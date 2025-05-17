using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class ColumnInfoTests
    {
        [Fact]
        public void ColumnInfo_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var column = new ColumnInfo();

            // Assert
            Assert.Equal(string.Empty, column.Name);
            Assert.Equal(string.Empty, column.DataType);
            Assert.False(column.IsNullable);
            Assert.Null(column.MaxLength);
            Assert.Null(column.Precision);
            Assert.Null(column.Scale);
            Assert.Null(column.DefaultValue);
            Assert.False(column.IsPrimaryKey);
            Assert.False(column.IsForeignKey);
            Assert.Null(column.ForeignKeyReference);
            Assert.Null(column.Description);
        }

        [Fact]
        public void ColumnInfo_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "CustomerId";
            var dataType = "int";
            var isNullable = false;
            var maxLength = 4;
            var precision = 10;
            var scale = 0;
            var defaultValue = "NEXT VALUE FOR [dbo].[CustomerIdSequence]";
            var isPrimaryKey = true;
            var isForeignKey = false;
            var foreignKeyReference = (string?)null;
            var description = "Primary key for Customer table";

            // Act
            var column = new ColumnInfo
            {
                Name = name,
                DataType = dataType,
                IsNullable = isNullable,
                MaxLength = maxLength,
                Precision = precision,
                Scale = scale,
                DefaultValue = defaultValue,
                IsPrimaryKey = isPrimaryKey,
                IsForeignKey = isForeignKey,
                ForeignKeyReference = foreignKeyReference,
                Description = description
            };

            // Assert
            Assert.Equal(name, column.Name);
            Assert.Equal(dataType, column.DataType);
            Assert.Equal(isNullable, column.IsNullable);
            Assert.Equal(maxLength, column.MaxLength);
            Assert.Equal(precision, column.Precision);
            Assert.Equal(scale, column.Scale);
            Assert.Equal(defaultValue, column.DefaultValue);
            Assert.Equal(isPrimaryKey, column.IsPrimaryKey);
            Assert.Equal(isForeignKey, column.IsForeignKey);
            Assert.Equal(foreignKeyReference, column.ForeignKeyReference);
            Assert.Equal(description, column.Description);
        }
    }
}

using Xunit;
using mssqlMCP.Models;
using System;

namespace mssqlMCP.Models.Tests
{
    public class AzureDevOpsWorkItemTests
    {
        [Fact]
        public void Id_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedId = 12345;

            // Act
            workItem.Id = expectedId;

            // Assert
            Assert.Equal(expectedId, workItem.Id);
        }

        [Fact]
        public void Id_DefaultValue_IsZero()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(0, workItem.Id);
        }

        [Fact]
        public void Title_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedTitle = "Fix critical bug in login module";

            // Act
            workItem.Title = expectedTitle;

            // Assert
            Assert.Equal(expectedTitle, workItem.Title);
        }

        [Fact]
        public void Title_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.Title);
        }

        [Fact]
        public void WorkItemType_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedType = "Bug";

            // Act
            workItem.WorkItemType = expectedType;

            // Assert
            Assert.Equal(expectedType, workItem.WorkItemType);
        }

        [Fact]
        public void WorkItemType_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.WorkItemType);
        }

        [Fact]
        public void State_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedState = "Active";

            // Act
            workItem.State = expectedState;

            // Assert
            Assert.Equal(expectedState, workItem.State);
        }

        [Fact]
        public void State_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.State);
        }

        [Fact]
        public void AssignedTo_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedAssignee = "John Doe";

            // Act
            workItem.AssignedTo = expectedAssignee;

            // Assert
            Assert.Equal(expectedAssignee, workItem.AssignedTo);
        }

        [Fact]
        public void AssignedTo_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.AssignedTo);
        }

        [Fact]
        public void CreatedBy_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedCreator = "Jane Smith";

            // Act
            workItem.CreatedBy = expectedCreator;

            // Assert
            Assert.Equal(expectedCreator, workItem.CreatedBy);
        }

        [Fact]
        public void CreatedBy_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.CreatedBy);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedDate = DateTime.UtcNow.AddDays(-7);

            // Act
            workItem.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, workItem.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(default(DateTime), workItem.CreatedDate);
        }

        [Fact]
        public void ChangedDate_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            DateTime? expectedDate = DateTime.UtcNow.AddDays(-1);

            // Act
            workItem.ChangedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, workItem.ChangedDate);

            // Act
            workItem.ChangedDate = null;

            // Assert
            Assert.Null(workItem.ChangedDate);
        }

        [Fact]
        public void ChangedDate_DefaultValue_IsNull()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Null(workItem.ChangedDate);
        }

        [Fact]
        public void ProjectId_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedId = Guid.NewGuid();

            // Act
            workItem.ProjectId = expectedId;

            // Assert
            Assert.Equal(expectedId, workItem.ProjectId);
        }

        [Fact]
        public void ProjectId_DefaultValue_IsGuidEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(Guid.Empty, workItem.ProjectId);
        }

        [Fact]
        public void ProjectName_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedName = "Project Phoenix";

            // Act
            workItem.ProjectName = expectedName;

            // Assert
            Assert.Equal(expectedName, workItem.ProjectName);
        }

        [Fact]
        public void ProjectName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.ProjectName);
        }

        [Fact]
        public void AreaPath_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedPath = "ProjectPhoenix\\Backend";

            // Act
            workItem.AreaPath = expectedPath;

            // Assert
            Assert.Equal(expectedPath, workItem.AreaPath);
        }

        [Fact]
        public void AreaPath_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.AreaPath);
        }

        [Fact]
        public void IterationPath_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedPath = "ProjectPhoenix\\Sprint 3";

            // Act
            workItem.IterationPath = expectedPath;

            // Assert
            Assert.Equal(expectedPath, workItem.IterationPath);
        }

        [Fact]
        public void IterationPath_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.IterationPath);
        }

        [Fact]
        public void Tags_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedTags = "Bug; UI; Critical";

            // Act
            workItem.Tags = expectedTags;

            // Assert
            Assert.Equal(expectedTags, workItem.Tags);
        }

        [Fact]
        public void Tags_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.Tags);
        }

        [Fact]
        public void Priority_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedPriority = 1;

            // Act
            workItem.Priority = expectedPriority;

            // Assert
            Assert.Equal(expectedPriority, workItem.Priority);
        }

        [Fact]
        public void Priority_DefaultValue_IsZero()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(0, workItem.Priority);
        }

        [Fact]
        public void Severity_GetSet()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();
            var expectedSeverity = "1 - Critical";

            // Act
            workItem.Severity = expectedSeverity;

            // Assert
            Assert.Equal(expectedSeverity, workItem.Severity);
        }

        [Fact]
        public void Severity_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var workItem = new AzureDevOpsWorkItem();

            // Assert
            Assert.Equal(string.Empty, workItem.Severity);
        }
    }
}
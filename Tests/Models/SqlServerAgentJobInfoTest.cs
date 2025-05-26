using Xunit;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;

namespace mssqlMCP.Models.Tests
{
    public class SqlServerAgentJobInfoTest
    {
        [Fact]
        public void JobId_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedJobId = Guid.NewGuid();

            // Act
            jobInfo.JobId = expectedJobId;

            // Assert
            Assert.Equal(expectedJobId, jobInfo.JobId);
        }

        [Fact]
        public void JobId_DefaultValue_IsGuidEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(Guid.Empty, jobInfo.JobId);
        }

        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedName = "Test Job";

            // Act
            jobInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, jobInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(string.Empty, jobInfo.Name);
        }

        [Fact]
        public void Enabled_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Act
            jobInfo.Enabled = true;

            // Assert
            Assert.True(jobInfo.Enabled);

            // Act
            jobInfo.Enabled = false;

            // Assert
            Assert.False(jobInfo.Enabled);
        }

        [Fact]
        public void Enabled_DefaultValue_IsFalse()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.False(jobInfo.Enabled);
        }

        [Fact]
        public void Description_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedDescription = "This is a test job description.";

            // Act
            jobInfo.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, jobInfo.Description);
        }

        [Fact]
        public void Description_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(string.Empty, jobInfo.Description);
        }

        [Fact]
        public void Owner_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedOwner = "sa";

            // Act
            jobInfo.Owner = expectedOwner;

            // Assert
            Assert.Equal(expectedOwner, jobInfo.Owner);
        }

        [Fact]
        public void Owner_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(string.Empty, jobInfo.Owner);
        }

        [Fact]
        public void DateCreated_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedDateCreated = DateTime.UtcNow.AddDays(-1);

            // Act
            jobInfo.DateCreated = expectedDateCreated;

            // Assert
            Assert.Equal(expectedDateCreated, jobInfo.DateCreated);
        }

        [Fact]
        public void DateCreated_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(default(DateTime), jobInfo.DateCreated);
        }

        [Fact]
        public void DateModified_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedDateModified = DateTime.UtcNow;

            // Act
            jobInfo.DateModified = expectedDateModified;

            // Assert
            Assert.Equal(expectedDateModified, jobInfo.DateModified);
        }

        [Fact]
        public void DateModified_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(default(DateTime), jobInfo.DateModified);
        }

        [Fact]
        public void Category_GetSet()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var expectedCategory = "Database Maintenance";

            // Act
            jobInfo.Category = expectedCategory;

            // Assert
            Assert.Equal(expectedCategory, jobInfo.Category);
        }

        [Fact]
        public void Category_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.Equal(string.Empty, jobInfo.Category);
        }

        [Fact]
        public void Steps_IsInitializedAndEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.NotNull(jobInfo.Steps);
            Assert.Empty(jobInfo.Steps);
        }

        [Fact]
        public void Steps_CanAddItems()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var step = new SqlServerAgentJobStepInfo();

            // Act
            jobInfo.Steps.Add(step);

            // Assert
            var addedStep = Assert.Single(jobInfo.Steps);
            Assert.Same(step, addedStep);
        }

        [Fact]
        public void Schedules_IsInitializedAndEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.NotNull(jobInfo.Schedules);
            Assert.Empty(jobInfo.Schedules);
        }

        [Fact]
        public void Schedules_CanAddItems()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var schedule = new SqlServerAgentJobScheduleInfo();

            // Act
            jobInfo.Schedules.Add(schedule);

            // Assert
            var addedSchedule = Assert.Single(jobInfo.Schedules);
            Assert.Same(schedule, addedSchedule);
        }

        [Fact]
        public void History_IsInitializedAndEmpty()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();

            // Assert
            Assert.NotNull(jobInfo.History);
            Assert.Empty(jobInfo.History);
        }

        [Fact]
        public void History_CanAddItems()
        {
            // Arrange
            var jobInfo = new SqlServerAgentJobInfo();
            var historyItem = new SqlServerAgentJobHistoryInfo();

            // Act
            jobInfo.History.Add(historyItem);

            // Assert
            var addedHistoryItem = Assert.Single(jobInfo.History);
            Assert.Same(historyItem, addedHistoryItem);
        }
    }
}
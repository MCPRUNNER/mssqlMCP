using Xunit;
using mssqlMCP.Models;
using System;

namespace mssqlMCP.Models.Tests
{
    public class SqlServerAgentJobStepInfoTests
    {
        [Fact]
        public void StepId_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedStepId = 1;

            // Act
            stepInfo.StepId = expectedStepId;

            // Assert
            Assert.Equal(expectedStepId, stepInfo.StepId);
        }

        [Fact]
        public void StepId_DefaultValue_IsZero()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(0, stepInfo.StepId);
        }

        [Fact]
        public void StepName_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedStepName = "Execute T-SQL";

            // Act
            stepInfo.StepName = expectedStepName;

            // Assert
            Assert.Equal(expectedStepName, stepInfo.StepName);
        }

        [Fact]
        public void StepName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(string.Empty, stepInfo.StepName);
        }

        [Fact]
        public void Subsystem_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedSubsystem = "TSQL";

            // Act
            stepInfo.Subsystem = expectedSubsystem;

            // Assert
            Assert.Equal(expectedSubsystem, stepInfo.Subsystem);
        }

        [Fact]
        public void Subsystem_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(string.Empty, stepInfo.Subsystem);
        }

        [Fact]
        public void Command_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedCommand = "PRINT 'Hello World'";

            // Act
            stepInfo.Command = expectedCommand;

            // Assert
            Assert.Equal(expectedCommand, stepInfo.Command);
        }

        [Fact]
        public void Command_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(string.Empty, stepInfo.Command);
        }

        [Fact]
        public void DatabaseName_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedDatabaseName = "master";

            // Act
            stepInfo.DatabaseName = expectedDatabaseName;

            // Assert
            Assert.Equal(expectedDatabaseName, stepInfo.DatabaseName);
        }

        [Fact]
        public void DatabaseName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(string.Empty, stepInfo.DatabaseName);
        }

        [Fact]
        public void OnSuccessAction_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedAction = 1; // Go to next step

            // Act
            stepInfo.OnSuccessAction = expectedAction;

            // Assert
            Assert.Equal(expectedAction, stepInfo.OnSuccessAction);
        }

        [Fact]
        public void OnSuccessAction_DefaultValue_IsZero()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(0, stepInfo.OnSuccessAction);
        }

        [Fact]
        public void OnFailAction_GetSet()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();
            var expectedAction = 2; // Quit job reporting failure

            // Act
            stepInfo.OnFailAction = expectedAction;

            // Assert
            Assert.Equal(expectedAction, stepInfo.OnFailAction);
        }

        [Fact]
        public void OnFailAction_DefaultValue_IsZero()
        {
            // Arrange
            var stepInfo = new SqlServerAgentJobStepInfo();

            // Assert
            Assert.Equal(0, stepInfo.OnFailAction);
        }
    }

    public class SqlServerAgentJobScheduleInfoTests
    {
        [Fact]
        public void ScheduleName_GetSet()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();
            var expectedScheduleName = "Daily Schedule";

            // Act
            scheduleInfo.ScheduleName = expectedScheduleName;

            // Assert
            Assert.Equal(expectedScheduleName, scheduleInfo.ScheduleName);
        }

        [Fact]
        public void ScheduleName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();

            // Assert
            Assert.Equal(string.Empty, scheduleInfo.ScheduleName);
        }

        [Fact]
        public void Enabled_GetSet()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();

            // Act
            scheduleInfo.Enabled = true;

            // Assert
            Assert.True(scheduleInfo.Enabled);

            // Act
            scheduleInfo.Enabled = false;
            // Assert
            Assert.False(scheduleInfo.Enabled);
        }

        [Fact]
        public void Enabled_DefaultValue_IsFalse()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();

            // Assert
            Assert.False(scheduleInfo.Enabled);
        }

        [Fact]
        public void FrequencyType_GetSet()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();
            var expectedFreqType = 4; // Daily

            // Act
            scheduleInfo.FrequencyType = expectedFreqType;

            // Assert
            Assert.Equal(expectedFreqType, scheduleInfo.FrequencyType);
        }

        [Fact]
        public void FrequencyType_DefaultValue_IsZero()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();

            // Assert
            Assert.Equal(0, scheduleInfo.FrequencyType);
        }

        [Fact]
        public void ActiveStartTime_GetSet()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();
            var expectedStartTime = 233000; // 23:30:00

            // Act
            scheduleInfo.ActiveStartTime = expectedStartTime;

            // Assert
            Assert.Equal(expectedStartTime, scheduleInfo.ActiveStartTime);
        }

        [Fact]
        public void ActiveStartTime_DefaultValue_IsZero()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();

            // Assert
            Assert.Equal(0, scheduleInfo.ActiveStartTime);
        }

        [Fact]
        public void ActiveEndTime_GetSet()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();
            var expectedEndTime = 235959; // 23:59:59

            // Act
            scheduleInfo.ActiveEndTime = expectedEndTime;

            // Assert
            Assert.Equal(expectedEndTime, scheduleInfo.ActiveEndTime);
        }

        [Fact]
        public void ActiveEndTime_DefaultValue_IsZero()
        {
            // Arrange
            var scheduleInfo = new SqlServerAgentJobScheduleInfo();

            // Assert
            Assert.Equal(0, scheduleInfo.ActiveEndTime);
        }
    }

    public class SqlServerAgentJobHistoryInfoTests
    {
        [Fact]
        public void InstanceId_GetSet()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();
            var expectedInstanceId = 12345;

            // Act
            historyInfo.InstanceId = expectedInstanceId;

            // Assert
            Assert.Equal(expectedInstanceId, historyInfo.InstanceId);
        }

        [Fact]
        public void InstanceId_DefaultValue_IsZero()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();

            // Assert
            Assert.Equal(0, historyInfo.InstanceId);
        }

        [Fact]
        public void RunDate_GetSet()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();
            var expectedRunDate = 20230525; // YYYYMMDD

            // Act
            historyInfo.RunDate = expectedRunDate;

            // Assert
            Assert.Equal(expectedRunDate, historyInfo.RunDate);
        }

        [Fact]
        public void RunDate_DefaultValue_IsZero()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();

            // Assert
            Assert.Equal(0, historyInfo.RunDate);
        }

        [Fact]
        public void RunTime_GetSet()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();
            var expectedRunTime = 103045; // HHMMSS

            // Act
            historyInfo.RunTime = expectedRunTime;

            // Assert
            Assert.Equal(expectedRunTime, historyInfo.RunTime);
        }

        [Fact]
        public void RunTime_DefaultValue_IsZero()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();

            // Assert
            Assert.Equal(0, historyInfo.RunTime);
        }

        [Fact]
        public void RunStatus_GetSet()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();
            var expectedRunStatus = 1; // Succeeded

            // Act
            historyInfo.RunStatus = expectedRunStatus;

            // Assert
            Assert.Equal(expectedRunStatus, historyInfo.RunStatus);
        }

        [Fact]
        public void RunStatus_DefaultValue_IsZero()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();

            // Assert
            Assert.Equal(0, historyInfo.RunStatus);
        }

        [Fact]
        public void RunDuration_GetSet()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();
            var expectedRunDuration = 120; // In seconds or HHMMSS format depending on source

            // Act
            historyInfo.RunDuration = expectedRunDuration;

            // Assert
            Assert.Equal(expectedRunDuration, historyInfo.RunDuration);
        }

        [Fact]
        public void RunDuration_DefaultValue_IsZero()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();

            // Assert
            Assert.Equal(0, historyInfo.RunDuration);
        }

        [Fact]
        public void Message_GetSet()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();
            var expectedMessage = "The job succeeded.";

            // Act
            historyInfo.Message = expectedMessage;

            // Assert
            Assert.Equal(expectedMessage, historyInfo.Message);
        }

        [Fact]
        public void Message_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var historyInfo = new SqlServerAgentJobHistoryInfo();

            // Assert
            Assert.Equal(string.Empty, historyInfo.Message);
        }
    }
}
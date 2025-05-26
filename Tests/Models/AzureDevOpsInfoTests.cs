using Xunit;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;

namespace mssqlMCP.Models.Tests
{
    public class AzureDevOpsInfoTests
    {
        [Fact]
        public void Projects_IsInitializedAndEmpty()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();

            // Assert
            Assert.NotNull(devOpsInfo.Projects);
            Assert.Empty(devOpsInfo.Projects);
        }

        [Fact]
        public void Projects_CanAddItems()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();
            var project = new AzureDevOpsProject();

            // Act
            devOpsInfo.Projects.Add(project);

            // Assert
            var addedProject = Assert.Single(devOpsInfo.Projects);
            Assert.Same(project, addedProject);
        }

        [Fact]
        public void Repositories_IsInitializedAndEmpty()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();

            // Assert
            Assert.NotNull(devOpsInfo.Repositories);
            Assert.Empty(devOpsInfo.Repositories);
        }

        [Fact]
        public void Repositories_CanAddItems()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();
            var repo = new AzureDevOpsRepository();

            // Act
            devOpsInfo.Repositories.Add(repo);

            // Assert
            var addedRepo = Assert.Single(devOpsInfo.Repositories);
            Assert.Same(repo, addedRepo);
        }

        [Fact]
        public void BuildDefinitionCount_GetSet()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();
            var expectedCount = 10;

            // Act
            devOpsInfo.BuildDefinitionCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, devOpsInfo.BuildDefinitionCount);
        }

        [Fact]
        public void BuildDefinitionCount_DefaultValue_IsZero()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();

            // Assert
            Assert.Equal(0, devOpsInfo.BuildDefinitionCount);
        }

        [Fact]
        public void ReleaseDefinitionCount_GetSet()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();
            var expectedCount = 5;

            // Act
            devOpsInfo.ReleaseDefinitionCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, devOpsInfo.ReleaseDefinitionCount);
        }

        [Fact]
        public void ReleaseDefinitionCount_DefaultValue_IsZero()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();

            // Assert
            Assert.Equal(0, devOpsInfo.ReleaseDefinitionCount);
        }

        [Fact]
        public void WorkItemCount_GetSet()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();
            var expectedCount = 100;

            // Act
            devOpsInfo.WorkItemCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, devOpsInfo.WorkItemCount);
        }

        [Fact]
        public void WorkItemCount_DefaultValue_IsZero()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();

            // Assert
            Assert.Equal(0, devOpsInfo.WorkItemCount);
        }

        [Fact]
        public void BuildDefinitions_IsInitializedAndEmpty()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();

            // Assert
            Assert.NotNull(devOpsInfo.BuildDefinitions);
            Assert.Empty(devOpsInfo.BuildDefinitions);
        }

        [Fact]
        public void BuildDefinitions_CanAddItems()
        {
            // Arrange
            var devOpsInfo = new AzureDevOpsInfo();
            var buildDef = new AzureDevOpsBuildDefinition();

            // Act
            devOpsInfo.BuildDefinitions.Add(buildDef);

            // Assert
            var addedBuildDef = Assert.Single(devOpsInfo.BuildDefinitions);
            Assert.Same(buildDef, addedBuildDef);
        }
    }

    public class AzureDevOpsProjectTests
    {
        [Fact]
        public void ProjectId_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedId = Guid.NewGuid();

            // Act
            project.ProjectId = expectedId;

            // Assert
            Assert.Equal(expectedId, project.ProjectId);
        }

        [Fact]
        public void ProjectId_DefaultValue_IsGuidEmpty()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(Guid.Empty, project.ProjectId);
        }

        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedName = "My Awesome Project";

            // Act
            project.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, project.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(string.Empty, project.Name);
        }

        [Fact]
        public void Description_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedDescription = "This is a test project.";

            // Act
            project.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, project.Description);
        }

        [Fact]
        public void Description_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(string.Empty, project.Description);
        }

        [Fact]
        public void State_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedState = "WellFormed";

            // Act
            project.State = expectedState;

            // Assert
            Assert.Equal(expectedState, project.State);
        }

        [Fact]
        public void State_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(string.Empty, project.State);
        }

        [Fact]
        public void Revision_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedRevision = 123;

            // Act
            project.Revision = expectedRevision;

            // Assert
            Assert.Equal(expectedRevision, project.Revision);
        }

        [Fact]
        public void Revision_DefaultValue_IsZero()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(0, project.Revision);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedDate = DateTime.UtcNow.AddDays(-5);

            // Act
            project.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, project.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(default(DateTime), project.CreatedDate);
        }

        [Fact]
        public void LastUpdatedDate_GetSet()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var expectedDate = DateTime.UtcNow.AddDays(-1);

            // Act
            project.LastUpdatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, project.LastUpdatedDate);
        }

        [Fact]
        public void LastUpdatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.Equal(default(DateTime), project.LastUpdatedDate);
        }

        [Fact]
        public void BuildDefinitions_IsInitializedAndEmpty()
        {
            // Arrange
            var project = new AzureDevOpsProject();

            // Assert
            Assert.NotNull(project.BuildDefinitions);
            Assert.Empty(project.BuildDefinitions);
        }

        [Fact]
        public void BuildDefinitions_CanAddItems()
        {
            // Arrange
            var project = new AzureDevOpsProject();
            var buildDef = new AzureDevOpsBuildDefinition();

            // Act
            project.BuildDefinitions.Add(buildDef);

            // Assert
            var addedBuildDef = Assert.Single(project.BuildDefinitions);
            Assert.Same(buildDef, addedBuildDef);
        }
    }

    public class AzureDevOpsRepositoryTests
    {
        [Fact]
        public void RepositoryId_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedId = Guid.NewGuid();

            // Act
            repo.RepositoryId = expectedId;

            // Assert
            Assert.Equal(expectedId, repo.RepositoryId);
        }

        [Fact]
        public void RepositoryId_DefaultValue_IsGuidEmpty()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(Guid.Empty, repo.RepositoryId);
        }

        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedName = "MainRepository";

            // Act
            repo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, repo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(string.Empty, repo.Name);
        }

        [Fact]
        public void ProjectId_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedId = Guid.NewGuid();

            // Act
            repo.ProjectId = expectedId;

            // Assert
            Assert.Equal(expectedId, repo.ProjectId);
        }

        [Fact]
        public void ProjectId_DefaultValue_IsGuidEmpty()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(Guid.Empty, repo.ProjectId);
        }

        [Fact]
        public void ProjectName_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedName = "AssociatedProject";

            // Act
            repo.ProjectName = expectedName;

            // Assert
            Assert.Equal(expectedName, repo.ProjectName);
        }

        [Fact]
        public void ProjectName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(string.Empty, repo.ProjectName);
        }

        [Fact]
        public void Size_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            long expectedSize = 1024 * 1024 * 50; // 50MB

            // Act
            repo.Size = expectedSize;

            // Assert
            Assert.Equal(expectedSize, repo.Size);
        }

        [Fact]
        public void Size_DefaultValue_IsZero()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(0L, repo.Size);
        }

        [Fact]
        public void BranchCount_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedCount = 5;

            // Act
            repo.BranchCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, repo.BranchCount);
        }

        [Fact]
        public void BranchCount_DefaultValue_IsZero()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(0, repo.BranchCount);
        }

        [Fact]
        public void CommitCount_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedCount = 1500;

            // Act
            repo.CommitCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, repo.CommitCount);
        }

        [Fact]
        public void CommitCount_DefaultValue_IsZero()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(0, repo.CommitCount);
        }

        [Fact]
        public void DefaultBranch_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedBranch = "refs/heads/main";

            // Act
            repo.DefaultBranch = expectedBranch;

            // Assert
            Assert.Equal(expectedBranch, repo.DefaultBranch);
        }

        [Fact]
        public void DefaultBranch_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(string.Empty, repo.DefaultBranch);
        }

        [Fact]
        public void RepositoryUrl_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();
            var expectedUrl = "https://dev.azure.com/org/project/_git/repo";

            // Act
            repo.RepositoryUrl = expectedUrl;

            // Assert
            Assert.Equal(expectedUrl, repo.RepositoryUrl);
        }

        [Fact]
        public void RepositoryUrl_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.Equal(string.Empty, repo.RepositoryUrl);
        }

        [Fact]
        public void IsDisabled_GetSet()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Act
            repo.IsDisabled = true;

            // Assert
            Assert.True(repo.IsDisabled);

            // Act
            repo.IsDisabled = false;

            // Assert
            Assert.False(repo.IsDisabled);
        }

        [Fact]
        public void IsDisabled_DefaultValue_IsFalse()
        {
            // Arrange
            var repo = new AzureDevOpsRepository();

            // Assert
            Assert.False(repo.IsDisabled);
        }
    }

    public class AzureDevOpsBuildDefinitionTests
    {
        [Fact]
        public void Id_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedId = 42;

            // Act
            buildDef.Id = expectedId;

            // Assert
            Assert.Equal(expectedId, buildDef.Id);
        }

        [Fact]
        public void Id_DefaultValue_IsZero()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(0, buildDef.Id);
        }

        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedName = "CI Build";

            // Act
            buildDef.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, buildDef.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(string.Empty, buildDef.Name);
        }

        [Fact]
        public void ProjectId_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedId = Guid.NewGuid();

            // Act
            buildDef.ProjectId = expectedId;

            // Assert
            Assert.Equal(expectedId, buildDef.ProjectId);
        }

        [Fact]
        public void ProjectId_DefaultValue_IsGuidEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(Guid.Empty, buildDef.ProjectId);
        }

        [Fact]
        public void ProjectName_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedName = "ProjectX";

            // Act
            buildDef.ProjectName = expectedName;

            // Assert
            Assert.Equal(expectedName, buildDef.ProjectName);
        }

        [Fact]
        public void ProjectName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(string.Empty, buildDef.ProjectName);
        }

        [Fact]
        public void RepositoryName_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedName = "RepoY";

            // Act
            buildDef.RepositoryName = expectedName;

            // Assert
            Assert.Equal(expectedName, buildDef.RepositoryName);
        }

        [Fact]
        public void RepositoryName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(string.Empty, buildDef.RepositoryName);
        }

        [Fact]
        public void Path_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedPath = "\\Builds\\Main";

            // Act
            buildDef.Path = expectedPath;

            // Assert
            Assert.Equal(expectedPath, buildDef.Path);
        }

        [Fact]
        public void Path_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(string.Empty, buildDef.Path);
        }

        [Fact]
        public void Type_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedType = "Build";

            // Act
            buildDef.Type = expectedType;

            // Assert
            Assert.Equal(expectedType, buildDef.Type);
        }

        [Fact]
        public void Type_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(string.Empty, buildDef.Type);
        }

        [Fact]
        public void QueueStatus_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedStatus = "Enabled";

            // Act
            buildDef.QueueStatus = expectedStatus;

            // Assert
            Assert.Equal(expectedStatus, buildDef.QueueStatus);
        }

        [Fact]
        public void QueueStatus_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(string.Empty, buildDef.QueueStatus);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedDate = DateTime.UtcNow.AddMonths(-1);

            // Act
            buildDef.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, buildDef.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(default(DateTime), buildDef.CreatedDate);
        }

        [Fact]
        public void LastBuildDate_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            DateTime? expectedDate = DateTime.UtcNow.AddHours(-2);

            // Act
            buildDef.LastBuildDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, buildDef.LastBuildDate);

            // Act
            buildDef.LastBuildDate = null;

            // Assert
            Assert.Null(buildDef.LastBuildDate);
        }

        [Fact]
        public void LastBuildDate_DefaultValue_IsNull()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Null(buildDef.LastBuildDate);
        }

        [Fact]
        public void TotalBuilds_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedCount = 250;

            // Act
            buildDef.TotalBuilds = expectedCount;

            // Assert
            Assert.Equal(expectedCount, buildDef.TotalBuilds);
        }

        [Fact]
        public void TotalBuilds_DefaultValue_IsZero()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(0, buildDef.TotalBuilds);
        }

        [Fact]
        public void SuccessfulBuilds_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedCount = 240;

            // Act
            buildDef.SuccessfulBuilds = expectedCount;

            // Assert
            Assert.Equal(expectedCount, buildDef.SuccessfulBuilds);
        }

        [Fact]
        public void SuccessfulBuilds_DefaultValue_IsZero()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(0, buildDef.SuccessfulBuilds);
        }

        [Fact]
        public void FailedBuilds_GetSet()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();
            var expectedCount = 10;

            // Act
            buildDef.FailedBuilds = expectedCount;

            // Assert
            Assert.Equal(expectedCount, buildDef.FailedBuilds);
        }

        [Fact]
        public void FailedBuilds_DefaultValue_IsZero()
        {
            // Arrange
            var buildDef = new AzureDevOpsBuildDefinition();

            // Assert
            Assert.Equal(0, buildDef.FailedBuilds);
        }
    }
}
using Xunit;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;

namespace mssqlMCP.Models.Tests
{
    public class SsisFolderInfoTests
    {
        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var folderInfo = new SsisFolderInfo();
            var expectedName = "Test Folder";

            // Act
            folderInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, folderInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var folderInfo = new SsisFolderInfo();

            // Assert
            Assert.Equal(string.Empty, folderInfo.Name);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var folderInfo = new SsisFolderInfo();
            var expectedDate = DateTime.UtcNow;

            // Act
            folderInfo.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, folderInfo.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var folderInfo = new SsisFolderInfo();

            // Assert
            Assert.Equal(default(DateTime), folderInfo.CreatedDate);
        }

        [Fact]
        public void Projects_IsInitializedAndEmpty()
        {
            // Arrange
            var folderInfo = new SsisFolderInfo();

            // Assert
            Assert.NotNull(folderInfo.Projects);
            Assert.Empty(folderInfo.Projects);
        }

        [Fact]
        public void Projects_CanAddItems()
        {
            // Arrange
            var folderInfo = new SsisFolderInfo();
            var project = new SsisProjectInfo();

            // Act
            folderInfo.Projects.Add(project);

            // Assert
            var addedProject = Assert.Single(folderInfo.Projects);
            Assert.Same(project, addedProject);
        }
    }

    public class SsisProjectInfoTests
    {
        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var expectedName = "Test Project";

            // Act
            projectInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, projectInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.Equal(string.Empty, projectInfo.Name);
        }

        [Fact]
        public void DeploymentModel_GetSet()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var expectedModel = "Project";

            // Act
            projectInfo.DeploymentModel = expectedModel;

            // Assert
            Assert.Equal(expectedModel, projectInfo.DeploymentModel);
        }

        [Fact]
        public void DeploymentModel_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.Equal(string.Empty, projectInfo.DeploymentModel);
        }

        [Fact]
        public void Description_GetSet()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var expectedDescription = "Test Project Description";

            // Act
            projectInfo.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, projectInfo.Description);
        }

        [Fact]
        public void Description_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.Equal(string.Empty, projectInfo.Description);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var expectedDate = DateTime.UtcNow.AddDays(-1);

            // Act
            projectInfo.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, projectInfo.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.Equal(default(DateTime), projectInfo.CreatedDate);
        }

        [Fact]
        public void LastDeployedDate_GetSet()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var expectedDate = DateTime.UtcNow;

            // Act
            projectInfo.LastDeployedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, projectInfo.LastDeployedDate);
        }

        [Fact]
        public void LastDeployedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.Equal(default(DateTime), projectInfo.LastDeployedDate);
        }

        [Fact]
        public void Packages_IsInitializedAndEmpty()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.NotNull(projectInfo.Packages);
            Assert.Empty(projectInfo.Packages);
        }

        [Fact]
        public void Packages_CanAddItems()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var package = new SsisPackageInfo();

            // Act
            projectInfo.Packages.Add(package);

            // Assert
            var addedPackage = Assert.Single(projectInfo.Packages);
            Assert.Same(package, addedPackage);
        }

        [Fact]
        public void Environments_IsInitializedAndEmpty()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();

            // Assert
            Assert.NotNull(projectInfo.Environments);
            Assert.Empty(projectInfo.Environments);
        }

        [Fact]
        public void Environments_CanAddItems()
        {
            // Arrange
            var projectInfo = new SsisProjectInfo();
            var environment = new SsisEnvironmentInfo();

            // Act
            projectInfo.Environments.Add(environment);

            // Assert
            var addedEnvironment = Assert.Single(projectInfo.Environments);
            Assert.Same(environment, addedEnvironment);
        }
    }

    public class SsisPackageInfoTests
    {
        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();
            var expectedName = "Test Package";

            // Act
            packageInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, packageInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();

            // Assert
            Assert.Equal(string.Empty, packageInfo.Name);
        }

        [Fact]
        public void Description_GetSet()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();
            var expectedDescription = "Test Package Description";

            // Act
            packageInfo.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, packageInfo.Description);
        }

        [Fact]
        public void Description_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();

            // Assert
            Assert.Equal(string.Empty, packageInfo.Description);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();
            var expectedDate = DateTime.UtcNow.AddHours(-2);

            // Act
            packageInfo.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, packageInfo.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();

            // Assert
            Assert.Equal(default(DateTime), packageInfo.CreatedDate);
        }

        [Fact]
        public void LastModifiedDate_GetSet()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();
            var expectedDate = DateTime.UtcNow.AddHours(-1);

            // Act
            packageInfo.LastModifiedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, packageInfo.LastModifiedDate);
        }

        [Fact]
        public void LastModifiedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();

            // Assert
            Assert.Equal(default(DateTime), packageInfo.LastModifiedDate);
        }

        [Fact]
        public void Parameters_IsInitializedAndEmpty()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();

            // Assert
            Assert.NotNull(packageInfo.Parameters);
            Assert.Empty(packageInfo.Parameters);
        }

        [Fact]
        public void Parameters_CanAddItems()
        {
            // Arrange
            var packageInfo = new SsisPackageInfo();
            var parameter = new SsisPackageParameterInfo();

            // Act
            packageInfo.Parameters.Add(parameter);

            // Assert
            var addedParameter = Assert.Single(packageInfo.Parameters);
            Assert.Same(parameter, addedParameter);
        }
    }

    public class SsisPackageParameterInfoTests
    {
        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();
            var expectedName = "ConnectionString";

            // Act
            paramInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, paramInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Assert
            Assert.Equal(string.Empty, paramInfo.Name);
        }

        [Fact]
        public void DataType_GetSet()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();
            var expectedDataType = "String";

            // Act
            paramInfo.DataType = expectedDataType;

            // Assert
            Assert.Equal(expectedDataType, paramInfo.DataType);
        }

        [Fact]
        public void DataType_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Assert
            Assert.Equal(string.Empty, paramInfo.DataType);
        }

        [Fact]
        public void DefaultValue_GetSet()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();
            var expectedValue = "Server=localhost;Database=TestDB;";

            // Act
            paramInfo.DefaultValue = expectedValue;

            // Assert
            Assert.Equal(expectedValue, paramInfo.DefaultValue);
        }

        [Fact]
        public void DefaultValue_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Assert
            Assert.Equal(string.Empty, paramInfo.DefaultValue);
        }

        [Fact]
        public void Required_GetSet()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Act
            paramInfo.Required = true;

            // Assert
            Assert.True(paramInfo.Required);

            // Act
            paramInfo.Required = false;

            // Assert
            Assert.False(paramInfo.Required);
        }

        [Fact]
        public void Required_DefaultValue_IsFalse()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Assert
            Assert.False(paramInfo.Required);
        }

        [Fact]
        public void Sensitive_GetSet()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Act
            paramInfo.Sensitive = true;

            // Assert
            Assert.True(paramInfo.Sensitive);

            // Act
            paramInfo.Sensitive = false;

            // Assert
            Assert.False(paramInfo.Sensitive);
        }

        [Fact]
        public void Sensitive_DefaultValue_IsFalse()
        {
            // Arrange
            var paramInfo = new SsisPackageParameterInfo();

            // Assert
            Assert.False(paramInfo.Sensitive);
        }
    }

    public class SsisEnvironmentInfoTests
    {
        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();
            var expectedName = "Development";

            // Act
            envInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, envInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();

            // Assert
            Assert.Equal(string.Empty, envInfo.Name);
        }

        [Fact]
        public void Description_GetSet()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();
            var expectedDescription = "Development Environment Settings";

            // Act
            envInfo.Description = expectedDescription;

            // Assert
            Assert.Equal(expectedDescription, envInfo.Description);
        }

        [Fact]
        public void Description_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();

            // Assert
            Assert.Equal(string.Empty, envInfo.Description);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();
            var expectedDate = DateTime.UtcNow.AddDays(-5);

            // Act
            envInfo.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, envInfo.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();

            // Assert
            Assert.Equal(default(DateTime), envInfo.CreatedDate);
        }

        [Fact]
        public void Variables_IsInitializedAndEmpty()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();

            // Assert
            Assert.NotNull(envInfo.Variables);
            Assert.Empty(envInfo.Variables);
        }

        [Fact]
        public void Variables_CanAddItems()
        {
            // Arrange
            var envInfo = new SsisEnvironmentInfo();
            var variable = new SsisEnvironmentVariableInfo();

            // Act
            envInfo.Variables.Add(variable);

            // Assert
            var addedVariable = Assert.Single(envInfo.Variables);
            Assert.Same(variable, addedVariable);
        }
    }

    public class SsisEnvironmentVariableInfoTests
    {
        [Fact]
        public void Name_GetSet()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();
            var expectedName = "ServerName";

            // Act
            varInfo.Name = expectedName;

            // Assert
            Assert.Equal(expectedName, varInfo.Name);
        }

        [Fact]
        public void Name_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();

            // Assert
            Assert.Equal(string.Empty, varInfo.Name);
        }

        [Fact]
        public void DataType_GetSet()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();
            var expectedDataType = "String";

            // Act
            varInfo.DataType = expectedDataType;

            // Assert
            Assert.Equal(expectedDataType, varInfo.DataType);
        }

        [Fact]
        public void DataType_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();

            // Assert
            Assert.Equal(string.Empty, varInfo.DataType);
        }

        [Fact]
        public void Value_GetSet()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();
            var expectedValue = "localhost";

            // Act
            varInfo.Value = expectedValue;

            // Assert
            Assert.Equal(expectedValue, varInfo.Value);
        }

        [Fact]
        public void Value_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();

            // Assert
            Assert.Equal(string.Empty, varInfo.Value);
        }

        [Fact]
        public void Sensitive_GetSet()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();

            // Act
            varInfo.Sensitive = true;

            // Assert
            Assert.True(varInfo.Sensitive);

            // Act
            varInfo.Sensitive = false;

            // Assert
            Assert.False(varInfo.Sensitive);
        }

        [Fact]
        public void Sensitive_DefaultValue_IsFalse()
        {
            // Arrange
            var varInfo = new SsisEnvironmentVariableInfo();

            // Assert
            Assert.False(varInfo.Sensitive);
        }
    }

    public class SsisCatalogInfoTests
    {
        [Fact]
        public void ServerName_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedName = "MySqlServer";

            // Act
            catalogInfo.ServerName = expectedName;

            // Assert
            Assert.Equal(expectedName, catalogInfo.ServerName);
        }

        [Fact]
        public void ServerName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(string.Empty, catalogInfo.ServerName);
        }

        [Fact]
        public void CatalogName_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedName = "SSISDB";

            // Act
            catalogInfo.CatalogName = expectedName;

            // Assert
            Assert.Equal(expectedName, catalogInfo.CatalogName);
        }

        [Fact]
        public void CatalogName_DefaultValue_IsStringEmpty()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(string.Empty, catalogInfo.CatalogName);
        }

        [Fact]
        public void CreatedDate_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedDate = DateTime.UtcNow.AddYears(-1);

            // Act
            catalogInfo.CreatedDate = expectedDate;

            // Assert
            Assert.Equal(expectedDate, catalogInfo.CreatedDate);
        }

        [Fact]
        public void CreatedDate_DefaultValue_IsDefaultDateTime()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(default(DateTime), catalogInfo.CreatedDate);
        }

        [Fact]
        public void Folders_IsInitializedAndEmpty()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.NotNull(catalogInfo.Folders);
            Assert.Empty(catalogInfo.Folders);
        }

        [Fact]
        public void Folders_CanAddItems()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var folder = new SsisFolderInfo();

            // Act
            catalogInfo.Folders.Add(folder);

            // Assert
            var addedFolder = Assert.Single(catalogInfo.Folders);
            Assert.Same(folder, addedFolder);
        }

        [Fact]
        public void ProjectCount_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedCount = 10;

            // Act
            catalogInfo.ProjectCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, catalogInfo.ProjectCount);
        }

        [Fact]
        public void ProjectCount_DefaultValue_IsZero()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(0, catalogInfo.ProjectCount);
        }

        [Fact]
        public void PackageCount_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedCount = 50;

            // Act
            catalogInfo.PackageCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, catalogInfo.PackageCount);
        }

        [Fact]
        public void PackageCount_DefaultValue_IsZero()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(0, catalogInfo.PackageCount);
        }

        [Fact]
        public void ProjectDeploymentCount_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedCount = 5;

            // Act
            catalogInfo.ProjectDeploymentCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, catalogInfo.ProjectDeploymentCount);
        }

        [Fact]
        public void ProjectDeploymentCount_DefaultValue_IsZero()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(0, catalogInfo.ProjectDeploymentCount);
        }

        [Fact]
        public void PackageDeploymentCount_GetSet()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();
            var expectedCount = 20;

            // Act
            catalogInfo.PackageDeploymentCount = expectedCount;

            // Assert
            Assert.Equal(expectedCount, catalogInfo.PackageDeploymentCount);
        }

        [Fact]
        public void PackageDeploymentCount_DefaultValue_IsZero()
        {
            // Arrange
            var catalogInfo = new SsisCatalogInfo();

            // Assert
            Assert.Equal(0, catalogInfo.PackageDeploymentCount);
        }
    }
}
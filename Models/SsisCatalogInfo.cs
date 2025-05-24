namespace mssqlMCP.Models;

public class SsisFolderInfo
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate
    {
        get; set;
    }
    public List<SsisProjectInfo> Projects { get; set; } = new();
}

public class SsisProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public string DeploymentModel { get; set; } = string.Empty; // "Project" or "Package"
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate
    {
        get; set;
    }
    public DateTime LastDeployedDate
    {
        get; set;
    }
    public List<SsisPackageInfo> Packages { get; set; } = new();
    public List<SsisEnvironmentInfo> Environments { get; set; } = new();
}

public class SsisPackageInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate
    {
        get; set;
    }
    public DateTime LastModifiedDate
    {
        get; set;
    }
    public List<SsisPackageParameterInfo> Parameters { get; set; } = new();
}

public class SsisPackageParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public bool Required
    {
        get; set;
    }
    public bool Sensitive
    {
        get; set;
    }
}

public class SsisEnvironmentInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate
    {
        get; set;
    }
    public List<SsisEnvironmentVariableInfo> Variables { get; set; } = new();
}

public class SsisEnvironmentVariableInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Sensitive
    {
        get; set;
    }
}

public class SsisCatalogInfo
{
    public string ServerName { get; set; } = string.Empty;
    public string CatalogName { get; set; } = string.Empty;
    public DateTime CreatedDate
    {
        get; set;
    }
    public List<SsisFolderInfo> Folders { get; set; } = new();
    public int ProjectCount
    {
        get; set;
    }
    public int PackageCount
    {
        get; set;
    }
    public int ProjectDeploymentCount
    {
        get; set;
    }
    public int PackageDeploymentCount
    {
        get; set;
    }
}

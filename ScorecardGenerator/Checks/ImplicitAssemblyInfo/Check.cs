using System.Xml.Linq;
using System.Xml.XPath;
using Serilog;

namespace ScorecardGenerator.Checks.ImplicitAssemblyInfo;

public class Check : BaseCheck
{
    private readonly IList<string> _requiredProperties = new List<string>
    {
        "Company",
        "Copyright",
        "Description",
        "FileVersion",
        "InformalVersion",
        "Product",
        "UserSecretsId"
    };
    
    public Check(ILogger logger) : base(logger)
    {
    }

    protected override IList<Deduction> Run(string workingDirectory, string relativePathToServiceRoot)
    {
        var absolutePathToServiceRoot = Path.Join(workingDirectory, relativePathToServiceRoot);
        var csprojFiles = Directory.GetFiles(absolutePathToServiceRoot, "*.csproj", SearchOption.TopDirectoryOnly);
        if (!csprojFiles.Any())
        {
            return new List<Deduction> {Deduction.Create(Logger, 100, "No csproj file found at {Location}", absolutePathToServiceRoot)};
        }
        var csproj = XDocument.Load(csprojFiles.First());

        var deductions = _requiredProperties
            .ToDictionary(propertyName => propertyName, propertyName => csproj.XPathSelectElement($"/Project/PropertyGroup/{propertyName}"))
            .Where(valueByPropertyName => valueByPropertyName.Value == null)
            .Select(valueByPropertyName => Deduction.Create(Logger, 20, "No <{ElementName}> element found in {CsProj}", valueByPropertyName.Key, csprojFiles.First()))
            .ToList();

        var generateAssemblyInfo = csproj.XPathSelectElement("/Project/PropertyGroup/GenerateAssemblyInfo")?.Value;
        if (string.IsNullOrEmpty(generateAssemblyInfo))
        {
            deductions.Add(Deduction.Create(Logger, 100, "No <GenerateAssemblyInfo> element found in {CsProj}", csprojFiles.First()));
        }
        else
        {
            const string expectedValue = "true";
            if (generateAssemblyInfo.ToLower() != expectedValue)
            {
                deductions.Add(Deduction.Create(Logger, 100, "Expected: <GenerateAssemblyInfo> should contain '{Expected}'. Actual: '{Actual}'", expectedValue, generateAssemblyInfo));
            }
        }

        return deductions;
    }
}
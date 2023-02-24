using System.Collections.Immutable;
using ScorecardGenerator.Calculation;
using ScorecardGenerator.Checks;
using Serilog;

namespace ScorecardGenerator.Test.Visualizer.AzureWikiTableVisualizer;

public class WithGitRepo : TestWithNeighboringDirectoryFixture
{
    [Test]
    public void DeterministicallyRendersServiceInfo()
    {
        var logger = new LoggerConfiguration().CreateLogger();
        
        var source = Path.Join(Directory.GetCurrentDirectory(), "Visualizer", "AzureWikiTableVisualizer", "WithGitRepo");

        var actualOutputPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(actualOutputPath);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relativePath = file.Replace(source, "");
            var destination = Path.Join(actualOutputPath, relativePath);
            destination = destination.Replace("_git", ".git");
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination);
        }

        var expectedOutputPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(expectedOutputPath);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var relativePath = file.Replace(source, "");
            var destination = Path.Join(expectedOutputPath, relativePath);
            destination = destination.Replace("_git", ".git");
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination);
        }
        
        var visualizer = new ScorecardGenerator.Visualizer.AzureWikiTableVisualizer(logger, actualOutputPath);
        var checks = new Dictionary<string, IList<CheckInfo>>
        {
            { "Gold", new List<CheckInfo> { new("Check", "PageContent"), new("DisqualifiedCheck", "Disqualified PageContent") } },
            { "Silver", new List<CheckInfo>() { new("Check2", "PageContent"), new("DisqualifiedCheck2", "Disqualified PageContent") } },
            { "Bronze", new List<CheckInfo>() { new("Check3", "PageContent"), new("DisqualifiedCheck3", "Disqualified PageContent") } }
        };

        var serviceInfo = new Dictionary<string, RunInfo.ServiceScorecard>
        {
            {"service", new RunInfo.ServiceScorecard(new Dictionary<string, IList<BaseCheck.Deduction>>
            {
                {"Check", new List<BaseCheck.Deduction> {BaseCheck.Deduction.Create( logger, 10, "justification: {Value}", "value")}},
                {"DisqualifiedCheck", new List<BaseCheck.Deduction> {BaseCheck.Deduction.Create( logger, 10, "justification: {Value}", "value"), BaseCheck.Deduction.CreateDisqualification( logger, "disqualify: {Value}", "disqualification")}},
                {"Check2", new List<BaseCheck.Deduction> {BaseCheck.Deduction.Create( logger, 20, "justification: {Value}", "value")}},
                {"DisqualifiedCheck2", new List<BaseCheck.Deduction> {BaseCheck.Deduction.Create( logger, 20, "justification: {Value}", "value"), BaseCheck.Deduction.CreateDisqualification( logger, "disqualify: {Value}", "disqualification")}}, 
                {"Check3", new List<BaseCheck.Deduction> {BaseCheck.Deduction.Create( logger, 30, "justification: {Value}", "value")}},
                {"DisqualifiedCheck3", new List<BaseCheck.Deduction> {BaseCheck.Deduction.Create( logger, 30, "justification: {Value}", "value"), BaseCheck.Deduction.CreateDisqualification( logger, "disqualify: {Value}", "disqualification")}} 
            }, 10)}
        }.ToImmutableSortedDictionary();
        
        visualizer.Visualize(new RunInfo(checks, serviceInfo));
        CompareFilesInsideDirectoriesWithNUnitAsserts(actualOutputPath, Path.Join(WorkingDirectory, RelativePathToServiceRoot));
    }

    private static void CompareFilesInsideDirectoriesWithNUnitAsserts(string absolutePathToDirectoryActual, string absolutePathToDirectoryExpected)
    {
        var actualDirectoryInfo = new DirectoryInfo(absolutePathToDirectoryActual);
        var expectedDirectoryInfo = new DirectoryInfo(absolutePathToDirectoryExpected);

        // filter .git and all files in .git
        var actualFiles = actualDirectoryInfo.GetFiles("*", SearchOption.AllDirectories).Where(file => !file.FullName.Contains(".git") && file.Name != ".DS_Store").ToArray();
        var expectedFiles = expectedDirectoryInfo.GetFiles("*", SearchOption.AllDirectories);

        Assert.That(actualFiles.Length, Is.EqualTo(expectedFiles.Length));

        var expectedFilesDictionary = expectedFiles.ToDictionary(file => file.Name);

        foreach (var actualFile in actualFiles)
        {
            Assert.That(actualFile.Name, Is.EqualTo(expectedFilesDictionary[actualFile.Name].Name));
            Assert.That(File.ReadAllText(actualFile.FullName), Is.EqualTo(File.ReadAllText(expectedFilesDictionary[actualFile.Name].FullName).Replace("YYYY-MM-DD", DateTime.Now.ToString("yyyy-MM-dd"))));
        }
    }
}
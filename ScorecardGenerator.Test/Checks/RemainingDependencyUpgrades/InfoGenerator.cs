﻿using ScorecardGenerator.Checks.RemainingDependencyUpgrades;

namespace ScorecardGenerator.Test.Checks.RemainingDependencyUpgrades;

public class InfoGeneratorTests
{
    [Test]
    [TestCase("git@ssh.dev.azure.com:v3/vimaster/ScorecardGenerator/TestService1 (fetch)")]
    [TestCase("vimaster@vs-ssh.visualstudio.com:v3/vimaster/ScorecardGenerator/TestService1 (fetch)")]
    [TestCase("https://dev.azure.com/vimaster/ScorecardGenerator/_git/TestService1 (fetch)")]
    [TestCase("https://vimaster.visualstudio.com/ScorecardGenerator/_git/TestService1 (fetch)")]
    public void GeneratesCorrectIInfo(string gitRepo)
    {
        const string EXPECTED = "AzureInfo: vimaster/ScorecardGenerator/TestService1";
        var actual = InfoGenerator.FromURL(gitRepo);
        Assert.That(actual.ToString(), Is.EqualTo(EXPECTED));
    }
}
﻿using Serilog;

namespace ScorecardGenerator.Checks.RemainingDependencyUpgrades.RepositoryInfo;

public interface IInfo
{
    public const int DEDUCTION_PER_ACTIVE_PULL_REQUEST = 20;
    public static IInfo? FromURL(string url)
    {
        return null;
    }

    IList<BaseCheck.Deduction> GetDeductions(ILogger logger, Func<string, HttpResponseMessage> getHTTPRequest, string absolutePathToProjectFile);
}
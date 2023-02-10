using Newtonsoft.Json;
using Serilog;

namespace ScorecardGenerator.Visualizer;

internal class AzureWikiTableVisualizer
{
    private readonly ILogger _logger;

    public AzureWikiTableVisualizer(ILogger logger)
    {
        _logger = logger.ForContext<AzureWikiTableVisualizer>();
    }

    private static string ColorizeNumber(int arg)
    {
        var color = arg switch
        {
            >= 90 => "green",
            >= 80 => "yellow",
            >= 70 => "orange",
            _ => "red"
        };
        return $"<span style=\"color:{color}\">{arg}</span>";
    }
    
    public string ToMarkdown(Calculation.RunInfo runInfo)
    {
        var lastUpdatedAt = DateTime.Now;
        var autoGenerationInfo = "<!-- !!! THIS FILE IS AUTOGENERATED - DO NOT EDIT IT MANUALLY !!! -->";
        var usageGuide = "Hover over entries to show details like full service paths and score justifications.";
        var generationInfo = $"Scorecard generated at: {lastUpdatedAt:yyyy-MM-dd HH:mm:ss}";
        
        const string headerElement = "th";
        const string columnElement = "td";
        int alternateColorIndex = 1;
        string ToBackgroundColor()
        {
            ++alternateColorIndex;

            if (alternateColorIndex % 2 == 0)
                return "background-color: rgba(0, 0, 0, 0.5);";
            else
                return "";
        }

        string StyleForElement(int alternateColorIndex, string element)
        {
            string style = "";
            if (alternateColorIndex <= 3)
            {
                style += "background-color: rgba(var(--palette-neutral-2),1);";
            }
            if (element == "th")
            {
                style += "position: sticky; top: -2px;";
            }
            if (alternateColorIndex == 2)
            {
                style += "top: 2.6em;";
            }

            return $"style=\"{style}\"";
        }
        string ToElement(string element, IEnumerable<TableContent> columns)
        {
            return $"<tr style=\"{ToBackgroundColor()}\">{string.Join("", columns.Select(entry => $"<{element} {StyleForElement(alternateColorIndex, element)} colspan=\"{entry.Colspan}\">{entry.Content}</{element}>"))}</tr>";
        }

        var runInfoJSON = JsonConvert.SerializeObject(runInfo);

        var groupData = ToElement(headerElement, runInfo.Checks.Where(group => group.Value.Any()).Select(group=>new TableContent(group.Key, group.Value.Count)).Prepend("   ").Append("   "));
        var headers = ToElement(headerElement, runInfo.Checks.Values.SelectMany(checksInGroup=>checksInGroup).Select(check => new TableContent(check)).Prepend("ServiceName").Append("Average"));
        
        var output = runInfo.ServiceScores.Select(pair =>
        {
            var (fullPathToService, (scoreByCheckName, average)) = pair;
            var serviceName = $"<span title=\"{fullPathToService}\">{Path.GetFileNameWithoutExtension(fullPathToService)}</span>";
            return ToElement(columnElement, scoreByCheckName.Select(check => new TableContent(ColorizeNumber(check.Value))).Prepend(serviceName).Append(ColorizeNumber(average)));
        });
        
        _logger.Information("Generated scorecard at {LastUpdatedAt}", lastUpdatedAt);
        
        return $"{autoGenerationInfo}{Environment.NewLine}{autoGenerationInfo}{Environment.NewLine}{autoGenerationInfo}{Environment.NewLine}{Environment.NewLine}{usageGuide}{Environment.NewLine}{Environment.NewLine}<table style=\"height: 40vh\">{string.Join(Environment.NewLine, output.Prepend(headers).Prepend(groupData).Prepend(""))}</table>{Environment.NewLine}{Environment.NewLine}{generationInfo}{Environment.NewLine}{Environment.NewLine}<!-- {runInfoJSON} -->";
    }

    private record TableContent(string Content, int Colspan = 1)
    { 
        public static implicit operator TableContent(string content) => new TableContent(content);
    }
}
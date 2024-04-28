using Humanizer;
using System.IO;
using System.Text.RegularExpressions;
using Transmittal.Library.Models;
using Transmittal.Library.Extensions;

namespace Transmittal.Library.Helpers;
public static class FilenameParser
{
    public static DocumentModel GetDocumentModel(string filePath, string projectIdentifier, string originator, string role, string exportRule)
    {
        // Generate the regular expression pattern from the export rule
        var pattern = GetPatternFromExportRule(exportRule);

        // Parse the filename using the regular expression pattern
        Regex regex = new Regex(pattern);
        MatchCollection matches = regex.Matches(Path.GetFileNameWithoutExtension(filePath));

        // Get the values for the document properties from the regular expression groups
        var documentProperties = new Dictionary<string, string>();
        foreach (Match match in matches)
        {
            string[] groupNames = regex.GetGroupNames();

            foreach (string groupName in groupNames)
            {
                if (int.TryParse(groupName, out int groupNumber))
                {
                    continue;
                }

                Group group = match.Groups[groupName];
                if (group.Success)
                {
                    if(groupName == "SheetName2")
                    {
                        documentProperties.Add("SheetName", group.Value);
                        continue;
                    }

                    if (groupName == "SheetName")
                    {
                        documentProperties.Add(groupName, group.Value.Humanize().Titleize());
                        continue;
                    }

                    documentProperties.Add(groupName, group.Value);
                }
            }
        }

        DocumentModel document = new DocumentModel
        {
            FileName = Path.GetFileName(filePath),
            DrgProj = projectIdentifier,
            DrgOriginator = originator,
            DrgVolume = documentProperties.GetValueOrDefault("Volume", "ZZ"),
            DrgLevel = documentProperties.GetValueOrDefault("Level", "XX"),
            DrgType = documentProperties.GetValueOrDefault("Type", "DR"),
            DrgRole = role,
            DrgNumber = documentProperties.GetValueOrDefault("SheetNo", "0000"),
            DrgName = documentProperties.GetValueOrDefault("SheetName", "Title"),
            DrgStatus = documentProperties.GetValueOrDefault("Status", "S0"),
            DrgRev = documentProperties.GetValueOrDefault("Rev", "P01")
        };

        return document;
    }

    //private static string GetPatternFromExportRule(string exportRule)
    //{
    //    // Escape any regex special characters in the export rule
    //    var escapedExportRule = Regex.Escape(exportRule);

    //    // Replace tag placeholders with regex capture groups
    //    var tagRegex = new Regex(@"(<\w+>)");
    //    var matchEvaluator = new MatchEvaluator(match => $"(?<{match.Groups[1].Value.Trim('<', '>')}>.*)");
    //    var pattern = tagRegex.Replace(escapedExportRule, matchEvaluator);

    //    // Add optional whitespace and/or hyphen characters between tags
    //    pattern = Regex.Replace(pattern, @"(</\w+>)\s*([^-<])", "$1[- ]*$2");

    //    // Add start and end anchors to the pattern
    //    pattern = $"^{pattern}$";

    //    return pattern;
    //}

    private static string GetPatternFromExportRule(string exportRule)
    {
        // Escape any regex special characters in the export rule
        var escapedExportRule = Regex.Escape(exportRule);

        // Replace curly tag placeholders with regex capture groups
        var tagRegex = new Regex(@"<{1}(?<tag>\w+)>{1}", RegexOptions.ExplicitCapture);
        var matchEvaluator = new MatchEvaluator(match =>
        {
            var tag = match.Groups["tag"].Value;
            return $"(?<{tag}>.*?)";
        });
        var pattern = tagRegex.Replace(escapedExportRule, matchEvaluator);

        // Add optional whitespace and/or hyphen characters between tags
        pattern = Regex.Replace(pattern, @"</\w+>\s{0,4}(?!\n)(?=\w)", "$0[- ]*");
        // Add start and end anchors to the pattern
        pattern = $"^{pattern}$";

        return pattern;
    }

}

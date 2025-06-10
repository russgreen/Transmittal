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
        // Find all tags in order
        var tagRegex = new Regex(@"<(\w+)>");
        var tagMatches = tagRegex.Matches(exportRule).Cast<Match>().ToList();
        var tags = tagMatches.Select(m => m.Groups[1].Value).ToList();

        // We'll build the pattern step by step
        string pattern = Regex.Escape(exportRule);

        for (int i = 0; i < tags.Count; i++)
        {
            var tag = tags[i];
            string replacement;

            // Find the delimiter after this tag in the original exportRule
            int tagEnd = tagMatches[i].Index + tagMatches[i].Length;
            char? delimiter = null;
            if (tagEnd < exportRule.Length)
            {
                char c = exportRule[tagEnd];
                if (c == '-' || c == ' ' || c == '_')
                    delimiter = c;
            }

            if (i == tags.Count - 1)
            {
                // Last tag: match to end
                replacement = $@"(?<{tag}>.+)";
            }
            else if (tag == "SheetNo" && delimiter.HasValue)
            {
                // For SheetNo, match anything (including delimiters) up to the delimiter + next tag's value
                // The next tag's value is not known, but we can look for the delimiter followed by a group that matches the next tag's pattern
                // We'll use a lookahead for delimiter followed by a non-greedy match for the next tag
                replacement = $@"(?<{tag}>.+?)(?={Regex.Escape(delimiter.ToString())}(?=[^" + Regex.Escape(delimiter.ToString()) + @"]+))";
            }
            else if (delimiter.HasValue)
            {
                // For other tags, match up to the next delimiter (non-greedy)
                replacement = $@"(?<{tag}>[^{Regex.Escape(delimiter.ToString())}]+)";
            }
            else
            {
                // Fallback: match up to next non-delimiter
                replacement = $@"(?<{tag}>.+?)";
            }

            pattern = new Regex(Regex.Escape("<" + tag + ">")).Replace(pattern, replacement, 1);
        }

        pattern = pattern.Replace(@"\<", "<").Replace(@"\>", ">");
        pattern = $"^{pattern}$";
        return pattern;
    }

}

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
                {
                    delimiter = c;
                }
            }

            if (i == tags.Count - 1)
            {
                // Last tag: match to end
                replacement = $@"(?<{tag}>.+)";
            }
            else if (tag == "SheetNo" && delimiter.HasValue)
            {
                // For SheetNo, allow internal delimiters within the value, but stop at the delimiter
                // that precedes the next tag followed by its own delimiter (e.g., -<Rev>_)
                char? nextTagFollowingDelimiter = null;
                if (i + 1 < tags.Count)
                {
                    int nextTagEnd = tagMatches[i + 1].Index + tagMatches[i + 1].Length;
                    if (nextTagEnd < exportRule.Length)
                    {
                        char c2 = exportRule[nextTagEnd];
                        if (c2 == '-' || c2 == ' ' || c2 == '_')
                        {
                            nextTagFollowingDelimiter = c2;
                        }
                    }
                }

                replacement = nextTagFollowingDelimiter.HasValue
                    ? $@"(?<{tag}>.+?)(?={Regex.Escape(delimiter.ToString())}(?=[^{Regex.Escape(delimiter.ToString())}]+{Regex.Escape(nextTagFollowingDelimiter.ToString())}))"
                    : $@"(?<{tag}>[^{Regex.Escape(delimiter.ToString())}]+)";
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

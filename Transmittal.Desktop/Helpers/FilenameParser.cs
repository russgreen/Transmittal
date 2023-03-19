using Humanizer;
using System.IO;
using System.Text.RegularExpressions;
using Transmittal.Library.Models;
using Transmittal.Library.Extensions;

namespace Transmittal.Desktop.Helpers;
internal static class FilenameParser
{
    internal static DocumentModel DocumentModel(string filePath, string projectIdentifier, string originator, string role)
    {
        string DocNo = "0000";
        string Volume = "ZZ";
        string Level = "XX";
        string DocType = "RP";
        string DocTitle = "Title";
        string Status = "S0";
        string Revision = "P01";

        // <ProjId>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>
        // RCO-O3S-01-00-GA-A-2000-FloorPlanLevel00-S0-P1.pdf
        // RCO-O3S-10-00-GA-A-10-GroundFloorPlan-S0-P1.pdf

        FileInfo fi = new FileInfo(filePath);

        string ext = fi.Extension;
        // regex patterns testing and built with http://regexstorm.net/tester
        string pattern1 = @"\w{2,6}\u002D\w{2,4}\u002D\w{2,5}\u002D\w{2,5}\u002D\w{2}\u002D\w\u002D\w{2,4}\u002D"; // <ProjId>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-
        string pattern2 = @"\w{2,6}\u002D\w{2,4}\u002D\w{2,5}\u002D\w{2,5}\u002D\w{2}\u002D\w\u002D"; // <ProjId>-<Originator>-<Volume>-<Level>-<Type>-<Role>-
        string pattern3 = @"\u002D\D\w\u002D\D\w+?" + ext; // -<Status>-<Rev>.ext
        string s1 = "";
        string s2 = "";
        string s3 = "";
        string s4 = "";
        Regex rgx;
        rgx = new Regex(pattern1, RegexOptions.IgnoreCase);
        if (rgx.IsMatch(fi.Name))
        {
            s1 = rgx.Match(fi.Name).Value.ToString();
            var sArr = s1.Split('-');
            Volume = sArr[2];
            Level = sArr[3];
            DocType = sArr[4];
            DocNo = sArr[6];
        }
        else
        {
            rgx = new Regex(pattern2, RegexOptions.IgnoreCase);
            if (rgx.IsMatch(fi.Name))
            {
                s1 = rgx.Match(fi.Name).Value.ToString();
                var sArr = s1.Split('-');
                Volume = sArr[2];
                Level = sArr[3];
                DocType = sArr[4];
                DocNo = sArr[6];
            }
        }

        rgx = new Regex(pattern3, RegexOptions.IgnoreCase);
        if (rgx.IsMatch(fi.Name))
        {
            s2 = rgx.Match(fi.Name).Value.ToString();
            var sArr = s2.Split('-');
            Status = sArr[1];
            Revision = sArr[2].Replace(ext, "");
        }

        // Try and extract a title from whats left
        if (rgx.IsMatch(fi.Name))
        {
            if (s1.Length > 0)
                s3 = fi.Name.Replace(s1, "");
            if (s3.StartsWith("-"))
            {
                s3.Remove(0, 1);
            }

            if (s3.Length >= 10)
            {
                // must be more characters left than <Status>-<Rev>.ext so they could be the title
                s4 = s3.Replace(s2.Remove(0, 1), "");
                if ((s4 ?? "") != (string.Empty ?? ""))
                {
                    if (s4.EndsWith("-"))
                    {
                        DocTitle = s4.Remove(s4.Length - 1).Humanize().Transform(To.TitleCase);
                    }
                }
            }
        }

        DocumentModel document = new DocumentModel
        {
            FileName = fi.Name,
            DrgProj = projectIdentifier,
            DrgOriginator = originator,
            DrgVolume = Volume,
            DrgLevel = Level,
            DrgType = DocType,
            DrgRole = role,
            DrgNumber = DocNo,
            DrgName = DocTitle,
            DrgStatus = Status,
            DrgRev = Revision
        };

        return document;
    }

    public static DocumentModel DocumentModel(string filePath, string projectIdentifier, string originator, string role, string exportRule)
    {
        // Generate the regular expression pattern from the export rule
        var pattern = GetPatternFromExportRule(exportRule);

        // Parse the filename using the regular expression pattern
        var match = Regex.Match(Path.GetFileNameWithoutExtension(filePath), pattern);
        if (!match.Success)
        {
            //throw new Exception("Filename does not match the export rule.");
        }

        // Get the values for the document properties from the regular expression groups
        var documentProperties = new Dictionary<string, string>();
        foreach (Group group in match.Groups)
        {
            documentProperties[group.Name] = group.Value;
        }

        DocumentModel document = new DocumentModel
        {
            FileName = Path.GetFileName(filePath),
            DrgProj = projectIdentifier,
            DrgOriginator = originator,
            DrgVolume = documentProperties.GetValueOrDefault("Volume", "ZZ"),
            DrgLevel = documentProperties.GetValueOrDefault("Level", "XX"),
            DrgType = documentProperties.GetValueOrDefault("Type", " "),
            DrgRole = role,
            DrgNumber = documentProperties.GetValueOrDefault("SheetNo", "0000"),
            DrgName = documentProperties.GetValueOrDefault("SheetName", "Title").Humanize(),
            DrgStatus = documentProperties.GetValueOrDefault("Status", "S0"),
            DrgRev = documentProperties.GetValueOrDefault("Rev", "P01")
        };

        return document;
    }

    private static string GetPatternFromExportRule(string exportRule)
    {
        // Escape any regex special characters in the export rule
        var escapedExportRule = Regex.Escape(exportRule);

        // Replace tag placeholders with regex capture groups
        var tagRegex = new Regex(@"(<\w+>)");
        var matchEvaluator = new MatchEvaluator(match => $"(?<{match.Groups[1].Value.Trim('<', '>')}>.*)");
        var pattern = tagRegex.Replace(escapedExportRule, matchEvaluator);

        // Add optional whitespace and/or hyphen characters between tags
        pattern = Regex.Replace(pattern, @"(</\w+>)\s*([^-<])", "$1[- ]*$2");

        // Add start and end anchors to the pattern
        pattern = $"^{pattern}$";

        return pattern;
    }
}

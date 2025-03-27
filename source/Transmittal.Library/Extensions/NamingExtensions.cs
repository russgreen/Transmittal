using Humanizer;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Transmittal.Library.Extensions;

public static class NamingExtensions
{
    /// <summary>
    /// Parses out folder paths that contain an optional <Format>, <DateDD>, <DateMM>, <DateYY>, <DateYYYY>, <Package>, <SheetCollection> tags
    /// </summary>
    /// <param name="path">the path the the folder containing the <FORMAT> tag</param>
    /// <param name="format">the text to replace the <FORMAT>  tag with, e.g. PDF, DWG, etc</param>
    /// <returns></returns>
    public static string ParseFolderName(this string path, string format, string package = null, string sheetCollection = null)
    {
        path = path.ParsePathWithEnvironmentVariables();

        var replacements = new Dictionary<string, string>
        {
            { "<DateDD>", DateTime.Now.ToStringDD() },
            { "<DateMM>", DateTime.Now.ToStringMM() },
            { "<DateYY>", DateTime.Now.ToStringYY() },
            { "<DateYYYY>", DateTime.Now.Year.ToString() },
            { "<Format>", format },
            { "<Package>", package },
            { "<SheetCollection>", sheetCollection }
        };

        foreach (var replacement in replacements)
        {
            if (!string.IsNullOrEmpty(replacement.Value))
            {
                path = path.Replace(replacement.Key, replacement.Value);
            }
        }

        return path;
    }

    /// <summary>
    /// Parses out folder paths that contain optional environment variables tags, %UserProfile%, %OneDriveConsumer%, %OneDriveCommercial%
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ParsePathWithEnvironmentVariables(this string path)
    {
        path = path.Replace("%UserProfile%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        path = path.Replace("%OneDriveConsumer%", Environment.GetEnvironmentVariable("OneDriveConsumer"));
        path = path.Replace("%OneDriveCommercial%", Environment.GetEnvironmentVariable("OneDriveCommercial"));

        return path;
    }

    /// <summary>
    /// Parses out file names that contain optional tags
    /// </summary>
    /// <param name="filenameFilter"></param>
    /// <param name="projNo"></param>
    /// <param name="projId"></param>
    /// <param name="projName"></param>
    /// <param name="originator"></param>
    /// <param name="volume"></param>
    /// <param name="level"></param>
    /// <param name="type"></param>
    /// <param name="role"></param>
    /// <param name="sheetNo"></param>
    /// <param name="sheetName"></param>
    /// <param name="rev"></param>
    /// <param name="status"></param>
    /// <param name="status2">The status description</param>
    /// <returns></returns>
    public static string ParseFilename(this string filenameFilter, string projNo, string projId, string projName, string originator, 
        string volume, string level, string type, string role, string sheetNo, string sheetName, string rev, string status, string statusDescription)
     {
        string fileName = filenameFilter;
        // <ProjNo>
        // <ProjId>
        // <Originator>
        // <Volume>
        // <Level>
        // <Type>
        // <Role>
        // <ProjName>
        // <SheetNo>
        // <SheetName>
        // <SheetName2>
        // <Status>
        // <StatusDescription>
        // <Rev>
        // <DateYY>
        // <DateYYYY>
        // <DateMM>
        // <DateDD>

        //if ((rev ?? "") == (string.Empty ?? ""))
        //    rev = "P00";

        var now = DateTime.Now;

        fileName = fileName.Replace("<Originator>", originator);
        fileName = fileName.Replace("<Volume>", volume);
        fileName = fileName.Replace("<Level>", level);
        fileName = fileName.Replace("<Type>", type);
        fileName = fileName.Replace("<Role>", role);
        fileName = fileName.Replace("<Volume>", volume);
        fileName = fileName.Replace("<ProjId>", projId);
        fileName = fileName.Replace("<ProjNo>", projNo);
        fileName = fileName.Replace("<ProjName>", projName);
        fileName = fileName.Replace("<SheetNo>", sheetNo);
        fileName = fileName.Replace("<SheetName>", sheetName.Dehumanize()); 
        fileName = fileName.Replace("<SheetName2>", sheetName);
        fileName = fileName.Replace("<Rev>", rev);
        fileName = fileName.Replace("<Status>", status);
        fileName = fileName.Replace("<StatusDescription>", statusDescription);
        fileName = fileName.Replace("<DateDD>", now.ToStringDD());
        fileName = fileName.Replace("<DateMM>", now.ToStringMM());
        fileName = fileName.Replace("<DateYY>", now.ToStringYY());
        fileName = fileName.Replace("<DateYYYY>", now.Year.ToString());
        
        return fileName.RemoveIllegalCharacters().RemoveTrailingSymbols();
    }

    public static string RemoveIllegalCharacters(this string illegalString)
    {
        string retval = illegalString;
        // this function will remove these characters \ / : * ? < > | 
        // from string and replace them a space
        retval = retval.Replace(@"\", " ");
        retval = retval.Replace("/", " ");
        retval = retval.Replace(":", "-");
        retval = retval.Replace("*", " ");
        retval = retval.Replace("?", " ");
        retval = retval.Replace("\"", " ");
        retval = retval.Replace("<", " ");
        retval = retval.Replace(">", " ");
        retval = retval.Replace("|", " ");
        retval = retval.Replace("'", "");

        return retval;
    }

    public static string RemoveTrailingSymbols(this string inputString)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            return inputString;
        }

        // remove trailing spaces and non-alphanumeric characters
        inputString.Trim();
        while (inputString.Length > 0 && !Char.IsLetterOrDigit(inputString[inputString.Length - 1]))
        {
            inputString = inputString.Remove(inputString.Length - 1, 1);
        }
        return inputString;
    }

    public static bool IsValidEmailAddress(this string inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
            return false;

        try
        {
            // Normalize the domain
            inputString = Regex.Replace(inputString, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines if the email is in proper form
            if (!Regex.IsMatch(inputString, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    private static string DomainMapper(Match match)
    {
        // Use IdnMapping class to convert Unicode domain names.
        var idn = new IdnMapping();

        // Pull out and process domain name (throws ArgumentException on invalid)
        var domainName = idn.GetAscii(match.Groups[2].Value);

        return match.Groups[1].Value + domainName;
    }
}

using Humanizer;

namespace Transmittal.Library.Extensions;

public static class NamingExtensions
{
    /// <summary>
    /// Parses out folder paths that contain an optional <Format>, <DateDD>, <DateMM>, <DateYY>, <DateYYYY> tags
    /// </summary>
    /// <param name="path">the path the the folder containing the <FORMAT> tag</param>
    /// <param name="format">the text to replace the <FORMAT>  tag with, e.g. PDF, DWG, etc</param>
    /// <returns></returns>
    public static string ParseFolderName(this string path, string format)
    {
        path = path.ParsePathWithEnvironmentVariables();
        //path.Replace("%UserProfile%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        //path = path.Replace("%OneDriveConsumer%", Environment.GetEnvironmentVariable("OneDriveConsumer"));
        //path = path.Replace("%OneDriveCommercial%", Environment.GetEnvironmentVariable("OneDriveCommercial"));

        path = path.Replace("<DateDD>", DateTime.Now.ToStringDD());
        path = path.Replace("<DateMM>", DateTime.Now.ToStringMM());
        path = path.Replace("<DateYY>", DateTime.Now.ToStringYY());
        path = path.Replace("<DateYYYY>", DateTime.Now.Year.ToString());

        path = path.Replace("<Format>", format);

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
}

using Humanizer;
using System.IO;
using System.Text.RegularExpressions;
using Transmittal.Library.Models;

namespace Transmittal.Desktop.Util;
internal static class ISO19650Parser
{
    internal static List<DocumentTypeModel> GetDocumentTypes()
    {
        List<DocumentTypeModel> documentTypes = new List<DocumentTypeModel>();

        documentTypes.Clear();
        documentTypes.Add(new DocumentTypeModel() { Code = "AF", Description = "Animation file (of a model)" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CM", Description = "Combined model (combined multidiscipline model)" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CR", Description = "Specific for the clash process" });
        documentTypes.Add(new DocumentTypeModel() { Code = "DR", Description = "2D drawing" });
        documentTypes.Add(new DocumentTypeModel() { Code = "M2", Description = "2D model file" });
        documentTypes.Add(new DocumentTypeModel() { Code = "M3", Description = "3D model file" });
        documentTypes.Add(new DocumentTypeModel() { Code = "MR", Description = "Model rendition file For other renditions, e.g thermal analysis etc." });
        documentTypes.Add(new DocumentTypeModel() { Code = "VS", Description = "Visualization file (of a model)" });
        documentTypes.Add(new DocumentTypeModel() { Code = "BQ", Description = "Bill of quantities" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CA", Description = "Calculations" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CO", Description = "Correspondence" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CP", Description = "Cost plan" });
        documentTypes.Add(new DocumentTypeModel() { Code = "DB", Description = "Database" });
        documentTypes.Add(new DocumentTypeModel() { Code = "FN", Description = "File note" });
        documentTypes.Add(new DocumentTypeModel() { Code = "HS", Description = "Health And safety" });
        documentTypes.Add(new DocumentTypeModel() { Code = "IE", Description = "Information exchange file" });
        documentTypes.Add(new DocumentTypeModel() { Code = "MI", Description = "Minutes / Action notes" });
        documentTypes.Add(new DocumentTypeModel() { Code = "MS", Description = "Method statement" });
        documentTypes.Add(new DocumentTypeModel() { Code = "PP", Description = "Presentation" });
        documentTypes.Add(new DocumentTypeModel() { Code = "PR", Description = "Programme" });
        documentTypes.Add(new DocumentTypeModel() { Code = "RD", Description = "Room data sheet" });
        documentTypes.Add(new DocumentTypeModel() { Code = "RI", Description = "Request for information" });
        documentTypes.Add(new DocumentTypeModel() { Code = "RP", Description = "Report" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SA", Description = "Schedule of accommodation" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SH", Description = "Schedule" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SN", Description = "Snagging list" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SP", Description = "Specification" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SU", Description = "Survey" });
        documentTypes.Add(new DocumentTypeModel() { Code = "GA", Description = "General arrangement drawing" });
        documentTypes.Add(new DocumentTypeModel() { Code = "DT", Description = "Detail/assembly drawing" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SK", Description = "Sketch drawings" });

        return documentTypes;
    }

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
        string pattern1 = @"\w{2,6}\u002D\w{2,4}\u002D\w{2}\u002D\w{2}\u002D\w{2}\u002D\w\u002D\w{2,4}\u002D"; // <ProjId>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-
        string pattern2 = @"\w{2,6}\u002D\w{2,4}\u002D\w{2}\u002D\w{2}\u002D\w{2}\u002D\w\u002D"; // <ProjId>-<Originator>-<Volume>-<Level>-<Type>-<Role>-
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
}

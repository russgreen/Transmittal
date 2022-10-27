namespace Transmittal.Library.Models;

public partial class SettingsModel : BaseModel
{
    public string DateFormatString { get; set; } = "dd.MM.yy";

    public string ProjectNumber { get; set; }
    public string ProjectIdentifier { get; set; }
    public string ProjectName { get; set; }
    public string ClientName { get; set; }

    public string Originator { get; set; }
    public string Role { get; set; }

    public bool RecordTransmittals { get; set; } = false;
    public string DatabaseFile { get; set; } = "[NONE]";
    public string DatabaseTemplateFile { get; set; } = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\Data\TemplateDatabase.tdb";
        
    public string DrawingIssueStore { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "DrawingIssues");
    public string IssueSheetStore { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "IssueSheets");
    public string DirectoryStore { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "Directory");
    public string ReportStore { get; set; } = string.Empty;
        
    public string FileNameFilter { get; set; } = "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>";
    public string FileNameFilter2 { get; set; } = "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>";
    public bool UseExtranet { get; set; } = false;
    public bool UseISO19650 { get; set; } = false;

    public List<IssueFormatModel> IssueFormats { get; set; } = new List<IssueFormatModel>()
    {
        new IssueFormatModel() { Code = "P", Description = "Paper" },
        new IssueFormatModel() { Code = "C", Description = "Cloud" },
        new IssueFormatModel() { Code = "E", Description = "Email" },
    };
    public List<DocumentStatusModel> DocumentStatuses { get; set; } = new List<DocumentStatusModel>()
    {
        new DocumentStatusModel() { Code = "S0", Description = "PRELIMINARY WIP" },
        new DocumentStatusModel() { Code = "S1", Description = "FOR CO-ORDINATION" },
        new DocumentStatusModel() { Code = "S2", Description = "FOR INFORMATION" },
        new DocumentStatusModel() { Code = "S3", Description = "FOR REVIEW AND COMMENT" },
        new DocumentStatusModel() { Code = "S4", Description = "FOR STAGE APPROVAL" },
        new DocumentStatusModel() { Code = "S6", Description = "FOR PIM AUTHORIZATION" },
        new DocumentStatusModel() { Code = "S7", Description = "FOR AIM AUTHORIZATION" },
        new DocumentStatusModel() { Code = "D1", Description = "SUITABLE FOR COSTING" }, // old BS1192 but useful
        new DocumentStatusModel() { Code = "D2", Description = "SUITABLE FOR TENDER" }, // old BS1192 but useful
        new DocumentStatusModel() { Code = "D3", Description = "FOR CONTRACTOR DESIGN" }, // old BS1192 but useful

        new DocumentStatusModel() { Code = "A3", Description = "CONTRACTUAL STAGE 3" },
        new DocumentStatusModel() { Code = "A4", Description = "CONTRACTUAL STAGE 4" },
        new DocumentStatusModel() { Code = "A5", Description = "CONTRACTUAL STAGE 5" },
        new DocumentStatusModel() { Code = "A6", Description = "CONTRACTUAL STAGE 6" },

        new DocumentStatusModel() { Code = "B3", Description = "PARTIAL STAGE 3" },
        new DocumentStatusModel() { Code = "B4", Description = "PARTIAL STAGE 4" },
        new DocumentStatusModel() { Code = "B5", Description = "PARTIAL STAGE 5" },
        new DocumentStatusModel() { Code = "B6", Description = "PARTIAL STAGE 6" },

        new DocumentStatusModel() { Code = "CR", Description = "AS BUILT" },
    };

    // names and guids of used shared parameters.  Allowing these to be changed means existing shared parameters can be used on projects.
    public bool UseCustomSharedParameters { get; set; } = false;
    // project paramaters
    public string ProjectIdentifierParamName { get; set; } 
    public string ProjectIdentifierParamGuid { get; set; } 
    public string OriginatorParamName { get; set; } 
    public string OriginatorParamGuid { get; set; }
    public string RoleParamName { get; set; }
    public string RoleParamGuid { get; set; }
    // sheet parameters
    public string SheetVolumeParamName { get; set; }
    public string SheetVolumeParamGuid { get; set; }
    public string SheetLevelParamName { get; set; }
    public string SheetLevelParamGuid { get; set; }
    public string DocumentTypeParamName { get; set; }
    public string DocumentTypeParamGuid { get; set; }
    public string SheetStatusParamName { get; set; }
    public string SheetStatusParamGuid { get; set; }
    public string SheetStatusDescriptionParamName { get; set; }
    public string SheetStatusDescriptionParamGuid { get; set; }
}

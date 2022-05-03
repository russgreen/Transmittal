namespace Transmittal.Library.Models;

public class SettingsModel
{
    public string DateFormatString { get; set; } = "dd.MM.yy";

    public string ProjectNumber { get; set; }
    public string ProjectIdentifier { get; set; }
    public string ProjectName { get; set; }
    public string Originator { get; set; }    
    public string Role { get; set; }


    public bool RecordTransmittals { get; set; } = false;
    public string DatabaseFile { get; set; } = "[NONE]";
    public string DatabaseTemplateFile { get; set; } = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\Data\TemplateDatabase.tdb";

    public string DrawingIssueStore { get; set; }
    public string IssueSheetStore { get; set; }
    public string DirectoryStore { get; set; }
    public string ReportStore { get; set; } = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\Reports\";


    public string FileNameFilter { get; set; } = "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>";
    public bool UseExtranet { get; set; } = false;
    public bool UseISO19650 { get; set; } = false;

    public List<IssueFormatModel> IssueFormats;
    public List<DocumentStatusModel> DocumentStatuses;

    // names and guids of used shared parameters.  Allowing these to be changed means existing shared parameters can be used on projects.
    // project paramaters
    public string ProjectIdentifierParamName { get; set; } = "Project Identifier";
    public string ProjectIdentifierParamGuid { get; set; } = "ce8c18ee-3b90-4f42-8938-ae90e3af5a6a";
    public string OriginatorParamName { get; set; } = "Originator";
    public string OriginatorParamGuid { get; set; } = "e45313b7-8419-4803-92f0-68558f9278b2";
    public string RoleParamName { get; set; } = "Role";
    public string RoleParamGuid { get; set; } = "67fcb5e8-4ffb-43b8-8ec9-c664fd997267";
    // sheet parameters
    public string SheetVolumeParamName { get; set; } = "Sheet Volume";
    public string SheetVolumeParamGuid { get; set; } = "9c16757c-175a-451a-a5d4-c4a6ff291acb";
    public string SheetLevelParamName { get; set; } = "Sheet Level";
    public string SheetLevelParamGuid { get; set; } = "e51af162-9025-48a0-bd2c-bc833fab0db0";
    public string DocumentTypeParamName { get; set; } = "Document Type";
    public string DocumentTypeParamGuid { get; set; } = "eb57d296-7d9c-459f-ace1-0bdaf95c3b29";
    public string SheetStatusParamName { get; set; } = "Sheet Status";
    public string SheetStatusParamGuid { get; set; } = "3304f169-ceb9-40b9-a69d-d8f3eb0a3fb9";
    public string SheetStatusDescriptionParamName { get; set; } = "Sheet Status Description";
    public string SheetStatusDescriptionParamGuid { get; set; } = "4effad6a-f05d-43dd-afb1-c2b6c5cb5b9a";
}

using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace Transmittal.Library.Models;

public partial class SettingsModel : ObservableValidator
{
    [ObservableProperty]
    private string _dateFormatString = "dd.MM.yy";

    [ObservableProperty]
    private string _projectNumber = string.Empty;

    [ObservableProperty]
    private string _projectIdentifier = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _clientName = string.Empty;

    [ObservableProperty]
    private string _originator = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private bool _recordTransmittals = false;

    [ObservableProperty]
    private string _databaseFile = "[NONE]";

    [ObservableProperty]
    private string _databaseTemplateFile = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\Data\TemplateDatabase.tdb";

    [ObservableProperty]
    private string _drawingIssueStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "DrawingIssues");

    [ObservableProperty]
    private string _issueSheetStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "IssueSheets");

    [ObservableProperty]
    private string _directoryStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Transmittal", "Directory");

    [ObservableProperty]
    private string _reportStore = string.Empty;

    [ObservableProperty]
    private string _fileNameFilter = "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>";

    [ObservableProperty]
    private string _fileNameFilter2 = "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>";

    [ObservableProperty]
    private bool _useExtranet = false;

    [ObservableProperty]
    private bool _useISO19650 = false;

    [ObservableProperty]
    private bool _useRevit = false;

    [ObservableProperty]
    private List<IssueFormatModel> _issueFormats = new List<IssueFormatModel>()
    {
        new IssueFormatModel() { Code = "P", Description = "Paper" },
        new IssueFormatModel() { Code = "C", Description = "Cloud" },
        new IssueFormatModel() { Code = "E", Description = "Email" },
    };

    [ObservableProperty]
    private List<DocumentStatusModel> _documentStatuses = new List<DocumentStatusModel>()
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
    [ObservableProperty]
    private bool _useCustomSharedParameters = false;

    // project paramaters
    [ObservableProperty]
    private string _projectIdentifierParamName;

    [ObservableProperty]
    private string _projectIdentifierParamGuid;

    [ObservableProperty]
    private string _originatorParamName;

    [ObservableProperty]
    private string _originatorParamGuid;

    [ObservableProperty]
    private string _roleParamName;

    [ObservableProperty]
    private string _roleParamGuid;

    // sheet parameters
    [ObservableProperty]
    private string _sheetVolumeParamName;

    [ObservableProperty]
    private string _sheetVolumeParamGuid;

    [ObservableProperty]
    private string _sheetLevelParamName;

    [ObservableProperty]
    private string _sheetLevelParamGuid;

    [ObservableProperty]
    private string _documentTypeParamName;

    [ObservableProperty]
    private string _documentTypeParamGuid;

    [ObservableProperty]
    private string _sheetStatusParamName;

    [ObservableProperty]
    private string _sheetStatusParamGuid;

    [ObservableProperty]
    private string _sheetStatusDescriptionParamName;

    [ObservableProperty]
    private string _sheetStatusDescriptionParamGuid;

    [ObservableProperty]
    private string _sheetPackageParamName;

    [ObservableProperty]
    private string _sheetPackageParamGuid;
}

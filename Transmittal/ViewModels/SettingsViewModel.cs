using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;
using Transmittal.Library.Models; 
using Transmittal.Library.ViewModels;
using Transmittal.Library.Services;
using Transmittal.Services;
using Transmittal.Extensions;
using System.ComponentModel.DataAnnotations;
using Autodesk.Revit.DB;
using System.Windows.Controls;
using Transmittal.Library.Extensions;
using Transmittal.Requesters;
using Transmittal.Library.Validation;
using CommunityToolkit.Mvvm.Messaging;
using Transmittal.Messages;
using Transmittal.Models;

namespace Transmittal.ViewModels;

internal partial class SettingsViewModel : BaseViewModel, IParameterGuidRequester
{
    public string WindowTitle { get; private set; }

    private readonly ISettingsServiceRvt _settingsServiceRvt = Host.GetService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public List<string> FolderNameParts => new List<string> { "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>", "<Format>", "%UserProfile%", "%OneDriveConsumer%", "%OneDriveCommercial%" };
    public List<string> FileNameParts => new List<string> { "<ProjNo>", "<ProjId>", "<Originator>", "<Volume>", "<Level>", "<Type>", "<Role>", "<ProjName>", "<SheetNo>", "<SheetName>", "<SheetName2>", "<Status>", "<StatusDescription>", "<Rev>", "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>" };
    
    public string ProjectNumber;
    public string Originator;
    public string Role;
        
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _fileNameFilter;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drawingIssueStore;

    [ObservableProperty]
    private string _sampleFolderName;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _dateFormatString;

    [ObservableProperty]
    private string _sampleDateString;

    [ObservableProperty]
    private bool _useISO19650;

    [ObservableProperty]
    private bool _useExtranet;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _fileNameFilter2;

    [ObservableProperty]
    private List<IssueFormatModel> _issueFormats;

    [ObservableProperty]
    private List<DocumentStatusModel> _documentStatuses;

    [ObservableProperty]
    private bool _recordTransmittals;
    [ObservableProperty]
    private string _databaseFile;
    [ObservableProperty]
    private string _databaseTemplateFile;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MustBeFalse(ErrorMessage = "Database file is not found")] 
    private bool _databaseNotFound; //used to control visibility of error message in UI
    [ObservableProperty]
    private string _reportTemplatePath;
    [ObservableProperty]
    private string _issueSheetStorePath;
    [ObservableProperty]
    private string _directoryStorePath;

    [ObservableProperty]
    private bool _useCustomSharedParameters;

    [ObservableProperty]
    private string _projectIdentifierParamGuid;
    [ObservableProperty]
    private string _originatorParamGuid;
    [ObservableProperty]
    private string _roleParamGuid;

    [ObservableProperty]
    private string _sheetVolumeParamGuid;
    [ObservableProperty]
    private string _sheetLevelParamGuid;
    [ObservableProperty]
    private string _documentTypeParamGuid;
    [ObservableProperty]
    private string _sheetStatusParamGuid;
    [ObservableProperty]
    private string _sheetStatusDescriptionParamGuid;

    public SettingsViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);

        ProjectNumber = _settingsService.GlobalSettings.ProjectNumber;
        Originator = _settingsService.GlobalSettings.Originator;
        Role = _settingsService.GlobalSettings.Role;

        //Settings = _settingsService.GlobalSettings;
        CheckForDatabaseFile();

        SetPropertiesFromGlobalSettings();

        WeakReferenceMessenger.Default.Register<ImportSettingsMessage>(this, (r, m) =>
        {
            SetPropertiesFromImportedSettings(m.Value);
        });
    }

    private void SetPropertiesFromGlobalSettings()
    {
        //BASIC SETTINGS
        FileNameFilter = _settingsService.GlobalSettings.FileNameFilter;
        DrawingIssueStore = _settingsService.GlobalSettings.DrawingIssueStore;
        DateFormatString = _settingsService.GlobalSettings.DateFormatString;

        UseISO19650 = _settingsService.GlobalSettings.UseISO19650;
        UseExtranet = _settingsService.GlobalSettings.UseExtranet;
        FileNameFilter2 = _settingsService.GlobalSettings.FileNameFilter2;

        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;

        //DATABASE SETTINGS
        RecordTransmittals = _settingsService.GlobalSettings.RecordTransmittals;
        DatabaseFile = _settingsService.GlobalSettings.DatabaseFile;
        DatabaseTemplateFile = _settingsService.GlobalSettings.DatabaseTemplateFile;
        ReportTemplatePath = _settingsService.GlobalSettings.ReportStore;
        IssueSheetStorePath = _settingsService.GlobalSettings.IssueSheetStore;
        DirectoryStorePath = _settingsService.GlobalSettings.DirectoryStore;

        //ADVANCED SETTINGS
        UseCustomSharedParameters = _settingsService.GlobalSettings.UseCustomSharedParameters;
        // project parameters
        ProjectIdentifierParamGuid = _settingsService.GlobalSettings.ProjectIdentifierParamGuid;
        OriginatorParamGuid = _settingsService.GlobalSettings.OriginatorParamGuid;
        RoleParamGuid = _settingsService.GlobalSettings.RoleParamGuid;
        // sheet parameters
        SheetVolumeParamGuid = _settingsService.GlobalSettings.SheetVolumeParamGuid;
        SheetLevelParamGuid = _settingsService.GlobalSettings.SheetLevelParamGuid;
        DocumentTypeParamGuid = _settingsService.GlobalSettings.DocumentTypeParamGuid;
        SheetStatusParamGuid = _settingsService.GlobalSettings.SheetStatusParamGuid;
        SheetStatusDescriptionParamGuid = _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid;
    }

    private void SetPropertiesFromImportedSettings(ImportSettingsModel settings)
    {
        //BASIC SETTINGS
        FileNameFilter = settings.FileNameFilter;
        DrawingIssueStore = settings.DrawingIssueStore;
        DateFormatString = settings.DateFormatString;

        UseISO19650 = settings.UseISO19650;
        UseExtranet = settings.UseExtranet;
        FileNameFilter2 = settings.FileNameFilter2;

        IssueFormats = settings.IssueFormats;
        DocumentStatuses = settings.DocumentStatuses;

        //DATABASE SETTINGS
        RecordTransmittals = settings.RecordTransmittals;
        DatabaseFile = string.Empty; //we can't really save the database file in an import settings file
        //DatabaseTemplateFile = settings.DatabaseTemplateFile; // we're not saving the template file path at the moment so not using this
        ReportTemplatePath = settings.ReportStore;
        IssueSheetStorePath = settings.IssueSheetStore;
        DirectoryStorePath = settings.DirectoryStore;

        //ADVANCED SETTINGS
        UseCustomSharedParameters = settings.UseCustomSharedParameters;
        // project parameters
        ProjectIdentifierParamGuid = settings.ProjectIdentifierParamGuid;
        OriginatorParamGuid = settings.OriginatorParamGuid;
        RoleParamGuid = settings.RoleParamGuid;
        // sheet parameters
        SheetVolumeParamGuid = settings.SheetVolumeParamGuid;
        SheetLevelParamGuid = settings.SheetLevelParamGuid;
        DocumentTypeParamGuid = settings.DocumentTypeParamGuid;
        SheetStatusParamGuid = settings.SheetStatusParamGuid;
        SheetStatusDescriptionParamGuid = settings.SheetStatusDescriptionParamGuid;

        //TODO check if all the parameters exist in the project or load them from the shared parameters file.
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _settingsService.GlobalSettings.FileNameFilter = FileNameFilter?.Trim();
        _settingsService.GlobalSettings.DrawingIssueStore = DrawingIssueStore?.Trim();
        _settingsService.GlobalSettings.DateFormatString = DateFormatString?.Trim();

        _settingsService.GlobalSettings.UseISO19650 = UseISO19650;
        _settingsService.GlobalSettings.UseExtranet = UseExtranet;
        _settingsService.GlobalSettings.FileNameFilter2 = FileNameFilter2?.Trim();

        _settingsService.GlobalSettings.IssueFormats = IssueFormats;
        _settingsService.GlobalSettings.DocumentStatuses = DocumentStatuses;

        
        _settingsService.GlobalSettings.RecordTransmittals = RecordTransmittals;
        _settingsService.GlobalSettings.DatabaseFile = DatabaseFile;
        _settingsService.GlobalSettings.DatabaseTemplateFile = DatabaseTemplateFile;
        _settingsService.GlobalSettings.ReportStore = ReportTemplatePath?.Trim();
        _settingsService.GlobalSettings.IssueSheetStore = IssueSheetStorePath?.Trim();
        _settingsService.GlobalSettings.DirectoryStore = DirectoryStorePath?.Trim();

        _settingsService.GlobalSettings.UseCustomSharedParameters = UseCustomSharedParameters;
        
        _settingsService.GlobalSettings.ProjectIdentifierParamGuid = ProjectIdentifierParamGuid;
        _settingsService.GlobalSettings.OriginatorParamGuid = OriginatorParamGuid;
        _settingsService.GlobalSettings.RoleParamGuid = RoleParamGuid;
        _settingsService.GlobalSettings.SheetVolumeParamGuid = SheetVolumeParamGuid;
        _settingsService.GlobalSettings.SheetLevelParamGuid = SheetLevelParamGuid;
        _settingsService.GlobalSettings.DocumentTypeParamGuid = DocumentTypeParamGuid;
        _settingsService.GlobalSettings.SheetStatusParamGuid = SheetStatusParamGuid;
        _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = SheetStatusDescriptionParamGuid;

        _settingsServiceRvt.UpdateSettingsRvt();

        if (RecordTransmittals == true)
        {
            // we have a database file so save a copy of settings to the database for use by desktop.exe
            _settingsService.UpdateSettings();
        }

        this.OnClosingRequest();
    }

    [RelayCommand]
    private void AppendToFileNameFilter(string filter)
    {
        if (filter != null && !FileNameFilter.Contains(filter))
        {
            FileNameFilter += $"{filter}";
        }
    }

    [RelayCommand]
    private void AppendToFileNameFilter2(string filter)
    {
        if (filter != null && !FileNameFilter2.Contains(filter))
        {
            FileNameFilter2 += $"{filter}";
        }
    }

    [RelayCommand]
    private void AppendToFolderPath(string filter)
    {   
        if (filter != null && !DrawingIssueStore.Contains(filter))
        {
            if (DrawingIssueStore.StartsWith("%"))
            {
                return;
            }

            if(filter.StartsWith("%")) 
            {
                DrawingIssueStore = filter += DrawingIssueStore;
                return;
            }

            DrawingIssueStore += $"{filter}";
        }
    }

    [RelayCommand]
    private void AddParametersToProject()
    {
        using var t = new Transaction(App.RevitDocument);
        t.Start("Add Parameters to Project");
        
        //get current shared parameters file
        var currentSharedParametersFile = App.CachedUiApp.Application.SharedParametersFilename;

        //load transmittal shared parameters file
        App.CachedUiApp.Application.SharedParametersFilename = $@"{System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location)}\Resources\TransmittalParameters.txt";

        var sharedParameterDefinitionFile = App.CachedUiApp.Application.OpenSharedParameterFile();

        //load shared parameters
        var groupProject = sharedParameterDefinitionFile.Groups.get_Item("Project Parameters");
        var groupSheets = sharedParameterDefinitionFile.Groups.get_Item("Sheet Information");

        //add project parameters
        var projectIdentifierParam = groupProject.Definitions.get_Item(_settingsService.GlobalSettings.ProjectIdentifierParamName);
        var originatorParam = groupProject.Definitions.get_Item(_settingsService.GlobalSettings.OriginatorParamName);
        var roleParam = groupProject.Definitions.get_Item(_settingsService.GlobalSettings.RoleParamName);

        var categoryProjectInfo = App.RevitDocument.Settings.Categories.get_Item( BuiltInCategory.OST_ProjectInformation);

#if REVIT2024_OR_GREATER
        BindSharedParameter(projectIdentifierParam, categoryProjectInfo, GroupTypeId.IdentityData);
        BindSharedParameter(originatorParam, categoryProjectInfo, GroupTypeId.IdentityData);
        BindSharedParameter(roleParam, categoryProjectInfo, GroupTypeId.IdentityData);
#else
        BindSharedParameter(projectIdentifierParam, categoryProjectInfo, BuiltInParameterGroup.PG_IDENTITY_DATA);
        BindSharedParameter(originatorParam, categoryProjectInfo, BuiltInParameterGroup.PG_IDENTITY_DATA);
        BindSharedParameter(roleParam, categoryProjectInfo, BuiltInParameterGroup.PG_IDENTITY_DATA);
#endif


        //add sheet parameters
        var sheetVolumeParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetVolumeParamName);
        var sheetLevelParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetLevelParamName);
        var documentTypeParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.DocumentTypeParamName);
        var sheetStatusParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetStatusParamName);
        var sheetStatusDescriptionParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetStatusDescriptionParamName);

        var categorySheets = App.RevitDocument.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);
#if REVIT2024_OR_GREATER
        BindSharedParameter(sheetVolumeParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetLevelParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(documentTypeParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetStatusParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetStatusDescriptionParam, categorySheets, GroupTypeId.Title);
#else
        BindSharedParameter(sheetVolumeParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetLevelParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(documentTypeParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetStatusParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetStatusDescriptionParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
#endif

        //set shared parameters file back to original
        App.CachedUiApp.Application.SharedParametersFilename = currentSharedParametersFile;

        t.Commit();        
    }


    [RelayCommand]
    public void LoadSettingsFromDatabase()
    {
        if(DatabaseNotFound) return;

        _settingsService.GetSettings();

        SetPropertiesFromGlobalSettings();

        //we loaded settings from the database so we must want to record transmittals
        RecordTransmittals = true;
    }

    partial void OnDrawingIssueStoreChanged(string value)
    {
        SampleFolderName = DrawingIssueStore.ParseFolderName("FORMAT");
    }

    partial void OnDateFormatStringChanged(string value)
    {
        SampleDateString = DateTime.Now.ToString(value);
    }

    partial void OnRecordTransmittalsChanged(bool value)
    {
        //TODO check for the database and create if it doesn't exist
        CheckForDatabaseFile();
    }

    partial void OnDatabaseFileChanged(string value)
    {
        _settingsService.GlobalSettings.DatabaseFile = DatabaseFile;

        CheckForDatabaseFile();
    }

    public void CheckForDatabaseFile()
    {
        DatabaseNotFound = false;

        if (DatabaseFile != null)
        {
            if (RecordTransmittals)
            {
                if (!_settingsServiceRvt.CheckDatabaseFileExists(DatabaseFile.Trim(), false))
                {
                    DatabaseNotFound = true;
                }
            }
        }
    }

#if REVIT2024_OR_GREATER
    private void BindSharedParameter(Definition definition, Category category, ForgeTypeId parameterGroup)
    {
        var ca = App.RevitDocument.Application.Create;

        var categorySet = ca.NewCategorySet();
        categorySet.Insert(category);

        var binding = ca.NewInstanceBinding(categorySet) as Binding;

        App.RevitDocument.ParameterBindings.Insert(definition, binding, parameterGroup);
    }
#else
    private void BindSharedParameter(Definition definition, Category category, BuiltInParameterGroup parameterGroup)
    {
        var ca = App.RevitDocument.Application.Create;

        var categorySet = ca.NewCategorySet();
        categorySet.Insert(category);

        var binding = ca.NewInstanceBinding(categorySet) as Binding;

        App.RevitDocument.ParameterBindings.Insert(definition, binding, parameterGroup);
    }
#endif

    public void ParameterComplete(string variableName, string parameterGuid)
    {
        switch (variableName)
        {
            case nameof(SettingsModel.ProjectIdentifierParamGuid) :
                ProjectIdentifierParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.OriginatorParamGuid):
                OriginatorParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.RoleParamGuid):
                RoleParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.SheetVolumeParamGuid):
                SheetVolumeParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.SheetLevelParamGuid):
                SheetLevelParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.DocumentTypeParamGuid):
                DocumentTypeParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.SheetStatusParamGuid):
                SheetStatusParamGuid = parameterGuid;
                break;

            case nameof(SettingsModel.SheetStatusDescriptionParamGuid):
                SheetStatusDescriptionParamGuid = parameterGuid;
                break;

            default:
                break;
        }
    }
}

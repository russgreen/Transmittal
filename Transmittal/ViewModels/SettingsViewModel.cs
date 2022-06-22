using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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

namespace Transmittal.ViewModels;

//[INotifyPropertyChanged]
internal partial class SettingsViewModel : BaseViewModel
{
    public string WindowTitle { get; private set; }
    
    private readonly ISettingsServiceRvt _settingsServiceRvt = Ioc.Default.GetRequiredService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();

    public List<string> FolderNameParts => new List<string> { "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>", "<Format>", "%UserProfile%", "%OneDriveConsumer%", "%OneDriveCommercial%" };
    public List<string> FileNameParts => new List<string> { "<ProjNo>", "<ProjId>", "<Originator>", "<Volume>", "<Level>", "<Type>", "<Role>", "<ProjName>", "<SheetNo>", "<SheetName>", "<SheetName2>", "<Status>", "<StatusDescription>", "<Rev>", "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>" };
    
    public string ProjectNumber;
    public string Originator;
    public string Role;
        

    [ObservableProperty]
    private string _fileNameFilter;

    [ObservableProperty]
    private string _drawingIssueStore;

    [ObservableProperty]
    private string _sampleFolderName;

    [ObservableProperty]
    private string _dateFormatString;

    [ObservableProperty]
    private string _sampleDateString;

    [ObservableProperty]
    private bool _useISO19650;

    [ObservableProperty]
    private bool _useExtranet;

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
    private bool _databaseNotFound; //used to control visibility of error message in UI
    [ObservableProperty]
    private string _reportTemplatePath;
    
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

        //BASIC SETTINGS
        FileNameFilter = _settingsService.GlobalSettings.FileNameFilter;
        DrawingIssueStore = _settingsService.GlobalSettings.DrawingIssueStore;
        DateFormatString = _settingsService.GlobalSettings.DateFormatString;

        UseISO19650 = _settingsService.GlobalSettings.UseISO19650;
        UseExtranet = _settingsService.GlobalSettings.UseExtranet;

        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;

        //DATABASE SETTINGS
        RecordTransmittals = _settingsService.GlobalSettings.RecordTransmittals;
        DatabaseFile = _settingsService.GlobalSettings.DatabaseFile;
        DatabaseTemplateFile = _settingsService.GlobalSettings.DatabaseTemplateFile;
        ReportTemplatePath = _settingsService.GlobalSettings.ReportStore;

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

    [ICommand]
    private void SaveSettings()
    {
        _settingsService.GlobalSettings.FileNameFilter = FileNameFilter;
        _settingsService.GlobalSettings.DrawingIssueStore = DrawingIssueStore;
        _settingsService.GlobalSettings.DateFormatString = DateFormatString;

        _settingsService.GlobalSettings.UseISO19650 = UseISO19650;
        _settingsService.GlobalSettings.UseExtranet = UseExtranet;

        _settingsService.GlobalSettings.IssueFormats = IssueFormats;
        _settingsService.GlobalSettings.DocumentStatuses = DocumentStatuses;

        
        _settingsService.GlobalSettings.RecordTransmittals = RecordTransmittals;
        _settingsService.GlobalSettings.DatabaseFile = DatabaseFile;
        _settingsService.GlobalSettings.DatabaseTemplateFile = DatabaseTemplateFile;
        _settingsService.GlobalSettings.ReportStore = ReportTemplatePath;

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

        if (_recordTransmittals == true)
        {
            // we have a database file so save a copy of settings to the database for use by desktop.exe
            _settingsService.UpdateSettings();
        }

        this.OnClosingRequest();
    }

    [ICommand]
    private void AppendToFileNameFilter(string filter)
    {
        if (filter != null && !_fileNameFilter.Contains(filter))
        {
            FileNameFilter += $"{filter}";
        }
    }

    [ICommand]
    private void AppendToFolderPath(string filter)
    {   
        if(filter == "%UserProfile%")
        {
            try
            {
                var tempPath = DrawingIssueStore;
                tempPath = tempPath.Replace("<", " ");
                tempPath = tempPath.Replace(">", " ");
                var dirInfo = new System.IO.DirectoryInfo(tempPath);

                //this has to go at the start of the string
                DrawingIssueStore = DrawingIssueStore.Replace(dirInfo.Root.ToString(), filter);
            }
            catch 
            { 
            }
            
            return;

        }
        if (filter != null && !_drawingIssueStore.Contains(filter))
        {
            DrawingIssueStore += $"{filter}";
        }
    }

    [ICommand]
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

        var categoryProjectInfo = App.RevitDocument.Settings.Categories.get_Item(BuiltInCategory.OST_ProjectInformation);
        BindSharedParameter(projectIdentifierParam, categoryProjectInfo, BuiltInParameterGroup.PG_IDENTITY_DATA);
        BindSharedParameter(originatorParam, categoryProjectInfo, BuiltInParameterGroup.PG_IDENTITY_DATA);
        BindSharedParameter(roleParam, categoryProjectInfo, BuiltInParameterGroup.PG_IDENTITY_DATA);

        //add sheet parameters
        var sheetVolumeParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetVolumeParamName);
        var sheetLevelParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetLevelParamName);
        var documentTypeParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.DocumentTypeParamName);
        var sheetStatusParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetStatusParamName);
        var sheetStatusDescriptionParam = groupSheets.Definitions.get_Item(_settingsService.GlobalSettings.SheetStatusDescriptionParamName);

        var categorySheets = App.RevitDocument.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);
        BindSharedParameter(sheetVolumeParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetLevelParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(documentTypeParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetStatusParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetStatusDescriptionParam, categorySheets, BuiltInParameterGroup.PG_TITLE);

        //set shared parameters file back to original
        App.CachedUiApp.Application.SharedParametersFilename = currentSharedParametersFile;

        t.Commit();        
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
        CheckForDatabaseFile();
    }

    private void CheckForDatabaseFile()
    {
        DatabaseNotFound = false;
        
        if(DatabaseFile != null)
        {
            if (_recordTransmittals)
            {
                if(!_settingsServiceRvt.CheckDatabaseFileExists(DatabaseFile, false))
                {
                    DatabaseNotFound = true;
                }
            }
        }
    }

    private void BindSharedParameter(Definition definition, Category category, BuiltInParameterGroup parameterGroup)
    {
        var ca = App.RevitDocument.Application.Create;

        var categorySet = ca.NewCategorySet();
        categorySet.Insert(category);

        var binding = ca.NewInstanceBinding(categorySet) as Binding;

        App.RevitDocument.ParameterBindings.Insert(definition, binding, parameterGroup);
    }

}

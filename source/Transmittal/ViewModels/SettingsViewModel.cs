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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Transmittal.Library.Messages;
using System.Windows;
using Transmittal.Library.DataAccess;

namespace Transmittal.ViewModels;

internal partial class SettingsViewModel : BaseViewModel, IParameterGuidRequester
{
    public string WindowTitle { get; private set; }

    private readonly ISettingsServiceRvt _settingsServiceRvt = Host.GetService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();
    private readonly ILogger<SettingsViewModel> _logger = Host.GetService<ILogger<SettingsViewModel>>();
    private readonly IDataConnection _dataConnection = Host.GetService<IDataConnection>();

    public List<string> FolderNameParts => new() { "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>", "<Format>", "<Package>", "<SheetCollection>", "%UserProfile%", "%OneDriveConsumer%", "%OneDriveCommercial%" };
    public List<string> FileNameParts => ["<ProjNo>", "<ProjId>", "<Originator>", "<Volume>", "<Level>", "<Type>", "<Role>", "<ProjName>", "<SheetNo>", "<SheetName>", "<SheetName2>", "<Status>", "<StatusDescription>", "<Rev>", "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>"];
    
    public string ProjectNumber;
    public string Originator;
    public string Role;

    public bool HasAnyErrors => GetAnyErrors();

    [ObservableProperty]
    private string _displayMessage = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _fileNameFilter;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _drawingIssueStore;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _drawingIssueStore2;

    [ObservableProperty]
    private string _sampleFolderName;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _dateFormatString;

    [ObservableProperty]
    private string _sampleDateString;

    [ObservableProperty]
    private bool _useISO19650;

    [ObservableProperty]
    private bool _useCDE;

    [ObservableProperty]
    private bool _useDrawingIssueStore2;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MustBeFalse(ErrorMessage = "CDE output folder is not found")]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private bool _drawingIssueStore2NotFound;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _fileNameFilter2;

    [ObservableProperty]
    private ObservableCollection<IssueFormatModel> _issueFormats;

    [ObservableProperty]
    private ObservableCollection<DocumentStatusModel> _documentStatuses;

    [ObservableProperty]
    private bool _recordTransmittals;

    [ObservableProperty]
    private string _databaseFile;

    [ObservableProperty]
    private string _databaseTemplateFile;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MustBeFalse(ErrorMessage = "Database file is not found")]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
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
    [NotifyDataErrorInfo]
    [Required]
    private string _projectIdentifierParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _originatorParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _roleParamGuid;


    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _sheetVolumeParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _sheetLevelParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _documentTypeParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _sheetStatusParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _sheetStatusDescriptionParamGuid;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _sheetPackageParamGuid;

    public SettingsViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);

        SetPropertiesFromGlobalSettings();

        CheckForDatabaseFile();

        CheckForOutputFolders();

        IssueFormats.CollectionChanged += IssueFormats_CollectionChanged;
        DocumentStatuses.CollectionChanged += DocumentStatuses_CollectionChanged;

        ProjectNumber = _settingsService.GlobalSettings.ProjectNumber;
        Originator = _settingsService.GlobalSettings.Originator;
        Role = _settingsService.GlobalSettings.Role;

        WeakReferenceMessenger.Default.Register<ImportSettingsMessage>(this, (r, m) =>
        {
            SetPropertiesFromImportedSettings(m.Value);
        });

        WeakReferenceMessenger.Default.Register<LockFileMessage>(this, (r, m) =>
        {
            ProcessLockFileMessage(m.Value);
        });
    }

    private bool GetAnyErrors()
    {
        if(GetErrors().Any() == true)
        {
            return true;
        }

        if (IssueFormats.Any(x => x.HasErrors == true))
        {
            return true;
        }

        if (DocumentStatuses.Any(x => x.HasErrors == true))
        {
            return true;
        }

        if(IssueFormats.Count == 0)
        {
            return true;
        }

        if (DocumentStatuses.Count == 0)
        {
            return true;
        }

        return false;
    }

    partial void OnIssueFormatsChanged(ObservableCollection<IssueFormatModel> value)
    {
        foreach (var item in value)
        {
            item.PropertyChanged += IssueFormat_PropertyChanged;
            item.ErrorsChanged += (s, args) => { this.OnPropertyChanged(nameof(this.HasAnyErrors)); };
        }
    }

    partial void OnDocumentStatusesChanged(ObservableCollection<DocumentStatusModel> value)
    {
        foreach (var item in value)
        {
            item.PropertyChanged += DocumentStatus_PropertyChanged;
            item.ErrorsChanged += (s, args) => { this.OnPropertyChanged(nameof(this.HasAnyErrors)); };
        }
    }

    private void IssueFormats_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _logger.LogDebug("IssueFormats changed");

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (IssueFormatModel item in e.NewItems)
            {
                item.PropertyChanged += IssueFormat_PropertyChanged;
                item.ErrorsChanged += (s, args) => { this.OnPropertyChanged(nameof(this.HasAnyErrors)); };
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (IssueFormatModel item in e.OldItems)
            {
                item.PropertyChanged -= IssueFormat_PropertyChanged;
                item.ErrorsChanged -= (s, args) => { this.OnPropertyChanged(nameof(this.HasAnyErrors)); };
            }
        }

        this.OnPropertyChanged(nameof(this.HasAnyErrors));
    }

    private void DocumentStatuses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _logger.LogDebug("DocumentStatuses changed");

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (DocumentStatusModel item in e.NewItems)
            {
                item.PropertyChanged += DocumentStatus_PropertyChanged;
                item.ErrorsChanged += (s, args) => { this.OnPropertyChanged(nameof(this.HasAnyErrors)); };
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (DocumentStatusModel item in e.OldItems)
            {
                item.PropertyChanged -= DocumentStatus_PropertyChanged;
                item.ErrorsChanged -= (s, args) => { this.OnPropertyChanged(nameof(this.HasAnyErrors)); };
            }
        }

        this.OnPropertyChanged(nameof(this.HasAnyErrors));
    }

    private void IssueFormat_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("IssueFormat property changed");

        var values = new HashSet<string>();
        var duplicates = new HashSet<string>();

        foreach (var item in IssueFormats)
        {
            var itemValue = item.GetType()
                .GetProperty(nameof(IssueFormatModel.Code))
                .GetValue(item, null).ToString();

            if (!values.Add(itemValue))
            {
                duplicates.Add(itemValue);
            }
        }

        if (duplicates.Count > 0)
        {
            _logger.LogDebug("IssueFormats contain duplicates");
            var model = (IssueFormatModel)sender;
            model.Code = string.Empty; // (nameof(IssueFormatModel.Code), "Issue formats contain duplicate codes");
        }
    }

    private void DocumentStatus_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        _logger.LogDebug("DocumentStatus property changed");

        var values = new HashSet<string>();
        var duplicates = new HashSet<string>();

        foreach (var item in DocumentStatuses)
        {
            var itemValue = item.GetType()
                .GetProperty(nameof(DocumentStatusModel.Code))
                .GetValue(item, null).ToString();

            if (!values.Add(itemValue))
            {
                duplicates.Add(itemValue);
            }
        }

        if (duplicates.Count > 0)
        {
            _logger.LogDebug("DocumentStatuses contain duplicates");
            var model = (DocumentStatusModel)sender;
            model.Code = string.Empty; // (nameof(IssueFormatModel.Code), "Issue formats contain duplicate codes");
        }
    }

    private void SetPropertiesFromGlobalSettings()
    {
        //BASIC SETTINGS
        FileNameFilter = _settingsService.GlobalSettings.FileNameFilter;
        DrawingIssueStore = _settingsService.GlobalSettings.DrawingIssueStore;
        DateFormatString = _settingsService.GlobalSettings.DateFormatString;

        UseISO19650 = _settingsService.GlobalSettings.UseISO19650;
        UseCDE = _settingsService.GlobalSettings.UseExtranet;
        FileNameFilter2 = _settingsService.GlobalSettings.FileNameFilter2;
        UseDrawingIssueStore2 = _settingsService.GlobalSettings.UseDrawingIssueStore2;
        DrawingIssueStore2 = _settingsService.GlobalSettings.DrawingIssueStore2;

        IssueFormats = new ObservableCollection<IssueFormatModel>(_settingsService.GlobalSettings.IssueFormats);
        DocumentStatuses = new ObservableCollection<DocumentStatusModel>(_settingsService.GlobalSettings.DocumentStatuses);

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
        SheetPackageParamGuid = _settingsService.GlobalSettings.SheetPackageParamGuid;
    }

    private void SetPropertiesFromImportedSettings(ImportSettingsModel settings)
    {
        //PROJECT SETTINGS
        ProjectNumber = _settingsService.GlobalSettings.ProjectNumber;
        Originator = _settingsService.GlobalSettings.Originator;
        Role = _settingsService.GlobalSettings.Role;

        //BASIC SETTINGS
        FileNameFilter = settings.FileNameFilter;
        DrawingIssueStore = settings.DrawingIssueStore;
        DateFormatString = settings.DateFormatString;

        UseISO19650 = settings.UseISO19650;
        UseCDE = settings.UseExtranet;
        FileNameFilter2 = settings.FileNameFilter2;
        UseDrawingIssueStore2 = settings.UseDrawingIssueStore2;
        DrawingIssueStore2 = settings.DrawingIssueStore2;

        IssueFormats = new ObservableCollection<IssueFormatModel>(settings.IssueFormats);
        DocumentStatuses = new ObservableCollection<DocumentStatusModel>(settings.DocumentStatuses);

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
        SheetPackageParamGuid = settings.SheetPackageParamGuid;

        //TODO check if all the parameters exist in the project or load them from the shared parameters file.
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _settingsService.GlobalSettings.FileNameFilter = FileNameFilter?.Trim();
        _settingsService.GlobalSettings.DrawingIssueStore = DrawingIssueStore?.Trim();
        _settingsService.GlobalSettings.DateFormatString = DateFormatString?.Trim();

        _settingsService.GlobalSettings.UseISO19650 = UseISO19650;
        _settingsService.GlobalSettings.UseExtranet = UseCDE;
        _settingsService.GlobalSettings.FileNameFilter2 = FileNameFilter2?.Trim();

        _settingsService.GlobalSettings.UseDrawingIssueStore2 = UseDrawingIssueStore2;
        _settingsService.GlobalSettings.DrawingIssueStore2 = DrawingIssueStore2?.Trim();

        _settingsService.GlobalSettings.IssueFormats = IssueFormats.ToList();
        _settingsService.GlobalSettings.DocumentStatuses = DocumentStatuses.ToList();

        _settingsService.GlobalSettings.UseRevit = true;
        
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
        _settingsService.GlobalSettings.SheetPackageParamGuid = SheetPackageParamGuid;

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
            if (filter.StartsWith("%") & DrawingIssueStore.StartsWith("%"))
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
    private void AppendToFolderPath2(string filter)
    {   
        if (filter != null && !DrawingIssueStore2.Contains(filter))
        {
            if (filter.StartsWith("%") & DrawingIssueStore2.StartsWith("%"))
            {
                return;
            }

            if(filter.StartsWith("%")) 
            {
                DrawingIssueStore2 = filter += DrawingIssueStore2;
                return;
            }

            DrawingIssueStore2 += $"{filter}";
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
            App.DesktopAssemblyFolder)}\Resources\TransmittalParameters.txt";

        var sharedParameterDefinitionFile = App.CachedUiApp.Application.OpenSharedParameterFile();

        //load shared parameters
        var groupProject = sharedParameterDefinitionFile.Groups.get_Item("Project Parameters");
        var groupSheets = sharedParameterDefinitionFile.Groups.get_Item("Sheet Information");

        //add project parameters
        var projectIdentifierParam = groupProject.Definitions.get_Item("ProjectIdentifier");
        var originatorParam = groupProject.Definitions.get_Item("Originator");
        var roleParam = groupProject.Definitions.get_Item("Role");

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
        var sheetVolumeParam = groupSheets.Definitions.get_Item("SheetVolume");
        var sheetLevelParam = groupSheets.Definitions.get_Item("SheetLevel");
        var documentTypeParam = groupSheets.Definitions.get_Item("DocumentType");
        var sheetStatusParam = groupSheets.Definitions.get_Item("SheetStatus");
        var sheetStatusDescriptionParam = groupSheets.Definitions.get_Item("SheetStatusDescription");
        var sheetPackageParam = groupSheets.Definitions.get_Item("SheetPackage");

        var categorySheets = App.RevitDocument.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);
#if REVIT2024_OR_GREATER
        BindSharedParameter(sheetVolumeParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetLevelParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(documentTypeParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetStatusParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetStatusDescriptionParam, categorySheets, GroupTypeId.Title);
        BindSharedParameter(sheetPackageParam, categorySheets, GroupTypeId.Title);
#else
        BindSharedParameter(sheetVolumeParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetLevelParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(documentTypeParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetStatusParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetStatusDescriptionParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
        BindSharedParameter(sheetPackageParam, categorySheets, BuiltInParameterGroup.PG_TITLE);
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

    partial void OnUseCDEChanged(bool oldValue, bool newValue)
    {
        if(newValue == false)
        {
            UseDrawingIssueStore2 = false;
        }
    }

    partial void OnDrawingIssueStoreChanged(string value)
    {
        SampleFolderName = DrawingIssueStore.ParseFolderName("FORMAT", "PACKAGE", "COLLECTION");
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
    partial void OnUseDrawingIssueStore2Changed(bool oldValue, bool newValue)
    {
        CheckForOutputFolders();
    }

    partial void OnDrawingIssueStore2Changed(string value)
    {
        CheckForOutputFolders();
    }

    partial void OnDatabaseFileChanged(string value)
    {
        _settingsService.GlobalSettings.DatabaseFile = DatabaseFile;

        CheckForDatabaseFile();
    }

    private void ProcessLockFileMessage(string value)
    {
        if (value == "")
        {
            DisplayMessage = "";
            return;
        }

        //so we have a lock file
        DisplayMessage = $"Waiting for database .lock file to clear. Check if .lock needs to be manually deleted.";

        DispatcherHelper.DoEvents();
      
    }

    public void CheckForOutputFolders()
    {
        DrawingIssueStore2NotFound = false;

        if(DrawingIssueStore2 != null)
        {
            if (UseDrawingIssueStore2)
            {
                //first we need the folder path with up the the first <field> only.
                var path = DrawingIssueStore2.ParseFolderName();

                if (!System.IO.Directory.Exists(path))
                {
                    DrawingIssueStore2NotFound = true;
                }
            }
        }


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

    public void UpgradeDatabase()
    {
        if (DatabaseNotFound) return;

        if (_settingsServiceRvt.CheckDatabaseFileExists(DatabaseFile.Trim(), true))
        {
            _dataConnection.UpgradeDatabase(DatabaseFile.Trim());
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

            case nameof(SettingsModel.SheetPackageParamGuid):
                SheetPackageParamGuid = parameterGuid;
                break;

            default:
                break;
        }
    }
}

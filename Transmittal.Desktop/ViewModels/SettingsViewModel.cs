using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Library.Services;
using Transmittal.Library.Models;
using Transmittal.Library.ViewModels;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using Transmittal.Library.Extensions;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;

namespace Transmittal.Desktop.ViewModels;
internal partial class SettingsViewModel : BaseViewModel
{
    public string WindowTitle { get; private set; }

    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();

    public List<string> FolderNameParts => new List<string> { "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>", "<Format>", "%UserProfile%", "%OneDriveConsumer%", "%OneDriveCommercial%" };
    public List<string> FileNameParts => new List<string> { "<ProjNo>", "<ProjId>", "<Originator>", "<Volume>", "<Level>", "<Type>", "<Role>", "<ProjName>", "<SheetNo>", "<SheetName>", "<SheetName2>", "<Status>", "<StatusDescription>", "<Rev>", "<DateYY>", "<DateYYYY>", "<DateMM>", "<DateDD>" };

    public bool HasAnyErrors => GetAnyErrors();

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _projectNumber;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _projectName;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _clientName;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _originator;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [NotifyPropertyChangedFor(nameof(HasAnyErrors))]
    private string _role;

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
    private ObservableCollection<IssueFormatModel> _issueFormats;

    [ObservableProperty]
    private ObservableCollection<DocumentStatusModel> _documentStatuses;

    [ObservableProperty]
    private string _reportTemplatePath;

    [ObservableProperty]
    private string _issueSheetStorePath;

    [ObservableProperty]
    private string _directoryStorePath;

    public SettingsViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({_settingsService.GlobalSettings.DatabaseFile})";

        SetPropertiesFromGlobalSettings();

        IssueFormats.CollectionChanged += IssueFormats_CollectionChanged;
        DocumentStatuses.CollectionChanged += DocumentStatuses_CollectionChanged;

    }

    private bool GetAnyErrors()
    {
        if (GetErrors().Any() == true)
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

        if (IssueFormats.Count == 0)
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

    private void SetPropertiesFromGlobalSettings()
    {
        //PROJECT SETTINGS
        ProjectNumber = _settingsService.GlobalSettings.ProjectNumber;
        ProjectName = _settingsService.GlobalSettings.ProjectName;
        ClientName = _settingsService.GlobalSettings.ClientName;
        Originator = _settingsService.GlobalSettings.Originator;
        Role = _settingsService.GlobalSettings.Role;

        //BASIC SETTINGS
        FileNameFilter = _settingsService.GlobalSettings.FileNameFilter;
        DrawingIssueStore = _settingsService.GlobalSettings.DrawingIssueStore;
        DateFormatString = _settingsService.GlobalSettings.DateFormatString;

        UseISO19650 = _settingsService.GlobalSettings.UseISO19650;

        IssueFormats = new ObservableCollection<IssueFormatModel>(_settingsService.GlobalSettings.IssueFormats);
        DocumentStatuses = new ObservableCollection<DocumentStatusModel>(_settingsService.GlobalSettings.DocumentStatuses);

        //DATABASE SETTINGS
        ReportTemplatePath = _settingsService.GlobalSettings.ReportStore;
        IssueSheetStorePath = _settingsService.GlobalSettings.IssueSheetStore;
        DirectoryStorePath = _settingsService.GlobalSettings.DirectoryStore;
    }

    private void IssueFormats_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine("IssueFormats changed");

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
        Debug.WriteLine("DocumentStatuses changed");

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
        Debug.WriteLine("IssueFormat property changed");

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
            Debug.WriteLine("IssueFormats contain duplicates");
            var model = (IssueFormatModel)sender;
            model.Code = string.Empty; // (nameof(IssueFormatModel.Code), "Issue formats contain duplicate codes");
        }
    }

    private void DocumentStatus_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Debug.WriteLine("DocumentStatus property changed");

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
            Debug.WriteLine("DocumentStatuses contain duplicates");
            var model = (DocumentStatusModel)sender;
            model.Code = string.Empty; // (nameof(IssueFormatModel.Code), "Issue formats contain duplicate codes");
        }
    }


    [RelayCommand]
    private void SaveSettings()
    {
        _settingsService.GlobalSettings.ProjectNumber = ProjectNumber?.Trim();
        _settingsService.GlobalSettings.ProjectName = ProjectName?.Trim();
        _settingsService.GlobalSettings.ClientName = ClientName?.Trim();
        _settingsService.GlobalSettings.Originator = Originator?.Trim();
        _settingsService.GlobalSettings.Role = Role?.Trim();

        _settingsService.GlobalSettings.FileNameFilter = FileNameFilter?.Trim();
        _settingsService.GlobalSettings.DrawingIssueStore = DrawingIssueStore?.Trim();
        _settingsService.GlobalSettings.DateFormatString = DateFormatString?.Trim();

        _settingsService.GlobalSettings.UseISO19650 = UseISO19650;

        _settingsService.GlobalSettings.IssueFormats = IssueFormats.ToList();
        _settingsService.GlobalSettings.DocumentStatuses = DocumentStatuses.ToList();

        _settingsService.GlobalSettings.ReportStore = ReportTemplatePath?.Trim();
        _settingsService.GlobalSettings.IssueSheetStore = IssueSheetStorePath?.Trim();
        _settingsService.GlobalSettings.DirectoryStore = DirectoryStorePath?.Trim();

        _settingsService.UpdateSettings();

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
    private void AppendToFolderPath(string filter)
    {
        if (filter != null && !DrawingIssueStore.Contains(filter))
        {
            if (DrawingIssueStore.StartsWith("%"))
            {
                return;
            }

            if (filter.StartsWith("%"))
            {
                DrawingIssueStore = filter += DrawingIssueStore;
                return;
            }

            DrawingIssueStore += $"{filter}";
        }
    }

    partial void OnDrawingIssueStoreChanged(string value)
    {
        SampleFolderName = DrawingIssueStore.ParseFolderName("FORMAT");
    }

    partial void OnDateFormatStringChanged(string value)
    {
        SampleDateString = DateTime.Now.ToString(value);
    }
}

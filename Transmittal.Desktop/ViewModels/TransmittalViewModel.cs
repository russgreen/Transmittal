using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Transmittal.Desktop.Requesters;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;
internal partial class TransmittalViewModel : BaseViewModel, IPersonRequester, IPackageRequester
{
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();
    private readonly IContactDirectoryService _contactDirectoryService = Host.GetService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Host.GetService<ITransmittalService>();
    private readonly ILogger<TransmittalViewModel> _logger = Host.GetService<ILogger<TransmittalViewModel>>();

    public string WindowTitle { get; private set; }

    private TransmittalModel _newTransmittal = new TransmittalModel();
    [ObservableProperty]
    private ObservableCollection<DocumentModel> _documents = new ObservableCollection<DocumentModel>();

    [ObservableProperty]
    private ObservableCollection<string> _packages;

    public List<DocumentTypeModel> DocumentTypes { get; private set; }
    public List<DocumentStatusModel> DocumentStatuses { get; private set; }
    public List<IssueFormatModel> IssueFormats { get; private set; }
    public bool IsDistributionValid => ValidateDistribution();

    [ObservableProperty]
    private bool _hasDocuments;

    [ObservableProperty]
    private int _copies = 1;
    [ObservableProperty]
    private IssueFormatModel _issueFormat;
    [ObservableProperty]
    private ObservableCollection<ProjectDirectoryModel> _projectDirectory;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDistributionValid))]
    private ObservableCollection<TransmittalDistributionModel> _distribution;
    [ObservableProperty]
    private ObservableCollection<object> _selectedProjectDirectory;
    [ObservableProperty]
    private ObservableCollection<object> _selectedDistribution;
    [ObservableProperty]
    private bool _hasDirectoryEntriesSelected = false;
    [ObservableProperty]
    private bool _hasDistributionEntriesSelected = false;
    [ObservableProperty]
    private bool _zipDocuments = true;

    [ObservableProperty]
    private bool _isBackEnabled = true;

    public TransmittalViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({_settingsService.GlobalSettings.DatabaseFile})";

        _settingsService.GetSettings();

        LoadPackages();

        WireUpDocumentsPage();

        WireUpDistributionPage();
    }

    private void LoadPackages()
    {
        Packages = new ObservableCollection<string>(_transmittalService.GetPackages());
    }

    private void WireUpDocumentsPage()
    {
        DocumentTypes = Library.Helpers.ISO19650.GetDocumentTypes();
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;
        Documents.CollectionChanged += Documents_CollectionChanged;
    }

    private void WireUpDistributionPage()
    {
        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        IssueFormat = IssueFormats.FirstOrDefault();

        if (_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            ProjectDirectory = new(_contactDirectoryService.GetProjectDirectory()
                .OrderBy(x => x.Company.CompanyName)
                .ThenBy(x => x.Person.FullNameReversed));

            ProjectDirectory.CollectionChanged += ProjectDirectory_CollectionChanged;

            SelectedProjectDirectory = new();
            SelectedProjectDirectory.CollectionChanged += SelectedProjectDirectory_CollectionChanged;

            Distribution = new();
            Distribution.CollectionChanged += Distribution_CollectionChanged;

            SelectedDistribution = new();
            SelectedDistribution.CollectionChanged += SelectedDistribution_CollectionChanged;
        }
    }

    #region Distribution

    public void PersonComplete(PersonModel model)
    {
        _contactDirectoryService.CreatePerson(model);

        ProjectDirectoryModel projectDirectoryModel = new()
        {
            Person = model,
            Company = _contactDirectoryService.GetCompany(model.CompanyID)
        };

        ProjectDirectory.Add(projectDirectoryModel);
    }

    private bool ValidateDistribution()
    {
        if (Distribution is null || Distribution.Count == 0)
        {
            return false;
        }

        return true;
    }

    private void Documents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDocuments = Documents.Count > 0;
    }

    private void ProjectDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {

    }

    private void Distribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsDistributionValid));
    }

    private void SelectedProjectDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDirectoryEntriesSelected = true;

        if (SelectedProjectDirectory == null || SelectedProjectDirectory.Count == 0)
        {
            HasDirectoryEntriesSelected = false;
        }
    }

    private void SelectedDistribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDistributionEntriesSelected = true;

        if (SelectedDistribution == null || SelectedDistribution.Count == 0)
        {
            HasDistributionEntriesSelected = false;
        }
    }

    [RelayCommand]
    private void AddToDistribition()
    {
        foreach (ProjectDirectoryModel directoryContact in SelectedProjectDirectory.Cast<ProjectDirectoryModel>().ToList())
        {
            if (directoryContact != null)
            {
                TransmittalDistributionModel distributionRecord = new()
                {

                    Company = directoryContact.Company,
                    Person = directoryContact.Person,
                    PersonID = directoryContact.Person.ID,
                    TransCopies = Copies,
                    TransFormat = IssueFormat.Code
                };

                ProjectDirectory.Remove(directoryContact);
                Distribution.Add(distributionRecord);
            }
        }
    }

    [RelayCommand]
    private void RemoveFromDistribution()
    {
        foreach (TransmittalDistributionModel distributionRecord in SelectedDistribution.Cast<TransmittalDistributionModel>().ToList())
        {
            if (distributionRecord != null)
            {
                ProjectDirectoryModel directoryContact = new()
                {
                    Company = distributionRecord.Company,
                    Person = distributionRecord.Person
                };

                Distribution.Remove(distributionRecord);
                ProjectDirectory.Add(directoryContact);
            }
        }
    }


    #endregion

    [RelayCommand]
    private void ProcessDocuments()
    {
        IsBackEnabled = false;

        try
        {
            if (ZipDocuments == true)
            {
                ZipDocumentsPackages();
            }
            RecordTransmittalInDatabase();
            LaunchTransmittalReport();

            this.OnClosingRequest();
            return;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error processing transmittal");
            this.OnClosingRequest();
            return;
        }
    }

    private void ZipDocumentsPackages()
    {
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(string.Empty);
        if (System.IO.Directory.Exists(folderPath))
        {
            string zipFileName = Path.Combine(folderPath, $"{DateTime.Now.ToStringYYMMDD()}-{DateTime.Now.ToShortTimeString().Replace(":", "")}_DocumentTransmittal.zip");

            using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var document in Documents)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(document.FilePath, document.FileName, CompressionLevel.Optimal);
                }
            }
        }
    }

    internal void AddFileToDocumentsList(string file)
    {
        var projectIdentifier = string.Empty;

        //check if we're using the project identifier on this project
        if (_settingsService.GlobalSettings.ProjectIdentifier is null || _settingsService.GlobalSettings.ProjectIdentifier == string.Empty)
        {
            projectIdentifier = _settingsService.GlobalSettings.ProjectNumber;
        }
        else
        {
            projectIdentifier = _settingsService.GlobalSettings.ProjectIdentifier;
        }

        //var documentModel = Util.ISO19650Parser.DocumentModel(file, projectIdentifier,
        //    _settingsService.GlobalSettings.Originator,
        //    _settingsService.GlobalSettings.Role);

        var documentModel = Library.Helpers.FilenameParser.GetDocumentModel(file, projectIdentifier,
    _settingsService.GlobalSettings.Originator,
    _settingsService.GlobalSettings.Role,
    _settingsService.GlobalSettings.FileNameFilter);

        documentModel.FilePath = file;

        if (!Documents.Any(x => x.FileName == documentModel.FileName))
        {
            Documents.Add(documentModel);
        }
    }

    private void RecordTransmittalInDatabase()
    {
        _newTransmittal.TransDate = DateTime.Now;
        _transmittalService.CreateTransmittal(_newTransmittal);

        foreach (TransmittalItemModel item in Documents)
        {
            item.TransID = _newTransmittal.ID;

            //check if we're using the project identifier on this project
            if (_settingsService.GlobalSettings.ProjectIdentifier is null || _settingsService.GlobalSettings.ProjectIdentifier == string.Empty)
            {
                item.DrgProj = _settingsService.GlobalSettings.ProjectNumber;
            }
            else
            {
                item.DrgProj = _settingsService.GlobalSettings.ProjectIdentifier;
            }

            item.DrgOriginator = _settingsService.GlobalSettings.Originator;
            item.DrgRole = _settingsService.GlobalSettings.Role;
            _transmittalService.CreateTransmittalItem(item);
        }

        foreach (TransmittalDistributionModel dist in Distribution)
        {
            dist.TransID = _newTransmittal.ID;
            _transmittalService.CreateTransmittalDist(dist);
        }
    }

    private void LaunchTransmittalReport()
    {
        Reports.Reports report = new(Host.GetService<ISettingsService>(),
     Host.GetService<IContactDirectoryService>(),
     Host.GetService<ITransmittalService>());

        report.ShowTransmittalReport(_newTransmittal.ID);
    }

    public void PackageComplete(string packageName)
    {
        Packages.Add(packageName);
    }
}

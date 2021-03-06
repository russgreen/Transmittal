using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using Transmittal.Desktop.Requesters;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;
internal partial class TransmittalViewModel : BaseViewModel, IPersonRequester
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
    private readonly IContactDirectoryService _contactDirectoryService = Ioc.Default.GetRequiredService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Ioc.Default.GetRequiredService<ITransmittalService>();

    public string WindowTitle { get; private set; }

    private TransmittalModel _newTransmittal = new TransmittalModel();
    [ObservableProperty]
    private ObservableCollection<DocumentModel> _documents = new ObservableCollection<DocumentModel>();

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
    private bool _isBackEnabled = true;

    public TransmittalViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({_settingsService.GlobalSettings.DatabaseFile})";

        _settingsService.GetSettings();

        WireUpDocumentsPage();

        WireUpDistributionPage();
    }

    private void WireUpDocumentsPage()
    {
        DocumentTypes = Util.ISO19650Parser.GetDocumentTypes();
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;
        Documents.CollectionChanged += Documents_CollectionChanged;
    }

    private void WireUpDistributionPage()
    {
        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        IssueFormat = IssueFormats.FirstOrDefault();

        if (_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            ProjectDirectory = new(_contactDirectoryService.GetProjectDirectory());
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
        if (_distribution is null || _distribution.Count == 0)
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

        if (_selectedProjectDirectory == null || _selectedProjectDirectory.Count == 0)
        {
            HasDirectoryEntriesSelected = false;
        }
    }

    private void SelectedDistribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDistributionEntriesSelected = true;

        if (_selectedDistribution == null || _selectedDistribution.Count == 0)
        {
            HasDistributionEntriesSelected = false;
        }
    }

    [RelayCommand]
    private void AddToDistribition()
    {
        foreach (ProjectDirectoryModel directoryContact in _selectedProjectDirectory.Cast<ProjectDirectoryModel>().ToList())
        {
            if (directoryContact != null)
            {
                TransmittalDistributionModel distributionRecord = new()
                {

                    Company = directoryContact.Company,
                    Person = directoryContact.Person,
                    PersonID = directoryContact.Person.ID,
                    TransCopies = _copies,
                    TransFormat = _issueFormat.Code
                };

                _projectDirectory.Remove(directoryContact);
                _distribution.Add(distributionRecord);
            }
        }
    }

    [RelayCommand]
    private void RemoveFromDistribution()
    {
        foreach (TransmittalDistributionModel distributionRecord in _selectedDistribution.Cast<TransmittalDistributionModel>().ToList())
        {
            if (distributionRecord != null)
            {
                ProjectDirectoryModel directoryContact = new()
                {
                    Company = distributionRecord.Company,
                    Person = distributionRecord.Person
                };

                _distribution.Remove(distributionRecord);
                _projectDirectory.Add(directoryContact);
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
            
            RecordTransmittalInDatabase();
            LaunchTransmittalReport();

            this.OnClosingRequest();
            return;
        }
        catch (Exception ex)
        {
             this.OnClosingRequest();
            return;
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

        var documentModel = Util.ISO19650Parser.DocumentModel(file, projectIdentifier,
            _settingsService.GlobalSettings.Originator,
            _settingsService.GlobalSettings.Role);

        if (!Documents.Any(x => x.FileName == documentModel.FileName))
        {
            Documents.Add(documentModel);
        }
    }

    private void RecordTransmittalInDatabase()
    {
        _newTransmittal.TransDate = DateTime.Now;
        _transmittalService.CreateTransmittal(_newTransmittal);

        foreach (TransmittalItemModel item in _documents)
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

        foreach (TransmittalDistributionModel dist in _distribution)
        {
            dist.TransID = _newTransmittal.ID;
            _transmittalService.CreateTransmittalDist(dist);
        }
    }

    private void LaunchTransmittalReport()
    {
        Reports.Reports report = new(Ioc.Default.GetService<ISettingsService>(),
                       Ioc.Default.GetService<IContactDirectoryService>(),
                       Ioc.Default.GetService<ITransmittalService>());

        report.ShowTransmittalReport(_newTransmittal.ID);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels;

internal partial class DirectoryViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService = Host.GetService<ISettingsService>();
    private readonly IContactDirectoryService _contactDirectoryService = Host.GetService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Host.GetService<ITransmittalService>();

    public string WindowTitle { get; private set; }

    private List<ProjectDirectoryModel> _projectDirectory;

    [ObservableProperty]
    private ObservableCollection<PersonModel> _people;
    [ObservableProperty]
    private PersonModel _selectedPerson;
    [ObservableProperty]
    private ObservableCollection<CompanyModel> _companies;
    [ObservableProperty]
    private CompanyModel _selectedCompany;

    [ObservableProperty]
    private bool _hasDatabase = true;
    [ObservableProperty]
    private bool _itemSelected = false;

    public DirectoryViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({_settingsService.GlobalSettings.DatabaseFile})";

        if (_settingsService.GlobalSettings.RecordTransmittals == false)
        {
            HasDatabase = false;
            return;
        }

        People = new ObservableCollection<PersonModel>(_contactDirectoryService.GetPeople_All());
        Companies = new ObservableCollection<CompanyModel>(_contactDirectoryService.GetCompanies_All());

        WireUpPeoplePropertyChangedEvents();
        WireUpCompaniesPropertyChangedEvents();
    }

    private void BuildProjectDirectory()
    {
        _projectDirectory = new();

        foreach (var person in People)
        {
            var company = new CompanyModel();

            if(person.CompanyID > 0)
            {
                company = Companies.ToList().Find(x => x.ID == person.CompanyID);
            }

            ProjectDirectoryModel projectDirectoryModel = new()
            {
                Person = person,
                Company = company
            };

            _projectDirectory.Add(projectDirectoryModel);
        }
    }

    private void WireUpPeoplePropertyChangedEvents()
    {
        People.CollectionChanged += People_CollectionChanged;

        foreach (var item in People)
        {
            item.PropertyChanged += Person_PropertyChanged;
        }
    }

    private void WireUpCompaniesPropertyChangedEvents()
    {
        Companies.CollectionChanged += Companies_CollectionChanged;

        foreach (var item in Companies)
        {
            item.PropertyChanged += Company_PropertyChanged;
        }
    }

    private void Company_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(SelectedCompany != null)
        {
            _contactDirectoryService.UpdateCompany(SelectedCompany);
        }
    }

    private void Person_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PersonModel.FullName) || e.PropertyName == nameof(PersonModel.FullNameReversed))
        {
            //we don't care if these property changes are fired as not saved seperately to the db
            return;
        }

        if (e.PropertyName == nameof(PersonModel.Archive))
        {
            SelectedPerson.ShowInReport = !SelectedPerson.Archive;
        }

        if(SelectedPerson != null)
        {
            _contactDirectoryService.UpdatePerson(SelectedPerson);
        }
    }

    partial void OnSelectedPersonChanged(PersonModel value)
    {
        if (value == null)
        {
            ItemSelected = false;
        }
        else
        {
            ItemSelected = true;
        }
    }

    private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.Action == NotifyCollectionChangedAction.Add)
        {
            PersonModel person = (PersonModel)e.NewItems[0];
            person.PropertyChanged += Person_PropertyChanged;

            _contactDirectoryService.CreatePerson(person);
        }
    }

    private void Companies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            CompanyModel company = (CompanyModel)e.NewItems[0];
            company.PropertyChanged += Company_PropertyChanged;

            _contactDirectoryService.CreateCompany(company);
        }
    }

    [RelayCommand]
    private void RemovePerson()
    {
        if (SelectedPerson != null)
        {
            if (_transmittalService.GetTransmittals_ByPerson(SelectedPerson.ID).Count == 0)
            {
                _contactDirectoryService.DeletePerson(SelectedPerson);
            }
        }
    }

    [RelayCommand]
    private void RemoveCompany()
    {
        if (SelectedCompany != null)
        {
            if (_contactDirectoryService.GetPeople_ByCompany(SelectedCompany.ID).Count == 0)
            {
                _contactDirectoryService.DeleteCompany(SelectedCompany);
            }
        }
    }

    [RelayCommand]
    private void ExportVCard()
    {
        ProjectDirectoryModel projectDirectoryModel = new()
        {
            Person = SelectedPerson,
            Company = Companies.ToList().Find(x => x.ID == SelectedPerson.CompanyID)
        };

        Library.Helpers.VCardHelper.ExportVCard(projectDirectoryModel);
    }

    [RelayCommand]
    private void ShowDirectoryReport()
    {
        BuildProjectDirectory();

        Reports.Reports reports = new(_settingsService, 
            _contactDirectoryService, 
            _transmittalService);

        reports.ShowProjectDirectoryReport(_projectDirectory);
    }
}

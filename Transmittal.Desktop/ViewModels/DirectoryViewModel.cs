using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Library.Models;
using Transmittal.Library.ViewModels;
using Transmittal.Library.Services;
using System.Collections.Specialized;

namespace Transmittal.Desktop.ViewModels;

internal partial class DirectoryViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
    private readonly IContactDirectoryService _contactDirectoryService = Ioc.Default.GetRequiredService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Ioc.Default.GetRequiredService<ITransmittalService>();

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

        foreach (var person in _people)
        {
            var company = new CompanyModel();

            if(person.CompanyID > 0)
            {
                company = _companies.ToList().Find(x => x.ID == person.CompanyID);
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
        _people.CollectionChanged += People_CollectionChanged;

        foreach (var item in People)
        {
            item.PropertyChanged += Person_PropertyChanged;
        }
    }

    private void WireUpCompaniesPropertyChangedEvents()
    {
        _companies.CollectionChanged += Companies_CollectionChanged;

        foreach (var item in Companies)
        {
            item.PropertyChanged += Company_PropertyChanged;
        }
    }

    private void Company_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if(_selectedCompany != null)
        {
            _contactDirectoryService.UpdateCompany(_selectedCompany);
        }

    }

    private void Person_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PersonModel.FullName) || e.PropertyName == nameof(PersonModel.FullNameReversed))
        {
            //we don't care if these property changes are fired as not saved seperately to the db
            return;
        }

        if(_selectedPerson != null)
        {
            _contactDirectoryService.UpdatePerson(_selectedPerson);
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

    [ICommand]
    private void RemovePerson()
    {
        //TODO impement method

        //check if the person has any transmittals recorded against them

        //if person transmittals = 0 then

        //remove the person from the database

        //remoe the person from the project directory
    }

    [ICommand]
    private void ExportVCard()
    {
        ProjectDirectoryModel projectDirectoryModel = new()
        {
            Person = _selectedPerson,
            Company = _companies.ToList().Find(x => x.ID == _selectedPerson.CompanyID)
        };

        Library.Helpers.VCardHelper.ExportVard(projectDirectoryModel);
    }

    [ICommand]
    private void ShowDirectoryReport()
    {
        BuildProjectDirectory();

        Reports.Reports reports = new(_settingsService, _contactDirectoryService, _transmittalService);

        reports.ShowProjectDirectoryReport(_projectDirectory);
    }
}

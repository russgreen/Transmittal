using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

internal partial class NewPersonViewModel : BaseViewModel, ICompanyRequester
{
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly IPersonRequester _callingViewModel;

    [ObservableProperty]
    private PersonModel _person = new();

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2)]
    private string _lastName;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "At least provide an initial")]
    [MinLength(1)]
    private string _firstName;
    [ObservableProperty]
    [EmailAddress]
    private string _email;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a company for the person")]
    private int _companyID;

    [ObservableProperty]
    private ObservableCollection<CompanyModel> _companies;

    public NewPersonViewModel(IPersonRequester caller, 
        IContactDirectoryService contactDirectoryService)
    {  
        _callingViewModel = caller;
        _contactDirectoryService = contactDirectoryService;

        this.ValidateAllProperties();

        Companies = new ObservableCollection<CompanyModel>(_contactDirectoryService.GetCompanies_All());

        if (Companies.Any())
        {
            CompanyID = Companies.First().ID;
        }
    }

    public void CompanyComplete(CompanyModel model)
    {
        _contactDirectoryService.CreateCompany(model);
        Companies.Add(model);
    }

    [RelayCommand]
    private void SendPerson()
    {
        Person.FirstName = FirstName;
        Person.LastName = LastName;
        Person.Email = Email;
        Person.CompanyID = CompanyID;
        _callingViewModel.PersonComplete(Person);
        this.OnClosingRequest();
    }

}

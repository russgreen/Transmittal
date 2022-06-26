using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Desktop.Requesters;

namespace Transmittal.Desktop.ViewModels;

internal partial class NewPersonViewModel : BaseViewModel, ICompanyRequester
{
    private readonly IContactDirectoryService _contactDirectoryService = Ioc.Default.GetRequiredService<IContactDirectoryService>();
    private readonly IPersonRequester _callingViewModel;

    [ObservableProperty]
    private PersonModel _person = new();

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2)]
    private string _lastName;
    [ObservableProperty]
    [Required(ErrorMessage = "At least provide an intial")]
    [MinLength(1)]
    private string _firstName;
    [ObservableProperty]
    [EmailAddress]
    private string _email;
    [ObservableProperty]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a company for the person")]
    private int _companyID;

    [ObservableProperty]
    private ObservableCollection<CompanyModel> _companies;

    public NewPersonViewModel(IPersonRequester caller)
    {  
        _callingViewModel = caller;

        this.ValidateAllProperties();

        Companies = new ObservableCollection<CompanyModel>(_contactDirectoryService.GetCompanies_All());
    }

    public void CompanyComplete(CompanyModel model)
    {
        _contactDirectoryService.CreateCompany(model);
        Companies.Add(model);
    }

    [RelayCommand]
    private void SendPerson()
    {
        _person.FirstName = _firstName;
        _person.LastName = _lastName;
        _person.Email = _email;
        _person.CompanyID = _companyID;
        _callingViewModel.PersonComplete(_person);
        this.OnClosingRequest();
    }

}

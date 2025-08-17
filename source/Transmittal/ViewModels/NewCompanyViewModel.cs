using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.Validation;
using Transmittal.Library.ViewModels;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

public partial class NewCompanyViewModel : BaseViewModel
{
    private readonly ICompanyRequester _callingViewModel;
    private readonly IContactDirectoryService _contactDirectoryService;

    [ObservableProperty]
    private CompanyModel _company = new();

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ValidationHelpers), nameof(ValidationHelpers.ValidateCompanyName))]
    public string _companyName;

    public IContactDirectoryService ContactDirectoryService => _contactDirectoryService;

    public NewCompanyViewModel(ICompanyRequester caller, 
        IContactDirectoryService contactDirectoryService)
    {
        _callingViewModel = caller;
        _contactDirectoryService = contactDirectoryService;

        this.ValidateAllProperties();
    }

    [RelayCommand]
    private void SendCompany()
    {
        Company.CompanyName = CompanyName;
        _callingViewModel.CompanyComplete(Company);
        this.OnClosingRequest();
    }
}

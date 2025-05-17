using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using Transmittal.Desktop.Requesters;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.Validation;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels
{
    public partial class NewCompanyViewModel : BaseViewModel, IHasContactDirectoryService
    {
        private readonly ICompanyRequester _callingViewModel;
        private readonly IContactDirectoryService _contactDirectoryService = Host.GetService<IContactDirectoryService>();

        [ObservableProperty]
        private CompanyModel _company = new();

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(ValidationHelpers), nameof(ValidationHelpers.ValidateCompanyName))]
        public string _companyName;

        public IContactDirectoryService ContactDirectoryService => _contactDirectoryService;

        public NewCompanyViewModel(ICompanyRequester caller)
        {
            _callingViewModel = caller;

            this.ValidateAllProperties();
        }

        [RelayCommand]
        private void SendCompany()
        {
            Company.CompanyName = CompanyName;
            _callingViewModel.CompanyComplete(Company);
            this.OnClosingRequest();
        }

        //public static ValidationResult ValidateCompanyName(string value, ValidationContext context)
        //{
        //    if (string.IsNullOrWhiteSpace(value))
        //        return new ValidationResult("A company name is required");

        //    var instance = (NewCompanyViewModel)context.ObjectInstance;
        //    var existingNames = instance._contactDirectoryService.GetCompanies_All()
        //        .Select(c => c.CompanyName?.Trim().ToLowerInvariant())
        //        .ToList();

        //    if (existingNames.Contains(value.Trim().ToLowerInvariant()))
        //        return new ValidationResult("This company name already exists.");

        //    return ValidationResult.Success;
        //}
    }
}

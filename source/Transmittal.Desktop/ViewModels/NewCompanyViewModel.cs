using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
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
        private readonly IContactDirectoryService _contactDirectoryService;
        private readonly IMessageBoxService _messageBoxService;

        [ObservableProperty]
        private CompanyModel _company = new();

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(ValidationHelpers), nameof(ValidationHelpers.ValidateCompanyName))]
        public string _companyName;

        public IContactDirectoryService ContactDirectoryService => _contactDirectoryService;

        public NewCompanyViewModel(ICompanyRequester caller, 
            IContactDirectoryService contactDirectoryService,
            IMessageBoxService messageBoxService)
        {
            _callingViewModel = caller;
            _contactDirectoryService = contactDirectoryService;
            _messageBoxService = messageBoxService;

            this.ValidateAllProperties();
        }

        [RelayCommand]
        private void SendCompany()
        {
            var companyName = CompanyName?.Trim();
            var companyMatch = _contactDirectoryService.FindCompanyMatches(companyName).FirstOrDefault();

            if (companyMatch != null)
            {
                var useExisting = _messageBoxService.ShowYesNo(
                    "Similar company found",
                    $"A similar company already exists:\n\n{companyMatch.CompanyName}\n\nUse the existing company instead of creating a new one?");

                if (useExisting)
                {
                    _callingViewModel.CompanyComplete(companyMatch);
                    this.OnClosingRequest();
                    return;
                }
            }

            Company.CompanyName = companyName;
            _callingViewModel.CompanyComplete(Company);
            this.OnClosingRequest();
        }
    }
}

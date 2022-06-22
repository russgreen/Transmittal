using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using Transmittal.Library.Models;
using Transmittal.Library.ViewModels;
using Transmittal.Requesters;

namespace Transmittal.ViewModels
{
    public partial class NewCompanyViewModel : BaseViewModel
    {
        private readonly ICompanyRequester _callingViewModel;

        [ObservableProperty]
        private CompanyModel _company = new();

        [ObservableProperty]
        [Required(ErrorMessage = "A company name is required")]
        public string _companyName;

        public NewCompanyViewModel(ICompanyRequester caller)
        {
            _callingViewModel = caller;

            this.ValidateAllProperties();
        }


        [ICommand]
        private void SendCompany()
        {
            _company.CompanyName = _companyName;
            _callingViewModel.CompanyComplete(_company);
            this.OnClosingRequest();
        }
    }
}

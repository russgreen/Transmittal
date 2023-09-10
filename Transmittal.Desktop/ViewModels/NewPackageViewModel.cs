using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using Transmittal.Desktop.Requesters;
using Transmittal.Library.ViewModels;

namespace Transmittal.Desktop.ViewModels
{
    internal partial class NewPackageViewModel : BaseViewModel
    {
        private readonly IPackageRequester _callingViewModel;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "A package name is required")]
        private string _packageName;

        public NewPackageViewModel(IPackageRequester caller)
        {
            _callingViewModel = caller;

            this.ValidateAllProperties();
        }

        [RelayCommand]
        private void SendPackage()
        {
            _callingViewModel.PackageComplete(PackageName);
            this.OnClosingRequest();
        }
    }
}

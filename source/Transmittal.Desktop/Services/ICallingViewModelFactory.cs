using Transmittal.Desktop.Requesters;
using Transmittal.Desktop.ViewModels;

namespace Transmittal.Desktop.Services;
internal interface ICallingViewModelFactory
{
    NewCompanyViewModel CreateNewCompanyViewModel(ICompanyRequester caller);
    NewPersonViewModel CreateNewPersonViewModel(IPersonRequester caller);
    NewPackageViewModel CreateNewPackageViewModel(IPackageRequester caller);
}

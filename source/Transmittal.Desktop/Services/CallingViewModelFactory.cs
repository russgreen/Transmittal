using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Desktop.Requesters;
using Transmittal.Desktop.ViewModels;
using Transmittal.Library.Services;

namespace Transmittal.Desktop.Services;
internal class CallingViewModelFactory : ICallingViewModelFactory
{
    private readonly IContactDirectoryService _contactDirectoryService;

    public CallingViewModelFactory(IContactDirectoryService contactDirectoryService)
    {
        _contactDirectoryService = contactDirectoryService;
    }

    public NewCompanyViewModel CreateNewCompanyViewModel(ICompanyRequester caller)
    {
        return new NewCompanyViewModel(caller, _contactDirectoryService);
    }

    public NewPersonViewModel CreateNewPersonViewModel(IPersonRequester caller)
    {
        return new NewPersonViewModel(caller, _contactDirectoryService);
    }

    public NewPackageViewModel CreateNewPackageViewModel(IPackageRequester caller)
    {
        return new NewPackageViewModel(caller);
    }
}

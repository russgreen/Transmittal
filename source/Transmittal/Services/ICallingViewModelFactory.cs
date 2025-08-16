using Transmittal.Requesters;
using Transmittal.ViewModels;

namespace Transmittal.Services;
internal interface ICallingViewModelFactory
{
    NewCompanyViewModel CreateNewCompanyViewModel(ICompanyRequester caller);
    NewPersonViewModel CreateNewPersonViewModel(IPersonRequester caller);
    NewRevisionViewModel CreateNewPackageViewModel(IRevisionRequester caller);
    ParameterSelectorViewModel CreateParameterSelectorViewModel(IParameterGuidRequester caller, string targetVariable);
    RevisionsViewModel CreateRevisionsViewModel(IRevisionRequester caller);
    NewRevisionViewModel CreateNewRevisionViewModel(IRevisionRequester caller);
    StatusViewModel CreateStatusViewModel(IStatusRequester caller);
}

using Transmittal.Library.Services;
using Transmittal.Requesters;
using Transmittal.ViewModels;

namespace Transmittal.Services;
internal class CallingViewModelFactory : ICallingViewModelFactory
{
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ISettingsService _settingsService;
    private readonly IMessageBoxService _messageBoxService;

    public CallingViewModelFactory(IContactDirectoryService contactDirectoryService, 
        ISettingsService settingsService,
        IMessageBoxService messageBoxService)
    {
        _contactDirectoryService = contactDirectoryService;
        _settingsService = settingsService;
        _messageBoxService = messageBoxService;
    }

    public NewCompanyViewModel CreateNewCompanyViewModel(ICompanyRequester caller)
    {
        return new NewCompanyViewModel(caller, _contactDirectoryService);
    }

    public NewRevisionViewModel CreateNewPackageViewModel(IRevisionRequester caller)
    {
        return new NewRevisionViewModel(caller, _settingsService);
    }

    public NewPersonViewModel CreateNewPersonViewModel(IPersonRequester caller)
    {
        return new NewPersonViewModel(caller, _contactDirectoryService);
    }



    public ParameterSelectorViewModel CreateParameterSelectorViewModel(IParameterGuidRequester caller,
        string targetVariable)
    {
        return new ParameterSelectorViewModel(caller, targetVariable);
    }

    public RevisionsViewModel CreateRevisionsViewModel(IRevisionRequester caller)
    {
        return new RevisionsViewModel(caller, 
            _settingsService, 
            _messageBoxService);
    }

    public NewRevisionViewModel CreateNewRevisionViewModel(IRevisionRequester caller)
    {
        return new NewRevisionViewModel(caller, _settingsService);
    }

    public StatusViewModel CreateStatusViewModel(IStatusRequester caller)
    {
        return new StatusViewModel(caller, _settingsService);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Transmittal.Library.Models;
using Transmittal.Library.ViewModels;
using Transmittal.Library.Services;
using Transmittal.Requesters;
using Transmittal.Services;

namespace Transmittal.ViewModels;

internal partial class StatusViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IStatusRequester _callingViewModel;

    public List<DocumentStatusModel> DocumentStatuses { get; private set; }

    [ObservableProperty]
    private DocumentStatusModel _selectedDocumentStatus;
       

    public StatusViewModel()
    {
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;
    }

    public StatusViewModel(IStatusRequester caller, 
        ISettingsService settingsService)
    {
        _callingViewModel = caller;
        _settingsService = settingsService;

        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;
    }
    
    [RelayCommand]
    private void SendStatus()
    {
        _callingViewModel.StatusComplete(SelectedDocumentStatus);
        this.OnClosingRequest();
    }
        
}

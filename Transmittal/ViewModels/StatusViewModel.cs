using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Transmittal.Library.Models;
using Transmittal.Library.ViewModels;
using Transmittal.Library.Services;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

//[INotifyPropertyChanged]
internal partial class StatusViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
    private readonly IStatusRequester _callingViewModel;

    public List<DocumentStatusModel> DocumentStatuses { get; private set; }

    [ObservableProperty]
    private DocumentStatusModel _selectedDocumentStatus;
       

    public StatusViewModel()
    {
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;
    }

    public StatusViewModel(IStatusRequester caller)
    {
        DocumentStatuses = _settingsService.GlobalSettings.DocumentStatuses;
        _callingViewModel = caller;
    }
    
    [RelayCommand]
    private void SendStatus()
    {
        _callingViewModel.StatusComplete(_selectedDocumentStatus);
        this.OnClosingRequest();
    }
        
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Transmittal.Library.ViewModels;
using Transmittal.Messages;

namespace Transmittal.ViewModels;
internal partial class ProgressViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _currentStepProgressLabel = string.Empty;

    [ObservableProperty]
    private double _drawingSheetsToProcess = 0;
    [ObservableProperty]
    private double _drawingSheetsProcessed = 0;
    [ObservableProperty]
    private string _drawingSheetProgressLabel = string.Empty;

    [ObservableProperty]
    private double _sheetTasksToProcess = 0;
    [ObservableProperty]
    private double _sheetTaskProcessed = 0;
    [ObservableProperty]
    private string _sheetTaskProgressLabel = string.Empty;

    public ProgressViewModel()
    {
        WeakReferenceMessenger.Default.Register<ProgressUpdateMessage>(this, (r, m) =>
        {
            CurrentStepProgressLabel = m.Value.CurrentStepProgressLabel;
            
            DrawingSheetsToProcess = m.Value.DrawingSheetsToProcess;
            DrawingSheetsProcessed = m.Value.DrawingSheetsProcessed;
            DrawingSheetProgressLabel = m.Value.DrawingSheetProgressLabel;

            SheetTasksToProcess = m.Value.SheetTasksToProcess;
            SheetTaskProcessed = m.Value.SheetTaskProcessed;
            SheetTaskProgressLabel  = m.Value.SheetTaskProgressLabel;
        });
    }

    [RelayCommand]
    public void CancelTransmittal()
    {
        CurrentStepProgressLabel = "Cancelling transmittal...";

        WeakReferenceMessenger.Default.Send(new CancelTransmittalMessage(true));
    }
}

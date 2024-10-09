using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.IO;
using Transmittal.Library.Messages;
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

    [ObservableProperty]
    private string _timeMessage = string.Empty;

    [ObservableProperty]
    private string _displayMessage = string.Empty;

    public ProgressViewModel()
    {
        var startTime = DateTime.Now;

        WeakReferenceMessenger.Default.Register<ProgressUpdateMessage>(this, (r, m) =>
        {
            CurrentStepProgressLabel = m.Value.CurrentStepProgressLabel;
            
            DrawingSheetsToProcess = m.Value.DrawingSheetsToProcess;
            DrawingSheetsProcessed = m.Value.DrawingSheetsProcessed;
            DrawingSheetProgressLabel = m.Value.DrawingSheetProgressLabel;

            SheetTasksToProcess = m.Value.SheetTasksToProcess;
            SheetTaskProcessed = m.Value.SheetTaskProcessed;
            SheetTaskProgressLabel  = m.Value.SheetTaskProgressLabel;

            var workDone = DrawingSheetsProcessed / DrawingSheetsToProcess;

            if (workDone == 0)
            {
                TimeMessage = "Calculating time remaining...";
                return;
            }
            else
            {
                var timeElapsed = DateTime.Now - startTime;
                var timeOverall = TimeSpan.FromTicks((long)(timeElapsed.Ticks / workDone));
                var timeRemaining = timeOverall - timeElapsed;

                TimeMessage = $"Time elapsed: {timeElapsed.Minutes:D2}:{timeElapsed.Seconds:D2} Time remaining: {timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
            }


        });

        WeakReferenceMessenger.Default.Register<LockFileMessage>(this, (r, m) =>
        {
            ProcessLockFileMessage(m.Value);
        });
    }

    private void ProcessLockFileMessage(string value)
    {
        if (value == "")
        {
            DisplayMessage = "";
            return;
        }

        //so we have a lock file
        DisplayMessage = $"Waiting for database lock file to clear. Check if the {value} needs to be manually deleted.";
    }

    [RelayCommand]
    public void CancelTransmittal()
    {
        CurrentStepProgressLabel = "Cancelling transmittal...";

        WeakReferenceMessenger.Default.Send(new CancelTransmittalMessage(true));
    }
}

﻿namespace Transmittal.Models;
internal class ProgressMessageModel
{
    public string CurrentStepProgressLabel { get; set; }

    public double DrawingSheetsToProcess { get; set; }

    public double DrawingSheetsProcessed { get; set; }

    public string DrawingSheetProgressLabel { get; set; }

    public double SheetTasksToProcess { get; set; }

    public double SheetTaskProcessed { get; set; }

    public string SheetTaskProgressLabel { get; set; }

}

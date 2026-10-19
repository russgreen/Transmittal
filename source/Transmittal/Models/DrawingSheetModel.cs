using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using Transmittal.Library.Models;

namespace Transmittal.Models;

public partial class DrawingSheetModel : TransmittalItemModel
{
    public ElementId ID { get; set; }
    public string IssueDate { get; set; }

    [ObservableProperty]
    private string _revDate;

    [ObservableProperty]
    private string _revNotes;

    [ObservableProperty]
    private bool _isDuplicateSheet;

    [ObservableProperty]
    private bool? _exportPDF = null;

    [ObservableProperty]
    private bool? _exportDWG = null;

    [ObservableProperty]
    private bool? _exportDWF = null;
}

using Autodesk.Revit.DB;
using Transmittal.Library.Models;

namespace Transmittal.Models;

public class DrawingSheetModel : TransmittalItemModel
{
    public ElementId ID { get; set; }
    public string IssueDate { get; set; }
    public string RevDate { get; set; }
    public string RevNotes { get; set; }
}

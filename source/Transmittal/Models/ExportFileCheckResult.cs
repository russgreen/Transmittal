using Autodesk.Revit.DB;
using Transmittal.Enums;

namespace Transmittal.Models;

internal class ExportFileCheckResult
{
    public ElementId SheetId { get; set; }

    public string SheetNumber { get; set; } = string.Empty;

    public ExportFormatType ExportFormat { get; set; }

    public string OutputPath { get; set; } = string.Empty;

    public bool FileExists { get; set; }
}

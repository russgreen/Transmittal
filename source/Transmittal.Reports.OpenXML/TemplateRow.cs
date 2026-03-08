using ClosedXML.Excel;

namespace Transmittal.Reports.OpenXML;

internal sealed class TemplateRow
{
    public int RowNumber { get; set; }
    public int FirstColumn { get; set; }
    public int LastColumn { get; set; }
    public IXLStyle RowStyle { get; set; }
    public List<TemplateCell> Cells { get; set; } = new List<TemplateCell>();
}

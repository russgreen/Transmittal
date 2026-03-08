using ClosedXML.Excel;

namespace Transmittal.Reports.OpenXML.Models;

internal sealed class TemplateCell
{
    public int Column { get; set; }
    public IXLStyle Style { get; set; }
    public bool HasFormula { get; set; }
    public string FormulaA1 { get; set; }
    public string TextValue { get; set; }
    public XLCellValue Value { get; set; }
    public XLDataType DataType { get; set; }
}

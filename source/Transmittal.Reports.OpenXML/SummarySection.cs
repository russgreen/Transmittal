namespace Transmittal.Reports.OpenXML;

internal sealed class SummarySection
{
    public int HeaderRow { get; set; }
    public int StartRow { get; set; }
    public int EndRow { get; set; }
    public int FirstDataColumn { get; set; }
    public int NumberColumn { get; set; }
    public int NameColumn { get; set; }
    public int PaperColumn { get; set; }
    public int LatestRevColumn { get; set; }
    public int CompanyColumn { get; set; }
    public int FormatRow { get; set; }
    public DateRows DateRows { get; set; } = new DateRows();
    public TemplateRow TemplateRow { get; set; }

    public void ShiftRows(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        HeaderRow += delta;
        StartRow += delta;
        EndRow += delta;

        if (FormatRow > 0)
        {
            FormatRow += delta;
        }

        if (DateRows == null)
        {
            return;
        }

        if (DateRows.YearRow > 0)
        {
            DateRows.YearRow += delta;
        }

        if (DateRows.MonthRow > 0)
        {
            DateRows.MonthRow += delta;
        }

        if (DateRows.DayRow > 0)
        {
            DateRows.DayRow += delta;
        }

        if (TemplateRow != null)
        {
            TemplateRow.RowNumber += delta;
        }
    }
}

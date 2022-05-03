using Transmittal.Library.Models;

namespace Transmittal.Reports.Models;

public class TransmittalItemReportModel : TransmittalItemModel
{
    public string Project { get; set; }

    public DateTime TransDate { get; set; }

    public int DateYear
    {
        get
        {
            return int.Parse(TransDate.Year.ToString().Substring(2, 2));
        }
    }
    public int DateMonth
    {
        get
        {
            return TransDate.Month;
        }
    }
    public int DateDay
    {
        get
        {
            return TransDate.Day;
        }
    }
}

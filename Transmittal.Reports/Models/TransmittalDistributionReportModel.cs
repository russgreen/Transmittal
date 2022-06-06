using Transmittal.Library.Models;

namespace Transmittal.Reports.Models;

public class TransmittalDistributionReportModel : TransmittalDistributionModel
{
    public CompanyModel Company { get; set; }
    public PersonModel Person { get; set; }
    public string ContactName => $"{Person.FullNameReversed} ({Company.CompanyName})";

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

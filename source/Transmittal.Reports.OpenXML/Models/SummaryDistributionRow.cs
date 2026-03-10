using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML.Models;

internal sealed class SummaryDistributionRow
{
    public Dictionary<int, DistCell> FormatByTransmittal { get; set; } = new Dictionary<int, DistCell>();
    public TransmittalDistributionModel TemplateDistribution { get; set; }
    public TransmittalModel TemplateTransmittal { get; set; }
    public PersonModel Person { get; set; }
    public CompanyModel CompanyModel { get; set; }
    public Dictionary<string, string> RowContext { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

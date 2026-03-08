using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML;

internal sealed class DistributionTemplateRow
{
    public TransmittalDistributionModel Distribution { get; set; }
    public PersonModel Person { get; set; }
    public CompanyModel Company { get; set; }
}

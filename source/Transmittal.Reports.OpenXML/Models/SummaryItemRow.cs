using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML.Models;

internal sealed class SummaryItemRow
{
    public Dictionary<int, string> RevisionsByTransmittal { get; set; } = new Dictionary<int, string>();
    public TransmittalItemModel TemplateItem { get; set; }
    public TransmittalModel TemplateTransmittal { get; set; }
    public Dictionary<string, string> RowContext { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

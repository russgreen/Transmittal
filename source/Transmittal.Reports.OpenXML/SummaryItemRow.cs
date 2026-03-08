using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML;

internal sealed class SummaryItemRow
{
    public string DrawingNumber { get; set; } = string.Empty;
    public string DrawingName { get; set; } = string.Empty;
    public string Paper { get; set; } = string.Empty;
    public string LatestRevision { get; set; } = string.Empty;
    public string Package { get; set; } = string.Empty;
    public Dictionary<int, string> RevisionsByTransmittal { get; set; } = new Dictionary<int, string>();
    public TransmittalItemModel TemplateItem { get; set; }
    public TransmittalModel TemplateTransmittal { get; set; }
    public Dictionary<string, string> RowContext { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

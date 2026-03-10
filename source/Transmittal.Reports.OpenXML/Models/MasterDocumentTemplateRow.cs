using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML.Models;

internal sealed class MasterDocumentTemplateRow
{
    public TransmittalModel Transmittal { get; set; }
    public TransmittalItemModel Item { get; set; }
}

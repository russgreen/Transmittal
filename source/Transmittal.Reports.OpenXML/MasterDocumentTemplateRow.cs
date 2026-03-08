using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML;

internal sealed class MasterDocumentTemplateRow
{
    public TransmittalModel Transmittal { get; set; }
    public TransmittalItemModel Item { get; set; }
}

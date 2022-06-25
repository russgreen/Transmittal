namespace Transmittal.Library.Models;
public class DocumentModel : TransmittalItemModel
{
    public string FilePath { get; set; }

    public string FileName { get; set; }

    public DocumentTypeModel DocumentType { get; set; }
}

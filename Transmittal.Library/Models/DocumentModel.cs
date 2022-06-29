using Transmittal.Library.Validation;

namespace Transmittal.Library.Models;
public class DocumentModel : TransmittalItemModel
{
    public string FilePath { get; set; }

    [BeginsWith(nameof(DocumentModel.DrgProj), 
        ErrorMessage = "The file name doesn't begin with the project number. Are you sure you want to record a transmittal?")]
    public string FileName { get; set; }

    public DocumentTypeModel DocumentType { get; set; }
}

using System.Reflection;
using Transmittal.Library.Validation;
using Humanizer;

namespace Transmittal.Library.Models;
public class DocumentModel : TransmittalItemModel
{
    public string FilePath { get; set; }

    [BeginsWith(nameof(DocumentModel.DrgProj),
        ErrorMessage = "The file name doesn't begin with the project number. Are you sure you want to record a transmittal?")]
    public string FileName { get; set; }

    public DocumentTypeModel DocumentType { get; set; }

    public string DocumentStatus => string.IsNullOrEmpty(DrgStatusDescription) ? DrgStatus : $"{DrgStatus} - {DrgStatusDescription.Transform(To.LowerCase, To.TitleCase)}";

    public DocumentModel()
    {
    }

    public DocumentModel(DocumentModel model)
    {
        DrgNumber = model.DrgNumber;
        DrgRev = model.DrgRev;
        DrgName = model.DrgName;
        DrgPaper = model.DrgPaper;
        DrgScale = model.DrgScale;
        DrgDrawn = model.DrgDrawn;
        DrgChecked = model.DrgChecked;
        DrgProj = model.DrgProj;
        DrgOriginator = model.DrgOriginator;
        DrgVolume = model.DrgVolume;
        DrgLevel = model.DrgLevel;
        DrgType = model.DrgType;
        DrgRole = model.DrgRole;
        DrgStatus = model.DrgStatus;
        DrgStatusDescription = model.DrgStatusDescription;
    }

    public DocumentModel(TransmittalItemModel model)
    {
        DrgNumber = model.DrgNumber;
        DrgRev = model.DrgRev;
        DrgName = model.DrgName;
        DrgPaper = model.DrgPaper;
        DrgScale = model.DrgScale;
        DrgDrawn = model.DrgDrawn;
        DrgChecked = model.DrgChecked;
        DrgProj = model.DrgProj;
        DrgOriginator = model.DrgOriginator;
        DrgVolume = model.DrgVolume;
        DrgLevel = model.DrgLevel;
        DrgType = model.DrgType;
        DrgRole = model.DrgRole;
        DrgStatus = model.DrgStatus;
        DrgStatusDescription = model.DrgStatusDescription;
    }
}

using Transmittal.Library.Models;

namespace Transmittal.Library.Helpers;
public static class ISO19650
{
    public static List<DocumentTypeModel> GetDocumentTypes()
    {
        List<DocumentTypeModel> documentTypes = new List<DocumentTypeModel>();

        documentTypes.Clear();
        documentTypes.Add(new DocumentTypeModel() { Code = "AF", Description = "Animation file (of a model)" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CM", Description = "Combined model (combined multidiscipline model)" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CR", Description = "Specific for the clash process" });
        documentTypes.Add(new DocumentTypeModel() { Code = "DR", Description = "2D drawing" });
        documentTypes.Add(new DocumentTypeModel() { Code = "M2", Description = "2D model file" });
        documentTypes.Add(new DocumentTypeModel() { Code = "M3", Description = "3D model file" });
        documentTypes.Add(new DocumentTypeModel() { Code = "MR", Description = "Model rendition file For other renditions, e.g thermal analysis etc." });
        documentTypes.Add(new DocumentTypeModel() { Code = "VS", Description = "Visualization file (of a model)" });
        documentTypes.Add(new DocumentTypeModel() { Code = "BQ", Description = "Bill of quantities" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CA", Description = "Calculations" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CO", Description = "Correspondence" });
        documentTypes.Add(new DocumentTypeModel() { Code = "CP", Description = "Cost plan" });
        documentTypes.Add(new DocumentTypeModel() { Code = "DB", Description = "Database" });
        documentTypes.Add(new DocumentTypeModel() { Code = "FN", Description = "File note" });
        documentTypes.Add(new DocumentTypeModel() { Code = "HS", Description = "Health And safety" });
        documentTypes.Add(new DocumentTypeModel() { Code = "IE", Description = "Information exchange file" });
        documentTypes.Add(new DocumentTypeModel() { Code = "MI", Description = "Minutes / Action notes" });
        documentTypes.Add(new DocumentTypeModel() { Code = "MS", Description = "Method statement" });
        documentTypes.Add(new DocumentTypeModel() { Code = "PP", Description = "Presentation" });
        documentTypes.Add(new DocumentTypeModel() { Code = "PR", Description = "Programme" });
        documentTypes.Add(new DocumentTypeModel() { Code = "RD", Description = "Room data sheet" });
        documentTypes.Add(new DocumentTypeModel() { Code = "RI", Description = "Request for information" });
        documentTypes.Add(new DocumentTypeModel() { Code = "RP", Description = "Report" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SA", Description = "Schedule of accommodation" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SH", Description = "Schedule" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SN", Description = "Snagging list" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SP", Description = "Specification" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SU", Description = "Survey" });
        documentTypes.Add(new DocumentTypeModel() { Code = "GA", Description = "General arrangement drawing" });
        documentTypes.Add(new DocumentTypeModel() { Code = "DT", Description = "Detail/assembly drawing" });
        documentTypes.Add(new DocumentTypeModel() { Code = "SK", Description = "Sketch drawings" });

        return documentTypes;
    }

}

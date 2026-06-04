using Transmittal.Library.Models;

namespace Transmittal.Library.Helpers;
public static class ISO19650
{
    public static List<DocumentTypeModel> GetDocumentTypes()
    {
        var documentTypes = new List<DocumentTypeModel>
        {
            // Existing document types
            new DocumentTypeModel { Code = "D", Description = "Drawing" },
            new DocumentTypeModel { Code = "G", Description = "Diagram" },
            new DocumentTypeModel { Code = "I", Description = "Image" },
            new DocumentTypeModel { Code = "L", Description = "List" },
            new DocumentTypeModel { Code = "M", Description = "Model" },
            new DocumentTypeModel { Code = "T", Description = "Textual" },
            new DocumentTypeModel { Code = "V", Description = "Video/Audio" },
            
            // New document types added
            new DocumentTypeModel { Code = "AG", Description = "Agenda" },
            new DocumentTypeModel { Code = "BL", Description = "Brochure" },
            new DocumentTypeModel { Code = "CT", Description = "Comment" },
            new DocumentTypeModel { Code = "CD", Description = "Conversation record" },
            new DocumentTypeModel { Code = "CO", Description = "Correspondence" },
            new DocumentTypeModel { Code = "EM", Description = "Email" },
            new DocumentTypeModel { Code = "FN", Description = "File note" },
            new DocumentTypeModel { Code = "LF", Description = "Leaflet" },
            new DocumentTypeModel { Code = "LT", Description = "Letter" },
            new DocumentTypeModel { Code = "ME", Description = "Memo" },
            new DocumentTypeModel { Code = "MI", Description = "Minutes" },
            new DocumentTypeModel { Code = "PO", Description = "Poster" },
            new DocumentTypeModel { Code = "PP", Description = "Presentation" },
            new DocumentTypeModel { Code = "PE", Description = "Press release" },
            new DocumentTypeModel { Code = "RI", Description = "Request" },
            new DocumentTypeModel { Code = "TQ", Description = "Technical query" },
            new DocumentTypeModel { Code = "TN", Description = "Transfer note" },
            new DocumentTypeModel { Code = "TL", Description = "Transmittal" },
            new DocumentTypeModel { Code = "AP", Description = "Application" },
            new DocumentTypeModel { Code = "CC", Description = "Contract" },
            new DocumentTypeModel { Code = "EW", Description = "Early warning notice" },
            new DocumentTypeModel { Code = "IN", Description = "Instruction" },
            new DocumentTypeModel { Code = "NO", Description = "Notification" },
            new DocumentTypeModel { Code = "PS", Description = "Proposal" },
            new DocumentTypeModel { Code = "RQ", Description = "Requisition" },
            new DocumentTypeModel { Code = "SO", Description = "Subcontract order" },
            new DocumentTypeModel { Code = "VA", Description = "Variation" },
            new DocumentTypeModel { Code = "DB", Description = "Database" },
            new DocumentTypeModel { Code = "DS", Description = "Data set" },
            new DocumentTypeModel { Code = "IE", Description = "Information exchange file" },
            new DocumentTypeModel { Code = "RD", Description = "Room data sheet" },
            new DocumentTypeModel { Code = "SA", Description = "Schedule of accommodation" },
            new DocumentTypeModel { Code = "CA", Description = "Calculations" },
            new DocumentTypeModel { Code = "SW", Description = "Scope of works" },
            new DocumentTypeModel { Code = "SP", Description = "Specification" },
            new DocumentTypeModel { Code = "BQ", Description = "Bill of quantities" },
            new DocumentTypeModel { Code = "CP", Description = "Cost plan" },
            new DocumentTypeModel { Code = "ES", Description = "Estimate" },
            new DocumentTypeModel { Code = "FA", Description = "Fee agreement" },
            new DocumentTypeModel { Code = "IV", Description = "Invoice" },
            new DocumentTypeModel { Code = "QN", Description = "Quotation" },
            new DocumentTypeModel { Code = "AF", Description = "Animation file (of a model)" },
            new DocumentTypeModel { Code = "CM", Description = "Combined model (combined multidiscipline model)" },
            new DocumentTypeModel { Code = "CR", Description = "Specific for the clash process" },
            new DocumentTypeModel { Code = "DR", Description = "2D drawing" },
            new DocumentTypeModel { Code = "M2", Description = "2D model file" },
            new DocumentTypeModel { Code = "M3", Description = "3D model file" },
            new DocumentTypeModel { Code = "MR", Description = "Model rendition file For other renditions, e.g thermal analysis etc." },
            new DocumentTypeModel { Code = "VS", Description = "Visualization file (of a model)" },
            new DocumentTypeModel { Code = "GA", Description = "General arrangement drawing" },
            new DocumentTypeModel { Code = "DT", Description = "Detail/assembly drawing" },
            new DocumentTypeModel { Code = "SK", Description = "Sketch drawings" }
        };

        return documentTypes;
    }

}

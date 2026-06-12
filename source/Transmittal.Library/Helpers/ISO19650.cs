using Transmittal.Library.Models;

namespace Transmittal.Library.Helpers;
public static class ISO19650
{
    public static List<DocumentTypeModel> GetDocumentTypes()
    {
        var documentTypes = new List<DocumentTypeModel>
        {
                // Core Types
                new DocumentTypeModel { Code = "D", Description = "Drawing" },
                new DocumentTypeModel { Code = "G", Description = "Diagram" },
                new DocumentTypeModel { Code = "I", Description = "Image" },
                new DocumentTypeModel { Code = "L", Description = "List" },
                new DocumentTypeModel { Code = "M", Description = "Model" },
                new DocumentTypeModel { Code = "T", Description = "Textual" },
                new DocumentTypeModel { Code = "V", Description = "Video/Audio" },

                // Business Correspondence & Communication (A-F)
                new DocumentTypeModel { Code = "AG", Description = "Agenda" },
                new DocumentTypeModel { Code = "BL", Description = "Brochure" },
                new DocumentTypeModel { Code = "CT", Description = "Comment" },
                new DocumentTypeModel { Code = "CD", Description = "Conversation record" },
                new DocumentTypeModel { Code = "CO", Description = "Correspondence" },
                new DocumentTypeModel { Code = "EM", Description = "Email" },
                new DocumentTypeModel { Code = "FN", Description = "File note" },
                new DocumentTypeModel { Code = "LF", Description = "Leaflet" },

                // Formal Documents & Records (L-S)
                new DocumentTypeModel { Code = "LT", Description = "Letter" },
                new DocumentTypeModel { Code = "ME", Description = "Memo" },
                new DocumentTypeModel { Code = "MI", Description = "Minutes" },
                new DocumentTypeModel { Code = "NO", Description = "Notification" },
                new DocumentTypeModel { Code = "PO", Description = "Poster" },
                new DocumentTypeModel { Code = "PP", Description = "Presentation" },
                new DocumentTypeModel { Code = "PE", Description = "Press release" },
                new DocumentTypeModel { Code = "PS", Description = "Proposal" },
                new DocumentTypeModel { Code = "RI", Description = "Request" },
                new DocumentTypeModel { Code = "TQ", Description = "Technical query" },
                new DocumentTypeModel { Code = "TN", Description = "Transfer note" },
                new DocumentTypeModel { Code = "TL", Description = "Transmittal" },
                new DocumentTypeModel { Code = "AP", Description = "Application" },
                new DocumentTypeModel { Code = "CC", Description = "Contract" },
                new DocumentTypeModel { Code = "EW", Description = "Early warning notice" },
                new DocumentTypeModel { Code = "IN", Description = "Instruction" },
                new DocumentTypeModel { Code = "RQ", Description = "Requisition" },
                new DocumentTypeModel { Code = "SO", Description = "Subcontract order" },
                new DocumentTypeModel { Code = "VA", Description = "Variation" },

                // Technical & Project Management (D-S)
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
                new DocumentTypeModel { Code = "TR", Description = "Tracker" }, 

                // Visual and Model Files (A-S)
                new DocumentTypeModel { Code = "AF", Description = "Animation file (of a model)" },
                new DocumentTypeModel { Code = "CM", Description = "Combined model (combined multidiscipline model)" },
                new DocumentTypeModel { Code = "CR", Description = "Specific for the clash process" },
                new DocumentTypeModel { Code = "DR", Description = "2D drawing" },
                new DocumentTypeModel { Code = "M2", Description = "Model – two-dimensional (file)" },
                new DocumentTypeModel { Code = "M3", Description = "Model – three-dimensional (file)" },
                new DocumentTypeModel { Code = "MR", Description = "Model rendition file For other renditions, e.g thermal analysis etc." },
                new DocumentTypeModel { Code = "VS", Description = "Visualization file (of a model)" },

                // Drawings and Diagrams (General)
                new DocumentTypeModel { Code = "GA", Description = "General arrangement drawing" },
                new DocumentTypeModel { Code = "DT", Description = "Detail/assembly drawing" },
                new DocumentTypeModel { Code = "IM", Description = "Image" },
                new DocumentTypeModel { Code = "PH", Description = "Photograph" }, 
                new DocumentTypeModel { Code = "SC", Description = "Schematic" }, 
                new DocumentTypeModel { Code = "SK", Description = "Sketch drawings" },

                // Standards and Procedures (C-T)
                new DocumentTypeModel { Code = "PZ", Description = "Protocol" }, 
                new DocumentTypeModel { Code = "RN", Description = "Regulation" }, 
                new DocumentTypeModel { Code = "SD", Description = "Standard" }, 
                new DocumentTypeModel { Code = "MS", Description = "Method statement" },
                new DocumentTypeModel { Code = "PY", Description = "Policy" }, 
                new DocumentTypeModel { Code = "PC", Description = "Procedure" }, 
                new DocumentTypeModel { Code = "PR", Description = "Programme" },
                new DocumentTypeModel { Code = "SY", Description = "Strategy" }, 
                new DocumentTypeModel { Code = "CE", Description = "Certificate" }, 
                new DocumentTypeModel { Code = "CH", Description = "Chart" }, 
                new DocumentTypeModel { Code = "DT", Description = "Data sheet" },
                new DocumentTypeModel { Code = "DE", Description = "Diary entry" }, 
                new DocumentTypeModel { Code = "DY", Description = "Directory" }, 
                new DocumentTypeModel { Code = "FM", Description = "Form" },
                new DocumentTypeModel { Code = "GU", Description = "Guide" }, 
                new DocumentTypeModel { Code = "HS", Description = "Health and safety" },
                new DocumentTypeModel { Code = "LI", Description = "List" }, 
                new DocumentTypeModel { Code = "LG", Description = "Log" }, 
                new DocumentTypeModel { Code = "MA", Description = "Manual" },
                new DocumentTypeModel { Code = "MX", Description = "Matrix" },
                new DocumentTypeModel { Code = "PT", Description = "Permit" }, 
                new DocumentTypeModel { Code = "PL", Description = "Plan" }, 
                new DocumentTypeModel { Code = "PW", Description = "Process workflow" },
                new DocumentTypeModel { Code = "RG", Description = "Register" }, 
                new DocumentTypeModel { Code = "RP", Description = "Report" },
                new DocumentTypeModel { Code = "SH", Description = "Schedule or table" },
                new DocumentTypeModel { Code = "SN", Description = "Snagging list" },
                new DocumentTypeModel { Code = "ST", Description = "Study" }, 
                new DocumentTypeModel { Code = "SU", Description = "Survey" },
                new DocumentTypeModel { Code = "TF", Description = "Technology file" }, 
                new DocumentTypeModel { Code = "TE", Description = "Template" }, 
                new DocumentTypeModel { Code = "TG", Description = "Training record" }, 
                new DocumentTypeModel { Code = "VL", Description = "Valuation" }
        };

        return documentTypes;
    }

}

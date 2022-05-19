using Transmittal.Library.Models;

namespace Transmittal.Library.Standards;

public class ISO19650
{
    public static List<DocumentStatusModel> GetDocumentStatuses()
    {
        List<DocumentStatusModel> documentStatuses = new List<DocumentStatusModel>();

        documentStatuses.Clear();
        
        // non-contractual status codes
        documentStatuses.Add(new DocumentStatusModel() { Code = "S0", Description = "PRELIMINARY WIP" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "S1", Description = "FOR CO-ORDINATION" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "S2", Description = "FOR INFORMATION" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "S3", Description = "FOR REVIEW AND COMMENT" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "S4", Description = "FOR STAGE APPROVAL" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "S6", Description = "FOR PIM AUTHORIZATION" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "S7", Description = "FOR AIM AUTHORIZATION" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "D1", Description = "SUITABLE FOR COSTING" }); // old BS1192 but useful
        documentStatuses.Add(new DocumentStatusModel() { Code = "D2", Description = "SUITABLE FOR TENDER" }); // old BS1192 but useful
        documentStatuses.Add(new DocumentStatusModel() { Code = "D3", Description = "FOR CONTRACTOR DESIGN" }); // old BS1192 but useful

        // contractual status codes
        documentStatuses.Add(new DocumentStatusModel() { Code = "A3", Description = "CONTRACTUAL STAGE 3" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "A4", Description = "CONTRACTUAL STAGE 4" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "A5", Description = "CONTRACTUAL STAGE 5" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "A6", Description = "CONTRACTUAL STAGE 6" });

        // partial contractual status codes - should be on preliminary revision
        documentStatuses.Add(new DocumentStatusModel() { Code = "B3", Description = "PARTIAL STAGE 3" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "B4", Description = "PARTIAL STAGE 4" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "B5", Description = "PARTIAL STAGE 5" });
        documentStatuses.Add(new DocumentStatusModel() { Code = "B6", Description = "PARTIAL STAGE 6" });

        // contractual status code
        documentStatuses.Add(new DocumentStatusModel() { Code = "CR", Description = "AS BUILT" });

        return documentStatuses;
    }
}

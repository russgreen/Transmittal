using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public class SettingsService : ISettingsService
{
    public SettingsModel GlobalSettings { get; set; }

    public SettingsService()
    {
        GlobalSettings = new();

        GlobalSettings.IssueFormats = GetIssueFormats();
        GlobalSettings.DrawingIssueStore = GetDrawingIssueStore();
        GlobalSettings.DocumentStatuses = GetDocumentStatuses();      
    }

    public void GetSettings()
    {
        GlobalSettings.RecordTransmittals = true;


    }

    public void UpdateSettings()
    {
       //throw new NotImplementedException();
    }
    
    private List<IssueFormatModel> GetIssueFormats()
    {
        //build the issue formats list  
        //TODO - get from the database if one exists
        var issueFormats = new List<IssueFormatModel>
        {
            new IssueFormatModel() { Code = "E", Description = "Email" },
            new IssueFormatModel() { Code = "C", Description = "Cloud" },
            new IssueFormatModel() { Code = "P", Description = "Paper" }
        };

        return issueFormats;
    }

    private List<DocumentStatusModel> GetDocumentStatuses()
    {
        // TODO - check if there is a database and load from there else use the standards
        List<DocumentStatusModel> documentStatuses = Standards.ISO19650.GetDocumentStatuses();

        return documentStatuses;
    } 

    private string GetDrawingIssueStore()
    {
       //get the current windows user documents folder
       var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);       
        
       //if the issuestore parameter does not exist use the users documents folder
       //TODO get the value from the paramater in the project

       return documentsFolder;
    }


}
    
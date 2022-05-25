using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public class SettingsService : ISettingsService
{
    private readonly IDataConnection _dataConnection;

    public SettingsModel GlobalSettings { get; set; }

    public SettingsService(IDataConnection dataConnection)
    {
        _dataConnection = dataConnection;        
        
        GlobalSettings = new();

        GetSettings();
    }

    public void GetSettings()
    {
        //this is where additonal settings related to reported are pulled form the project DB.
        
        //GlobalSettings.IssueFormats = GetIssueFormats();
        //GlobalSettings.DocumentStatuses = GetDocumentStatuses();
    }

    public void UpdateSettings()
    {
       //throw new NotImplementedException();
    }

    private List<IssueFormatModel> GetIssueFormats()
    {
        //build the issue formats list  
        //TODO - get from the database if one exists
        List<IssueFormatModel> issueFormats = new();

        return issueFormats;
    }

    private List<DocumentStatusModel> GetDocumentStatuses()
    {
        // TODO - check if there is a database and load from there else use the standards
        List<DocumentStatusModel> documentStatuses = new();

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
    
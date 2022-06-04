using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public class SettingsService : ISettingsService
{
    private readonly IDataConnection _connection;

    public SettingsModel GlobalSettings { get; set; }
    
    public SettingsService(IDataConnection dataConnection)
    {
        _connection = dataConnection;        
        
        GlobalSettings = new();
    }

    public void GetSettings()
    {
        //this is where additonal settings related to desktop.exe and reporting are pulled form the project DB.

        if (GlobalSettings.DatabaseFile != "[NONE]" || GlobalSettings.DatabaseFile != null)
        {
            if (File.Exists(GlobalSettings.DatabaseFile))
            {
                string sql = "SELECT * FROM Settings WHERE ID = 1;";

                var dbSettings = _connection.LoadData<SettingsModel, dynamic>(
                    GlobalSettings.DatabaseFile, 
                    sql, null).FirstOrDefault();

                if (dbSettings != null)
                {
                    GlobalSettings.DateFormatString = dbSettings.DateFormatString;
                    GlobalSettings.DrawingIssueStore = dbSettings.DrawingIssueStore;
                    GlobalSettings.IssueSheetStore = dbSettings.IssueSheetStore;
                    GlobalSettings.ReportStore = dbSettings.ReportStore;
                    GlobalSettings.DirectoryStore = dbSettings.DirectoryStore;
                    GlobalSettings.FileNameFilter = dbSettings.FileNameFilter;
                    GlobalSettings.ProjectIdentifier = dbSettings.ProjectIdentifier;
                    GlobalSettings.ProjectNumber = dbSettings.ProjectNumber;
                    GlobalSettings.ProjectName = dbSettings.ProjectName;
                    GlobalSettings.UseExtranet = dbSettings.UseExtranet;
                    GlobalSettings.UseISO19650 = dbSettings.UseISO19650;
                    GlobalSettings.Originator = dbSettings.Originator;
                    GlobalSettings.Role = dbSettings.Role;
                }
                    
            }
        }
    }

    public void UpdateSettings()
    {
        string sql = "UPDATE Settings SET " +
            "DateFormatString = @DateFormatString, " +
            "DrawingIssueStore = @DrawingIssueStore, " +
            "IssueSheetStore = @IssueSheetStore, " +
            "ReportStore = @ReportStore, " +
            "DirectoryStore = @DirectoryStore, " +
            "FileNameFilter = @FileNameFilter, " +
            "ProjectIdentifier = @ProjectIdentifier, " +
            "ProjectNumber = @ProjectNumber, " +           
            "ProjectName = @ProjectName, " +
            "UseExtranet = @UseExtranet, " +
            "UseISO19650 = @UseISO19650, " +
            "Originator = @Originator, " +
            "Role = @Role, " +
            "ProjectIdentifierParamGuid = @ProjectIdentifierParamGuid, " +
            "OriginatorParamGuid = @OriginatorParamGuid, " +
            "RoleParamGuid = @RoleParamGuid, " +
            "SheetVolumeParamGuid = @SheetVolumeParamGuid, " +
            "SheetLevelParamGuid = @SheetLevelParamGuid, " +
            "DocumentTypeParamGuid = @DocumentTypeParamGuid, " +
            "SheetStatusParamGuid = @SheetStatusParamGuid, " +
            "SheetStatusDescriptionParamGuid = @SheetStatusDescriptionParamGuid " +
            "WHERE ID=1;";

        _connection.SaveData(
            GlobalSettings.DatabaseFile,
            sql, new
            {
                DateFormatString = GlobalSettings.DateFormatString,
                DrawingIssueStore = GlobalSettings.DrawingIssueStore,
                IssueSheetStore = GlobalSettings.IssueSheetStore,
                ReportStore = GlobalSettings.ReportStore,
                DirectoryStore = GlobalSettings.DirectoryStore,
                FileNameFilter = GlobalSettings.FileNameFilter,
                ProjectIdentifier = GlobalSettings.ProjectIdentifier,
                ProjectNumber = GlobalSettings.ProjectNumber,
                ProjectName = GlobalSettings.ProjectName,                
                UseExtranet = GlobalSettings.UseExtranet,
                UseISO19650 = GlobalSettings.UseISO19650,
                Originator = GlobalSettings.Originator,
                Role = GlobalSettings.Role,
                ProjectIdentifierParamGuid = GlobalSettings.ProjectIdentifierParamGuid,
                OriginatorParamGuid = GlobalSettings.OriginatorParamGuid,
                RoleParamGuid = GlobalSettings.RoleParamGuid,
                SheetVolumeParamGuid = GlobalSettings.SheetVolumeParamGuid,
                SheetLevelParamGuid = GlobalSettings.SheetLevelParamGuid,
                DocumentTypeParamGuid = GlobalSettings.DocumentTypeParamGuid,
                SheetStatusParamGuid = GlobalSettings.SheetStatusParamGuid,
                SheetStatusDescriptionParamGuid = GlobalSettings.SheetStatusDescriptionParamGuid
            });
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
    
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Extensions;
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
            if (File.Exists(GlobalSettings.DatabaseFile.ParsePathWithEnvironmentVariables()))
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
                    GlobalSettings.FileNameFilter2 = dbSettings.FileNameFilter2;
                    GlobalSettings.UseExtranet = dbSettings.UseExtranet;
                    GlobalSettings.UseISO19650 = dbSettings.UseISO19650;
                    GlobalSettings.ProjectNumber = dbSettings.ProjectNumber;
                    GlobalSettings.ProjectName = dbSettings.ProjectName;
                    GlobalSettings.ProjectIdentifier = dbSettings.ProjectIdentifier;
                    GlobalSettings.ClientName = dbSettings.ClientName;
                    GlobalSettings.Originator = dbSettings.Originator;
                    GlobalSettings.Role = dbSettings.Role;
                }

                //get status and issue formats from database
                GlobalSettings.DocumentStatuses.Clear();
                GlobalSettings.DocumentStatuses = GetDocumentStatuses();

                GlobalSettings.IssueFormats.Clear();
                GlobalSettings.IssueFormats = GetIssueFormats();
            }
        }
    }

    public void UpdateSettings()
    {
        _connection.UpgradeDatabase(GlobalSettings.DatabaseFile);

        string sql = "UPDATE Settings SET " +
            "DateFormatString = @DateFormatString, " +
            "DrawingIssueStore = @DrawingIssueStore, " +
            "IssueSheetStore = @IssueSheetStore, " +
            "ReportStore = @ReportStore, " +
            "DirectoryStore = @DirectoryStore, " +
            "FileNameFilter = @FileNameFilter, " +
            "FileNameFilter2 = @FileNameFilter2, " +
            "ProjectIdentifier = @ProjectIdentifier, " +
            "ProjectNumber = @ProjectNumber, " +           
            "ProjectName = @ProjectName, " +
            "ClientName = @ClientName, " +
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
                FileNameFilter2 = GlobalSettings.FileNameFilter2,
                ProjectIdentifier = GlobalSettings.ProjectIdentifier,
                ProjectNumber = GlobalSettings.ProjectNumber,
                ProjectName = GlobalSettings.ProjectName,
                ClientName = GlobalSettings.ClientName,
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

        SaveIssueFormats();
        SaveDocumentStatuses();
    }

    //TODO save document statuses and issue formats to the database

    private List<IssueFormatModel> GetIssueFormats()
    {
        //build the issue formats list  
        string sql = "SELECT * FROM IssueFormat;";

        List<IssueFormatModel> issueFormats = _connection.LoadData<IssueFormatModel, dynamic>(
            GlobalSettings.DatabaseFile,
            sql, null).ToList();

        return issueFormats;
    }

    private List<DocumentStatusModel> GetDocumentStatuses()
    {
        string sql = "SELECT * FROM DocumentStatus;";

        List<DocumentStatusModel> documentStatuses = _connection.LoadData<DocumentStatusModel, dynamic>(
            GlobalSettings.DatabaseFile,
            sql, null).ToList();

        return documentStatuses;
    } 

    private void SaveIssueFormats()
    {
        //clear the records in the table
        string sql = "DELETE FROM IssueFormat;";
        _connection.SaveData(
            GlobalSettings.DatabaseFile,
            sql, new { });

        //save the new records
        sql = "INSERT INTO IssueFormat (Code, Description) VALUES (@Code, @Description )";
        _connection.SaveData(
    GlobalSettings.DatabaseFile,
    sql, GlobalSettings.IssueFormats);
    }

    private void SaveDocumentStatuses()
    {
        //clear the records in the table
        string sql = "DELETE FROM DocumentStatus;";
        _connection.SaveData(
    GlobalSettings.DatabaseFile,
    sql, new { });

        //save the new records
        sql = "INSERT INTO DocumentStatus  (Code, Description) VALUES (@Code, @Description )";
        _connection.SaveData(
    GlobalSettings.DatabaseFile,
    sql, GlobalSettings.DocumentStatuses);
    }
}
    
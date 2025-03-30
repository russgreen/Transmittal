using Microsoft.Extensions.Logging;
using System.IO;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;

public class SettingsService : ISettingsService
{
    private readonly IDataConnection _connection;
    private readonly ILogger<SettingsService> _logger;  

    public SettingsModel GlobalSettings { get; set; }
    
    public SettingsService(IDataConnection dataConnection,
        ILogger<SettingsService> logger)
    {
        _connection = dataConnection;   
        _logger = logger;
        
        GlobalSettings = new();
    }

    public void GetSettings()
    {
        //this is where additional settings related to desktop.exe and reporting are pulled form the project DB.
        _logger.LogDebug("Getting settings from {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);

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
                    GlobalSettings.DrawingIssueStore2 = dbSettings.DrawingIssueStore2;
                    GlobalSettings.IssueSheetStore = dbSettings.IssueSheetStore;
                    GlobalSettings.ReportStore = dbSettings.ReportStore;
                    GlobalSettings.DirectoryStore = dbSettings.DirectoryStore;
                    GlobalSettings.FileNameFilter = dbSettings.FileNameFilter;

                    GlobalSettings.UseExtranet = dbSettings.UseExtranet;
                    GlobalSettings.UseISO19650 = dbSettings.UseISO19650;

                    GlobalSettings.UseRevit = dbSettings.UseRevit;
                    GlobalSettings.ProjectNumber = dbSettings.ProjectNumber;
                    GlobalSettings.ProjectName = dbSettings.ProjectName;
                    GlobalSettings.ProjectIdentifier = dbSettings.ProjectIdentifier;
                    GlobalSettings.ClientName = dbSettings.ClientName;
                    GlobalSettings.Originator = dbSettings.Originator;
                    GlobalSettings.Role = dbSettings.Role;

                    GlobalSettings.ProjectIdentifierParamGuid = dbSettings.ProjectIdentifierParamGuid;
                    GlobalSettings.OriginatorParamGuid = dbSettings.OriginatorParamGuid;
                    GlobalSettings.RoleParamGuid = dbSettings.RoleParamGuid;

                    GlobalSettings.SheetVolumeParamGuid = dbSettings.SheetVolumeParamGuid;
                    GlobalSettings.SheetLevelParamGuid = dbSettings.SheetLevelParamGuid;
                    GlobalSettings.DocumentTypeParamGuid = dbSettings.DocumentTypeParamGuid;
                    GlobalSettings.SheetStatusParamGuid = dbSettings.SheetStatusParamGuid;
                    GlobalSettings.SheetStatusDescriptionParamGuid = dbSettings.SheetStatusDescriptionParamGuid;
                   
                    //these are new settings, so we need to check if they exists in the database
                    GlobalSettings.SheetPackageParamGuid = dbSettings.SheetPackageParamGuid ?? GlobalSettings.SheetPackageParamGuid;

                    GlobalSettings.DrawingIssueStore2 = dbSettings.DrawingIssueStore2 ?? GlobalSettings.DrawingIssueStore2;
                    GlobalSettings.UseDrawingIssueStore2 = dbSettings.UseDrawingIssueStore2 ? dbSettings.UseDrawingIssueStore2 : GlobalSettings.UseDrawingIssueStore2;
                }

                //get status and issue formats from database
                GlobalSettings.DocumentStatuses.Clear();
                GlobalSettings.DocumentStatuses = GetDocumentStatuses();

                GlobalSettings.IssueFormats.Clear();
                GlobalSettings.IssueFormats = GetIssueFormats();

                _connection.UpgradeDatabase(GlobalSettings.DatabaseFile);

            }
        }

    }

    public void UpdateSettings()
    {
        _logger.LogDebug("Updating settings in {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);

        try
        {
            string sql = "UPDATE Settings SET " +
                "DateFormatString = @DateFormatString, " +
                "DrawingIssueStore = @DrawingIssueStore, " +
                "DrawingIssueStore2 = @DrawingIssueStore2, " +
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
                "UseDrawingIssueStore2 = @UseDrawingIssueStore2, " +
                "UseISO19650 = @UseISO19650, " +
                "UseRevit = @UseRevit, " +
                "Originator = @Originator, " +
                "Role = @Role, " +
                "ProjectIdentifierParamGuid = @ProjectIdentifierParamGuid, " +
                "OriginatorParamGuid = @OriginatorParamGuid, " +
                "RoleParamGuid = @RoleParamGuid, " +
                "SheetVolumeParamGuid = @SheetVolumeParamGuid, " +
                "SheetLevelParamGuid = @SheetLevelParamGuid, " +
                "DocumentTypeParamGuid = @DocumentTypeParamGuid, " +
                "SheetStatusParamGuid = @SheetStatusParamGuid, " +
                "SheetStatusDescriptionParamGuid = @SheetStatusDescriptionParamGuid, " +
                "SheetPackageParamGuid = @SheetPackageParamGuid " +
                "WHERE ID=1;";

            _connection.SaveData(
                GlobalSettings.DatabaseFile,
                sql, new
                {
                    DateFormatString = GlobalSettings.DateFormatString,
                    DrawingIssueStore = GlobalSettings.DrawingIssueStore,
                    DrawingIssueStore2 = GlobalSettings.DrawingIssueStore2,
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
                    UseDrawingIssueStore2 = GlobalSettings.UseDrawingIssueStore2,
                    UseRevit = GlobalSettings.UseRevit,
                    Originator = GlobalSettings.Originator,
                    Role = GlobalSettings.Role,
                    ProjectIdentifierParamGuid = GlobalSettings.ProjectIdentifierParamGuid,
                    OriginatorParamGuid = GlobalSettings.OriginatorParamGuid,
                    RoleParamGuid = GlobalSettings.RoleParamGuid,
                    SheetVolumeParamGuid = GlobalSettings.SheetVolumeParamGuid,
                    SheetLevelParamGuid = GlobalSettings.SheetLevelParamGuid,
                    DocumentTypeParamGuid = GlobalSettings.DocumentTypeParamGuid,
                    SheetStatusParamGuid = GlobalSettings.SheetStatusParamGuid,
                    SheetStatusDescriptionParamGuid = GlobalSettings.SheetStatusDescriptionParamGuid,
                    SheetPackageParamGuid = GlobalSettings.SheetPackageParamGuid
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings in {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);
        }
       

        SaveIssueFormats();
        SaveDocumentStatuses();
    }
    //TODO save document statuses and issue formats to the database

    private List<IssueFormatModel> GetIssueFormats()
    {
        _logger.LogDebug("Getting issue formats from {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);

        //build the issue formats list  
        string sql = "SELECT * FROM IssueFormat;";

        List<IssueFormatModel> issueFormats = _connection.LoadData<IssueFormatModel, dynamic>(
            GlobalSettings.DatabaseFile,
            sql, null).ToList();

        return issueFormats;
    }

    private List<DocumentStatusModel> GetDocumentStatuses()
    {
        _logger.LogDebug("Getting document statuses from {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);

        string sql = "SELECT * FROM DocumentStatus;";

        List<DocumentStatusModel> documentStatuses = _connection.LoadData<DocumentStatusModel, dynamic>(
            GlobalSettings.DatabaseFile,
            sql, null).ToList();

        return documentStatuses;
    } 

    private void SaveIssueFormats()
    {
        _logger.LogDebug("Saving issue formats to {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);

        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving issue formats to {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);
        }

    }

    private void SaveDocumentStatuses()
    {
        _logger.LogDebug("Saving document statuses to {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);

        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving document statuses to {GlobalSettings.DatabaseFile}", GlobalSettings.DatabaseFile);
        }

    }
}
    
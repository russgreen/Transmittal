using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Microsoft.Extensions.Logging;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Services;

internal class SettingsServiceRvt : ISettingsServiceRvt
{
    private const string _dataStorageElementName = "TransmittalSettings";
    private const string _schemaName = "TransmittalAppSettings";
    private const string _schemaGuid = "302151AE-3986-46F5-A172-0E327D0D191E";
    private const string _schemaNameV1 = "TransmittalAppSettingsV1";
    private const string _schemaGuidV1 = "42DCEC84-45AB-4CA1-8C6B-8C4853A23BCF";
    private const string _schemaNameV2 = "TransmittalAppSettingsV2";
    private const string _schemaGuidV2 = "2896AAE5-B7E9-4854-8AC7-8D20FD59C51E";
    private const string _vendorID = "Transmittal";

    // project paramaters
    private const string _projectIdentifierParamGuid = "ce8c18ee-3b90-4f42-8938-ae90e3af5a6a";
    private const string _originatorParamGuid = "e45313b7-8419-4803-92f0-68558f9278b2";
    private const string _roleParamGuid = "67fcb5e8-4ffb-43b8-8ec9-c664fd997267";
    // sheet parameters
    private const string _sheetVolumeParamGuid = "9c16757c-175a-451a-a5d4-c4a6ff291acb";
    private const string _sheetLevelParamGuid = "e51af162-9025-48a0-bd2c-bc833fab0db0";
    private const string _documentTypeParamGuid = "eb57d296-7d9c-459f-ace1-0bdaf95c3b29";
    private const string _sheetStatusParamGuid = "3304f169-ceb9-40b9-a69d-d8f3eb0a3fb9";
    private const string _sheetStatusDescriptionParamGuid = "4effad6a-f05d-43dd-afb1-c2b6c5cb5b9a";
    private const string _sheetPackageParamGuid = "24308d83-9bd6-42cb-a801-d253b37dde03";

    private Autodesk.Revit.DB.ProjectInfo _projectInfo = null;

    private readonly ISettingsService _settingsService;
    private readonly IDataConnection _dataConnection;
    private readonly ILogger<SettingsServiceRvt> _logger;   

    private Schema _oldSchema = null;
    private Schema _oldSchemaV1 = null;
    private Schema _schema = null;

    public SettingsServiceRvt(IDataConnection dataConnection, 
        ISettingsService settingsService,
        ILogger<SettingsServiceRvt> logger)
    {
        _settingsService = settingsService;
        _dataConnection = dataConnection;
        _logger = logger;

        _schema = null;
    }

    public bool GetSettingsRvt(Document rvtDoc)
    {
        //first get the project information 
        _projectInfo = rvtDoc.ProjectInformation;

        //reset the global settings to handle switching between models
        _settingsService.GlobalSettings = new();

        //set the default parameter values
        SetParameters();

        //check for the schema and load data from it if it exists
        if (SchemaExists(_schemaName))
        {
            _oldSchema = GetSchema(_schemaName);
            _logger.LogDebug("Found old schema");
        }
        if (SchemaExists(_schemaNameV1))
        {
            _oldSchemaV1 = GetSchema(_schemaNameV1);
            _logger.LogDebug("Found old schema V1");
        }
        if (SchemaExists(_schemaNameV2))
        {
            _schema = GetSchema(_schemaNameV2);
            _logger.LogDebug("Found schema V2");
        }

        if (_schema != null)
        {
            _logger.LogDebug("Schema exists. Getting settings from {schemeName}", _schema.SchemaName);
            //get the settings from the schema
            //GetSettingsFromSchemaV1();
            GetSettingsFromSchemaV2();
        }
        else
        {
            if(_oldSchema != null)
            {
                //we still have the old schema so get settings from it then delete it
                _logger.LogDebug("Getting settings from {schemeName}", _oldSchema.SchemaName);
                GetSettingsFromSchema();
                DeleteSchema(_oldSchema);
            }
            if (_oldSchemaV1 != null)
            {
                //we still have the old schema so get settings from it then delete it
                _logger.LogDebug("Getting settings from {schemeName}", _oldSchemaV1.SchemaName);
                GetSettingsFromSchemaV1();
                DeleteSchema(_oldSchemaV1);
            }

            //create the schema and load the settings from it
            //CreateSchema();
            CreateAndSaveSchemaToRvt();
            SaveSettingsToSchema(); //saves the default settings only at this point
            GetSettingsFromSchemaV2(); //don't really need this call but its here to help debugging
        }

        //if the value is not empty try and open the database and read the settings
        if (_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            if (CheckDatabaseFileExists(_settingsService.GlobalSettings.DatabaseFile)) 
            {
                _settingsService.GetSettings();
            }
            else
            {
                return false;
            }
        }

        //we always want to load project information from the revit parameters
        //and then save back to the DB for use by the desktop app....revit is primary
        _settingsService.GlobalSettings.ProjectNumber = _projectInfo.Number;
        _settingsService.GlobalSettings.ProjectName = _projectInfo.Name;
        _settingsService.GlobalSettings.ClientName = _projectInfo.ClientName;

        //get some project data from shared parameters
        _settingsService.GlobalSettings.ProjectIdentifier = Util.GetParameterValueString(_projectInfo, _settingsService.GlobalSettings.ProjectIdentifierParamGuid);
        _settingsService.GlobalSettings.Originator = Util.GetParameterValueString(_projectInfo, _settingsService.GlobalSettings.OriginatorParamGuid);
        _settingsService.GlobalSettings.Role = Util.GetParameterValueString(_projectInfo, _settingsService.GlobalSettings.RoleParamGuid);

        return true;
    }

    public void UpdateSettingsRvt()
    {
        SaveSettingsToSchema();
    }

    public void SetParameters()
    {
        // project paramaters
        _settingsService.GlobalSettings.ProjectIdentifierParamGuid = _projectIdentifierParamGuid;
        _settingsService.GlobalSettings.OriginatorParamGuid = _originatorParamGuid;
        _settingsService.GlobalSettings.RoleParamGuid = _roleParamGuid;
        // sheet parameters
        _settingsService.GlobalSettings.SheetVolumeParamGuid = _sheetVolumeParamGuid;
        _settingsService.GlobalSettings.SheetLevelParamGuid = _sheetLevelParamGuid;
        _settingsService.GlobalSettings.DocumentTypeParamGuid = _documentTypeParamGuid;
        _settingsService.GlobalSettings.SheetStatusParamGuid = _sheetStatusParamGuid;
        _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = _sheetStatusDescriptionParamGuid;
        _settingsService.GlobalSettings.SheetPackageParamGuid = _sheetPackageParamGuid;
    }

    public bool CheckDatabaseFileExists(string filepath, bool checkConnection = true)
    {
        if (filepath != "[NONE]" || filepath != null)
        {
            var databaseFile = filepath.ParsePathWithEnvironmentVariables();
            var databaseExists = System.IO.File.Exists(databaseFile);

            if (databaseExists)
            {
                if (checkConnection == true)
                {
                    return _dataConnection.CheckConnection(databaseFile);
                }
            }

            return databaseExists;
        }

        return false;
    }

    private List<IssueFormatModel> GetIssueFormats()
    {
        //build the issue formats list  
        var issueFormats = new List<IssueFormatModel>
        {
            new IssueFormatModel() { Code = "E", Description = "Email" },
            new IssueFormatModel() { Code = "C", Description = "Cloud" },
            new IssueFormatModel() { Code = "P", Description = "Paper" }
        };

        return issueFormats;
    }

    private void DeleteSchema(Schema schema)
    {
        using (Transaction deleteSchema = new Transaction(App.RevitDocument, "TransmittalSettings"))
        {
            deleteSchema.Start();

            DataStorage dataStorageElement = FindDataStorageElement(App.RevitDocument, schema);

            dataStorageElement.DeleteEntity(schema);

            deleteSchema.Commit();

            _logger.LogDebug("Schema {schemaName} deleted", schema.SchemaName);
        }
    }

    private Schema CreateSchema()
    {
        //build the schema
        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid(_schemaGuidV2));
        schemaBuilder.SetReadAccessLevel(AccessLevel.Public); // allow anyone to read the object
        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public); // TODO why does it not work when we restrict writing to this vendor only
        schemaBuilder.SetVendorId(_vendorID); // required because of restricted write-access
        schemaBuilder.SetSchemaName(_schemaNameV2);

        FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.FileNameFilter), typeof(string));
        fieldBuilder.SetDocumentation("The filename filter rule for transmittal exports");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.FileNameFilter2), typeof(string));
        fieldBuilder.SetDocumentation("The filename filter rule for transmittal export copies to extranet");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DrawingIssueStore), typeof(string));
        fieldBuilder.SetDocumentation("The location to save the transmittal exports");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.UseISO19650), typeof(bool));
        fieldBuilder.SetDocumentation("Use the ISO19650 for transmittal exports");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.UseExtranet), typeof(bool));
        fieldBuilder.SetDocumentation("Use the extranet for transmittal exports");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DateFormatString), typeof(string));
        fieldBuilder.SetDocumentation("The date format string for revisions exports");


        fieldBuilder = schemaBuilder.AddMapField(nameof(_settingsService.GlobalSettings.IssueFormats), typeof(string), typeof(string));
        fieldBuilder.SetDocumentation("The issue formats for transmittal exports");

        fieldBuilder = schemaBuilder.AddMapField(nameof(_settingsService.GlobalSettings.DocumentStatuses), typeof(string), typeof(string));
        fieldBuilder.SetDocumentation("The document statuses for transmittal exports");


        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.RecordTransmittals), typeof(bool));
        fieldBuilder.SetDocumentation("Record transmittals in the database");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DatabaseTemplateFile), typeof(string));
        fieldBuilder.SetDocumentation("The location of the template database");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DatabaseFile), typeof(string));
        fieldBuilder.SetDocumentation("The location of the database");


        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.IssueSheetStore), typeof(string));
        fieldBuilder.SetDocumentation("The location to save the transmittal reports");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DirectoryStore), typeof(string));
        fieldBuilder.SetDocumentation("The location to save the directory reports");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.ReportStore), typeof(string));
        fieldBuilder.SetDocumentation("The location of customised report templates");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters), typeof(bool));
        fieldBuilder.SetDocumentation("Use custom shared parameters");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The project identifier parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The originator parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.RoleParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The role parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The sheet volume parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The sheet level parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The document type parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The sheet status parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The sheet status description parameter guid");

        fieldBuilder = schemaBuilder.AddSimpleField(nameof(_settingsService.GlobalSettings.SheetPackageParamGuid), typeof(string));
        fieldBuilder.SetDocumentation("The sheet work package parameter guid");

        _schema = schemaBuilder.Finish(); // register the Schema object

        return _schema;
    }

    private void CreateAndSaveSchemaToRvt()
    {
        try
        {
            _schema = CreateSchema(); 
            DataStorage dataStorageElement = FindDataStorageElement(App.RevitDocument, _schema);
        
            using (Transaction createSchema = new Transaction(App.RevitDocument, "TransmittalSettings"))
            {
                createSchema.Start();

                if (dataStorageElement == null)
                {
                    dataStorageElement = DataStorage.Create(App.RevitDocument);
                    dataStorageElement.Name = _dataStorageElementName;
                }

                Entity entity = new Entity(_schema); // create an entity (object) for this schema (class)

                dataStorageElement.SetEntity(entity);

                createSchema.Commit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schema");
        }
    }
        
    private void SaveSettingsToSchema()
    {
        try
        {
            _schema = GetSchema(_schemaNameV2);
            DataStorage dataStorageElement = FindDataStorageElement(App.RevitDocument, _schema);

            using (Transaction storeData = new Transaction(App.RevitDocument, "TransmittalSettings"))
            {
                storeData.Start();

                Entity entity = dataStorageElement.GetEntity(_schema);

                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter)), _settingsService.GlobalSettings.FileNameFilter);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter2)), _settingsService.GlobalSettings.FileNameFilter2);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DrawingIssueStore)), _settingsService.GlobalSettings.DrawingIssueStore);
                entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.UseISO19650)), _settingsService.GlobalSettings.UseISO19650);
                entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.UseExtranet)), _settingsService.GlobalSettings.UseExtranet);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DateFormatString)), _settingsService.GlobalSettings.DateFormatString);

                entity.Set<IDictionary<string, string>>(_schema.GetField(nameof(_settingsService.GlobalSettings.IssueFormats)),
                    ListOfIssueFormatToDictionary(_settingsService.GlobalSettings.IssueFormats));
                entity.Set<IDictionary<string, string>>(_schema.GetField(nameof(_settingsService.GlobalSettings.DocumentStatuses)),
                    ListOfDocumentStatusToDictionary(_settingsService.GlobalSettings.DocumentStatuses));

                entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.RecordTransmittals)), _settingsService.GlobalSettings.RecordTransmittals);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseFile)), _settingsService.GlobalSettings.DatabaseFile);

                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.IssueSheetStore)), _settingsService.GlobalSettings.IssueSheetStore);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DirectoryStore)), _settingsService.GlobalSettings.DirectoryStore);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.ReportStore)), _settingsService.GlobalSettings.ReportStore);

                entity.Set<bool>(_schema.GetField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters)), _settingsService.GlobalSettings.UseCustomSharedParameters);

                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid)), _settingsService.GlobalSettings.ProjectIdentifierParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid)), _settingsService.GlobalSettings.OriginatorParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.RoleParamGuid)), _settingsService.GlobalSettings.RoleParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid)), _settingsService.GlobalSettings.SheetVolumeParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid)), _settingsService.GlobalSettings.SheetLevelParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid)), _settingsService.GlobalSettings.DocumentTypeParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid)), _settingsService.GlobalSettings.SheetStatusParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)), _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid);
                entity.Set<string>(_schema.GetField(nameof(_settingsService.GlobalSettings.SheetPackageParamGuid)), _settingsService.GlobalSettings.SheetPackageParamGuid);

                dataStorageElement.SetEntity(entity);

                storeData.Commit();
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving settings to schema");
        }
    }
    
    private void GetSettingsFromSchema()
    {
        Schema schema = GetSchema(_schemaName);
        DataStorage dataStorageElement = FindDataStorageElement(App.RevitDocument, schema);
        Entity entity = dataStorageElement.GetEntity(_oldSchema);

        _settingsService.GlobalSettings.FileNameFilter = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter)));
        _settingsService.GlobalSettings.DrawingIssueStore = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DrawingIssueStore)));

        _settingsService.GlobalSettings.UseISO19650 = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.UseISO19650)));
        _settingsService.GlobalSettings.UseExtranet = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.UseExtranet)));

        _settingsService.GlobalSettings.DateFormatString = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DateFormatString)));

        _settingsService.GlobalSettings.IssueFormats = DictionaryToListOfIssueFormat(
            entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.IssueFormats))));
        _settingsService.GlobalSettings.DocumentStatuses = DictionaryToListOfDocumentStatus(
    entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.DocumentStatuses))));

        _settingsService.GlobalSettings.RecordTransmittals = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.RecordTransmittals)));        
        _settingsService.GlobalSettings.DatabaseFile = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseFile)));

        _settingsService.GlobalSettings.UseCustomSharedParameters = entity.Get<bool>(
            schema.GetField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters)));

        _settingsService.GlobalSettings.ProjectIdentifierParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid)));
        _settingsService.GlobalSettings.OriginatorParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid)));
        _settingsService.GlobalSettings.RoleParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.RoleParamGuid)));
        _settingsService.GlobalSettings.SheetVolumeParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid)));
        _settingsService.GlobalSettings.SheetLevelParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid)));
        _settingsService.GlobalSettings.DocumentTypeParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid)));
        _settingsService.GlobalSettings.SheetStatusParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid)));
        _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = entity.Get<string>(
            schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)));
    }

    private void GetSettingsFromSchemaV1()
    {
        try
        {
            Schema schema = GetSchema(_schemaNameV1);
            DataStorage dataStorageElement = FindDataStorageElement(App.RevitDocument, schema);
            Entity entity = dataStorageElement.GetEntity(_oldSchemaV1);

            _settingsService.GlobalSettings.FileNameFilter = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter)));
            _settingsService.GlobalSettings.FileNameFilter2 = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter2)));
            _settingsService.GlobalSettings.DrawingIssueStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DrawingIssueStore)));

            _settingsService.GlobalSettings.UseISO19650 = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.UseISO19650)));
            _settingsService.GlobalSettings.UseExtranet = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.UseExtranet)));

            _settingsService.GlobalSettings.DateFormatString = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DateFormatString)));

            _settingsService.GlobalSettings.IssueFormats = DictionaryToListOfIssueFormat(
                entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.IssueFormats))));
            _settingsService.GlobalSettings.DocumentStatuses = DictionaryToListOfDocumentStatus(
        entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.DocumentStatuses))));

            _settingsService.GlobalSettings.RecordTransmittals = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.RecordTransmittals)));
            _settingsService.GlobalSettings.DatabaseFile = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseFile)));

            _settingsService.GlobalSettings.IssueSheetStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.IssueSheetStore)));
            _settingsService.GlobalSettings.DirectoryStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DirectoryStore)));
            _settingsService.GlobalSettings.ReportStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.ReportStore)));

            _settingsService.GlobalSettings.UseCustomSharedParameters = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters)));

            _settingsService.GlobalSettings.ProjectIdentifierParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid)));
            _settingsService.GlobalSettings.OriginatorParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid)));
            _settingsService.GlobalSettings.RoleParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.RoleParamGuid)));
            _settingsService.GlobalSettings.SheetVolumeParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid)));
            _settingsService.GlobalSettings.SheetLevelParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid)));
            _settingsService.GlobalSettings.DocumentTypeParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid)));
            _settingsService.GlobalSettings.SheetStatusParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid)));
            _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings from schema V1");
        }

    }

    private void GetSettingsFromSchemaV2()
    {
        try
        {
            Schema schema = GetSchema(_schemaNameV2);
            DataStorage dataStorageElement = FindDataStorageElement(App.RevitDocument, schema);
            Entity entity = dataStorageElement.GetEntity(_schema);

            _settingsService.GlobalSettings.FileNameFilter = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter)));
            _settingsService.GlobalSettings.FileNameFilter2 = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.FileNameFilter2)));
            _settingsService.GlobalSettings.DrawingIssueStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DrawingIssueStore)));

            _settingsService.GlobalSettings.UseISO19650 = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.UseISO19650)));
            _settingsService.GlobalSettings.UseExtranet = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.UseExtranet)));

            _settingsService.GlobalSettings.DateFormatString = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DateFormatString)));

            _settingsService.GlobalSettings.IssueFormats = DictionaryToListOfIssueFormat(
                entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.IssueFormats))));
            _settingsService.GlobalSettings.DocumentStatuses = DictionaryToListOfDocumentStatus(
        entity.Get<IDictionary<string, string>>(schema.GetField(nameof(_settingsService.GlobalSettings.DocumentStatuses))));

            _settingsService.GlobalSettings.RecordTransmittals = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.RecordTransmittals)));
            _settingsService.GlobalSettings.DatabaseFile = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DatabaseFile)));

            _settingsService.GlobalSettings.IssueSheetStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.IssueSheetStore)));
            _settingsService.GlobalSettings.DirectoryStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DirectoryStore)));
            _settingsService.GlobalSettings.ReportStore = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.ReportStore)));

            _settingsService.GlobalSettings.UseCustomSharedParameters = entity.Get<bool>(
                schema.GetField(nameof(_settingsService.GlobalSettings.UseCustomSharedParameters)));

            _settingsService.GlobalSettings.ProjectIdentifierParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.ProjectIdentifierParamGuid)));
            _settingsService.GlobalSettings.OriginatorParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.OriginatorParamGuid)));
            _settingsService.GlobalSettings.RoleParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.RoleParamGuid)));
            _settingsService.GlobalSettings.SheetVolumeParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetVolumeParamGuid)));
            _settingsService.GlobalSettings.SheetLevelParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetLevelParamGuid)));
            _settingsService.GlobalSettings.DocumentTypeParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.DocumentTypeParamGuid)));
            _settingsService.GlobalSettings.SheetStatusParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusParamGuid)));
            _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)));
            _settingsService.GlobalSettings.SheetPackageParamGuid = entity.Get<string>(
                schema.GetField(nameof(_settingsService.GlobalSettings.SheetPackageParamGuid)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings from schema V2");
        }
        
    }

    private Schema GetSchema(string schemaName)
    {
        Schema s = Schema.ListSchemas().FirstOrDefault(q => q.SchemaName == schemaName);
        _logger.LogDebug("Schema {schemaName} found: {s}", s.SchemaName, s);

        return s;
    }

    private bool SchemaExists(string schemaName)
    {
        IList<Schema> schemas = Schema.ListSchemas();
        if (schemas.Count == 0)
        {
             return false;           
        }
        else
        {
            foreach (Schema schema in schemas)
            {
                if(schema.SchemaName == schemaName)
                {
                    List<ElementId> ids = ElementsWithStorage(App.RevitDocument, schema);
                    if (ids.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    private DataStorage FindDataStorageElement(Document doc, Schema schema)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.OfClass(typeof(DataStorage));
        collector.WherePasses(new ExtensibleStorageFilter(schema.GUID));

        return collector.FirstElement() as DataStorage;
    }

    private List<ElementId> ElementsWithStorage(Document doc, Schema schema)
    {
        List<ElementId> ids = new List<ElementId>();
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.WherePasses(new ExtensibleStorageFilter(schema.GUID));
        ids.AddRange(collector.ToElementIds());
        return ids;
    }   

    private IDictionary<string, string> ListOfIssueFormatToDictionary(List<IssueFormatModel> listToConvert)
    {
        IDictionary<string, string> dictionary = new Dictionary<string, string>();

        foreach (IssueFormatModel item in listToConvert)
        {
            dictionary.Add(item.Code ?? string.Empty, item.Description ?? string.Empty);
        }

        return dictionary;
    }

    private IDictionary<string, string> ListOfDocumentStatusToDictionary(List<DocumentStatusModel> listToConvert)
    {
        IDictionary<string, string> dictionary = new Dictionary<string, string>();

        foreach (DocumentStatusModel item in listToConvert)
        {
            dictionary.Add(item.Code ?? string.Empty, item.Description ?? string.Empty);
        }

        return dictionary;
    }    

    private List<IssueFormatModel> DictionaryToListOfIssueFormat(IDictionary<string, string> dictionaryToConvert)
    {
        List<IssueFormatModel> list = new();

        foreach (KeyValuePair<string, string> item in dictionaryToConvert)
        {
            list.Add(new IssueFormatModel(item.Key, item.Value));
        }

        return list;
    }

    private List<DocumentStatusModel> DictionaryToListOfDocumentStatus(IDictionary<string, string> dictionaryToConvert)
    {
        List<DocumentStatusModel> list = new();

        foreach (KeyValuePair<string, string> item in dictionaryToConvert)
        {
            list.Add(new DocumentStatusModel(item.Key, item.Value));
        }

        return list;
    }    
}
    
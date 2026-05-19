using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Extensions;
using System.IO;
using Transmittal.Exceptions;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Enums;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

namespace Transmittal.Services;

internal class SettingsServiceRvtV2 : ISettingsServiceRvt
{
    private const string _dataStorageElementName = "TransmittalSettings";
    private const string _vendorId = "Transmittal";
    private const string _schemaNamePrefix = "TransmittalAppSettings";


    private static readonly SchemaVersionInfo[] _knownSchemas =
    [
        new(0, "TransmittalAppSettings", "302151AE-3986-46F5-A172-0E327D0D191E"),
        new(1, "TransmittalAppSettingsV1", "42DCEC84-45AB-4CA1-8C6B-8C4853A23BCF"),
        new(2, "TransmittalAppSettingsV2", "2896AAE5-B7E9-4854-8AC7-8D20FD59C51E"),
        new(3, "TransmittalAppSettingsV3", "8D2FD527-CF54-45DB-9248-C65F6729D18B"),
        new(4, "TransmittalAppSettingsV4", "5A3671ED-90E7-48B3-8BC1-D2C37CF31D5A"),
    ];

    private static readonly SchemaVersionInfo _latestSchema = _knownSchemas[_knownSchemas.Length - 1];

    // project parameters
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

    private static readonly (string Name, Type Type, string Documentation)[] _simpleFields =
    [
        (nameof(SettingsModel.FileNameFilter), typeof(string), "The filename filter rule for transmittal exports"),
        (nameof(SettingsModel.FileNameFilter2), typeof(string), "The filename filter rule for transmittal export copies to extranet"),
        (nameof(SettingsModel.DrawingIssueStore), typeof(string), "The location to save the transmittal exports"),
        (nameof(SettingsModel.DrawingIssueStore2), typeof(string), "The secondary location to save the transmittal exports"),
        (nameof(SettingsModel.UseISO19650), typeof(bool), "Use the ISO19650 for transmittal exports"),
        (nameof(SettingsModel.UseExtranet), typeof(bool), "Use the extranet for transmittal exports"),
        (nameof(SettingsModel.UseDrawingIssueStore2), typeof(bool), "Use the secondary store for transmittal exports"),
        (nameof(SettingsModel.DateFormatString), typeof(string), "The date format string for revisions exports"),
        (nameof(SettingsModel.RecordTransmittals), typeof(bool), "Record transmittals in the database"),
        (nameof(SettingsModel.DatabaseTemplateFile), typeof(string), "The location of the template database"),
        (nameof(SettingsModel.DatabaseFile), typeof(string), "The location of the database"),
        (nameof(SettingsModel.IssueSheetStore), typeof(string), "The location to save the transmittal reports"),
        (nameof(SettingsModel.DirectoryStore), typeof(string), "The location to save the directory reports"),
        (nameof(SettingsModel.ReportStore), typeof(string), "The location of customised report templates"),
        (nameof(SettingsModel.UseCustomSharedParameters), typeof(bool), "Use custom shared parameters"),
        (nameof(SettingsModel.ProjectIdentifierParamGuid), typeof(string), "The project identifier parameter guid"),
        (nameof(SettingsModel.OriginatorParamGuid), typeof(string), "The originator parameter guid"),
        (nameof(SettingsModel.RoleParamGuid), typeof(string), "The role parameter guid"),
        (nameof(SettingsModel.SheetVolumeParamGuid), typeof(string), "The sheet volume parameter guid"),
        (nameof(SettingsModel.SheetLevelParamGuid), typeof(string), "The sheet level parameter guid"),
        (nameof(SettingsModel.DocumentTypeParamGuid), typeof(string), "The document type parameter guid"),
        (nameof(SettingsModel.SheetStatusParamGuid), typeof(string), "The sheet status parameter guid"),
        (nameof(SettingsModel.SheetStatusDescriptionParamGuid), typeof(string), "The sheet status description parameter guid"),
        (nameof(SettingsModel.SheetPackageParamGuid), typeof(string), "The sheet work package parameter guid"),
        (nameof(SettingsModel.ShowFileTransfer), typeof(bool), "Show file transfer controls in transmittal workflow"),
        (nameof(SettingsModel.FileTransferType), typeof(int), "Preferred transfer service enum value"),
        (nameof(SettingsModel.ProjectDirectoryDocumentTypeCode), typeof(string), "Project directory report document type code"),
        (nameof(SettingsModel.ProjectDirectoryFirstNumber), typeof(string), "Project directory report first number"),
        (nameof(SettingsModel.ProjectDirectoryVolume), typeof(string), "Project directory report volume or functional code"),
        (nameof(SettingsModel.ProjectDirectoryLevel), typeof(string), "Project directory report level or spatial code"),
        (nameof(SettingsModel.TransmittalSheetDocumentTypeCode), typeof(string), "Transmittal sheet report document type code"),
        (nameof(SettingsModel.TransmittalSheetFirstNumber), typeof(string), "Transmittal sheet report first number"),
        (nameof(SettingsModel.TransmittalSheetVolume), typeof(string), "Transmittal sheet report volume or functional code"),
        (nameof(SettingsModel.TransmittalSheetLevel), typeof(string), "Transmittal sheet report level or spatial code"),
        (nameof(SettingsModel.TransmittalSummaryDocumentTypeCode), typeof(string), "Transmittal summary report document type code"),
        (nameof(SettingsModel.TransmittalSummaryFirstNumber), typeof(string), "Transmittal summary report first number"),
        (nameof(SettingsModel.TransmittalSummaryVolume), typeof(string), "Transmittal summary report volume or functional code"),
        (nameof(SettingsModel.TransmittalSummaryLevel), typeof(string), "Transmittal summary report level or spatial code"),
        (nameof(SettingsModel.MasterDocumentsListDocumentTypeCode), typeof(string), "Master documents list report document type code"),
        (nameof(SettingsModel.MasterDocumentsListFirstNumber), typeof(string), "Master documents list report first number"),
        (nameof(SettingsModel.MasterDocumentsListVolume), typeof(string), "Master documents list report volume or functional code"),
        (nameof(SettingsModel.MasterDocumentsListLevel), typeof(string), "Master documents list report level or spatial code"),
    ];

    private static readonly (string Name, string Documentation)[] _mapFields =
    [
        (nameof(SettingsModel.IssueFormats), "The issue formats for transmittal exports"),
        (nameof(SettingsModel.DocumentStatuses), "The document statuses for transmittal exports"),
    ];

    private readonly ISettingsService _settingsService;
    private readonly IDataConnection _dataConnection;
    private readonly ILogger<SettingsServiceRvtV2> _logger;
    private readonly IMessageBoxService _messageBox;

    public SettingsServiceRvtV2(
        IDataConnection dataConnection,
        ISettingsService settingsService,
        ILogger<SettingsServiceRvtV2> logger,
        IMessageBoxService messageBox)
    {
        _settingsService = settingsService;
        _dataConnection = dataConnection;
        _logger = logger;
        _messageBox = messageBox;
    }

    public bool GetSettingsRvt(Document rvtDoc)
    {
        _settingsService.GlobalSettings = new SettingsModel();
        SetParameters();

        EnsureSchemaVersionSupported();

        var loadedVersion = TryLoadSettingsFromExistingSchema(rvtDoc);
        if (loadedVersion < 0)
        {
            EnsureLatestSchemaAndStorage(rvtDoc);
            SaveSettingsToSchemaInternal(rvtDoc);
        }
        else if (loadedVersion < _latestSchema.Version)
        {
            DeleteLegacySchemas(rvtDoc);
            EnsureLatestSchemaAndStorage(rvtDoc);
            SaveSettingsToSchemaInternal(rvtDoc);
        }

        if (!ValidateExternalDependencies())
        {
            return false;
        }

        SyncProjectInfoFromRevit(rvtDoc.ProjectInformation);
        return true;
    }

    public void UpdateSettingsRvt()
    {
        SaveSettingsToSchemaInternal(App.RevitDocument);
    }

    public void SetParameters()
    {
        var settings = _settingsService.GlobalSettings;

        settings.ProjectIdentifierParamGuid = _projectIdentifierParamGuid;
        settings.OriginatorParamGuid = _originatorParamGuid;
        settings.RoleParamGuid = _roleParamGuid;
        settings.SheetVolumeParamGuid = _sheetVolumeParamGuid;
        settings.SheetLevelParamGuid = _sheetLevelParamGuid;
        settings.DocumentTypeParamGuid = _documentTypeParamGuid;
        settings.SheetStatusParamGuid = _sheetStatusParamGuid;
        settings.SheetStatusDescriptionParamGuid = _sheetStatusDescriptionParamGuid;
        settings.SheetPackageParamGuid = _sheetPackageParamGuid;
    }

    public bool CheckDatabaseFileExists(string filepath, bool checkConnection = true)
    {
        if (string.IsNullOrWhiteSpace(filepath) || filepath == "[NONE]")
        {
            return false;
        }

        var databaseFile = filepath.ParsePathWithEnvironmentVariables();
        if (string.IsNullOrWhiteSpace(databaseFile) || !File.Exists(databaseFile))
        {
            return false;
        }

        return !checkConnection || _dataConnection.CheckConnection(databaseFile);
    }

    private int TryLoadSettingsFromExistingSchema(Document document)
    {
        if (TryLoadSettingsFromSchema(document, _latestSchema))
        {
            return _latestSchema.Version;
        }

        for (var i = _knownSchemas.Length - 2; i >= 0; i--)
        {
            var version = _knownSchemas[i];
            if (TryLoadSettingsFromSchema(document, version))
            {
                return version.Version;
            }
        }

        return -1;
    }

    private bool TryLoadSettingsFromSchema(Document document, SchemaVersionInfo version)
    {
        var schema = GetSchema(version.Guid);
        if (schema == null || !SchemaHasStorage(document, schema))
        {
            return false;
        }

        var dataStorage = FindDataStorageElement(document, schema);
        if (dataStorage == null)
        {
            return false;
        }

        _logger.LogDebug("Loading settings from schema {SchemaName}", schema.SchemaName);
        var entity = dataStorage.GetEntity(schema);
        LoadSettingsFromEntity(schema, entity);
        return true;
    }

    private void LoadSettingsFromEntity(Schema schema, Entity entity)
    {
        var settings = _settingsService.GlobalSettings;

        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.FileNameFilter), value => settings.FileNameFilter = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.FileNameFilter2), value => settings.FileNameFilter2 = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DrawingIssueStore), value => settings.DrawingIssueStore = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DrawingIssueStore2), value => settings.DrawingIssueStore2 = value);
        ReadIfPresent<bool>(schema, entity, nameof(SettingsModel.UseISO19650), value => settings.UseISO19650 = value);
        ReadIfPresent<bool>(schema, entity, nameof(SettingsModel.UseExtranet), value => settings.UseExtranet = value);
        ReadIfPresent<bool>(schema, entity, nameof(SettingsModel.UseDrawingIssueStore2), value => settings.UseDrawingIssueStore2 = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DateFormatString), value => settings.DateFormatString = value);

        ReadIfPresent<IDictionary<string, string>>(schema, entity, nameof(SettingsModel.IssueFormats), value =>
            settings.IssueFormats = DictionaryToList(value, static (code, description) => new IssueFormatModel(code, description)));
        ReadIfPresent<IDictionary<string, string>>(schema, entity, nameof(SettingsModel.DocumentStatuses), value =>
            settings.DocumentStatuses = DictionaryToList(value, static (code, description) => new DocumentStatusModel(code, description)));

        ReadIfPresent<bool>(schema, entity, nameof(SettingsModel.RecordTransmittals), value => settings.RecordTransmittals = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DatabaseFile), value => settings.DatabaseFile = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DatabaseTemplateFile), value => settings.DatabaseTemplateFile = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.IssueSheetStore), value => settings.IssueSheetStore = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DirectoryStore), value => settings.DirectoryStore = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.ReportStore), value => settings.ReportStore = value);

        ReadIfPresent<bool>(schema, entity, nameof(SettingsModel.UseCustomSharedParameters), value => settings.UseCustomSharedParameters = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.ProjectIdentifierParamGuid), value => settings.ProjectIdentifierParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.OriginatorParamGuid), value => settings.OriginatorParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.RoleParamGuid), value => settings.RoleParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.SheetVolumeParamGuid), value => settings.SheetVolumeParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.SheetLevelParamGuid), value => settings.SheetLevelParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.DocumentTypeParamGuid), value => settings.DocumentTypeParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.SheetStatusParamGuid), value => settings.SheetStatusParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.SheetStatusDescriptionParamGuid), value => settings.SheetStatusDescriptionParamGuid = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.SheetPackageParamGuid), value => settings.SheetPackageParamGuid = value);
        ReadIfPresent<bool>(schema, entity, nameof(SettingsModel.ShowFileTransfer), value => settings.ShowFileTransfer = value);

        ReadIfPresent<int>(schema, entity, nameof(SettingsModel.FileTransferType), value =>
        {
            if (Enum.IsDefined(typeof(FileTransferType), value))
            {
                settings.FileTransferType = (FileTransferType)value;
            }
        });

        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.ProjectDirectoryDocumentTypeCode), value => settings.ProjectDirectoryDocumentTypeCode = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.ProjectDirectoryFirstNumber), value => settings.ProjectDirectoryFirstNumber = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.ProjectDirectoryVolume), value => settings.ProjectDirectoryVolume = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.ProjectDirectoryLevel), value => settings.ProjectDirectoryLevel = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSheetDocumentTypeCode), value => settings.TransmittalSheetDocumentTypeCode = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSheetFirstNumber), value => settings.TransmittalSheetFirstNumber = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSheetVolume), value => settings.TransmittalSheetVolume = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSheetLevel), value => settings.TransmittalSheetLevel = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSummaryDocumentTypeCode), value => settings.TransmittalSummaryDocumentTypeCode = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSummaryFirstNumber), value => settings.TransmittalSummaryFirstNumber = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSummaryVolume), value => settings.TransmittalSummaryVolume = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.TransmittalSummaryLevel), value => settings.TransmittalSummaryLevel = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.MasterDocumentsListDocumentTypeCode), value => settings.MasterDocumentsListDocumentTypeCode = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.MasterDocumentsListFirstNumber), value => settings.MasterDocumentsListFirstNumber = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.MasterDocumentsListVolume), value => settings.MasterDocumentsListVolume = value);
        ReadIfPresent<string>(schema, entity, nameof(SettingsModel.MasterDocumentsListLevel), value => settings.MasterDocumentsListLevel = value);
    }

    private void SaveSettingsToSchemaInternal(Document document)
    {
        var schema = EnsureLatestSchemaAndStorage(document);
        var dataStorage = FindDataStorageElement(document, schema);
        if (dataStorage == null)
        {
            throw new InvalidOperationException($"Could not find {_dataStorageElementName} data storage for schema {schema.SchemaName}.");
        }

        var settings = _settingsService.GlobalSettings;

        using var transaction = new Transaction(document, _dataStorageElementName);
        transaction.Start();

        var entity = new Entity(schema);
        WriteIfPresent(schema, entity, nameof(SettingsModel.FileNameFilter), settings.FileNameFilter);
        WriteIfPresent(schema, entity, nameof(SettingsModel.FileNameFilter2), settings.FileNameFilter2);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DrawingIssueStore), settings.DrawingIssueStore);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DrawingIssueStore2), settings.DrawingIssueStore2);
        WriteIfPresent(schema, entity, nameof(SettingsModel.UseISO19650), settings.UseISO19650);
        WriteIfPresent(schema, entity, nameof(SettingsModel.UseExtranet), settings.UseExtranet);
        WriteIfPresent(schema, entity, nameof(SettingsModel.UseDrawingIssueStore2), settings.UseDrawingIssueStore2);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DateFormatString), settings.DateFormatString);
        WriteIfPresent(schema, entity, nameof(SettingsModel.IssueFormats), ListToDictionary(settings.IssueFormats, static item => item.Code, static item => item.Description));
        WriteIfPresent(schema, entity, nameof(SettingsModel.DocumentStatuses), ListToDictionary(settings.DocumentStatuses, static item => item.Code, static item => item.Description));
        WriteIfPresent(schema, entity, nameof(SettingsModel.RecordTransmittals), settings.RecordTransmittals);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DatabaseTemplateFile), settings.DatabaseTemplateFile);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DatabaseFile), settings.DatabaseFile);
        WriteIfPresent(schema, entity, nameof(SettingsModel.IssueSheetStore), settings.IssueSheetStore);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DirectoryStore), settings.DirectoryStore);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ReportStore), settings.ReportStore);
        WriteIfPresent(schema, entity, nameof(SettingsModel.UseCustomSharedParameters), settings.UseCustomSharedParameters);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ProjectIdentifierParamGuid), settings.ProjectIdentifierParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.OriginatorParamGuid), settings.OriginatorParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.RoleParamGuid), settings.RoleParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.SheetVolumeParamGuid), settings.SheetVolumeParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.SheetLevelParamGuid), settings.SheetLevelParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.DocumentTypeParamGuid), settings.DocumentTypeParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.SheetStatusParamGuid), settings.SheetStatusParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.SheetStatusDescriptionParamGuid), settings.SheetStatusDescriptionParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.SheetPackageParamGuid), settings.SheetPackageParamGuid);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ShowFileTransfer), settings.ShowFileTransfer);
        WriteIfPresent(schema, entity, nameof(SettingsModel.FileTransferType), (int)settings.FileTransferType);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ProjectDirectoryDocumentTypeCode), settings.ProjectDirectoryDocumentTypeCode);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ProjectDirectoryFirstNumber), settings.ProjectDirectoryFirstNumber);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ProjectDirectoryVolume), settings.ProjectDirectoryVolume);
        WriteIfPresent(schema, entity, nameof(SettingsModel.ProjectDirectoryLevel), settings.ProjectDirectoryLevel);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSheetDocumentTypeCode), settings.TransmittalSheetDocumentTypeCode);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSheetFirstNumber), settings.TransmittalSheetFirstNumber);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSheetVolume), settings.TransmittalSheetVolume);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSheetLevel), settings.TransmittalSheetLevel);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSummaryDocumentTypeCode), settings.TransmittalSummaryDocumentTypeCode);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSummaryFirstNumber), settings.TransmittalSummaryFirstNumber);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSummaryVolume), settings.TransmittalSummaryVolume);
        WriteIfPresent(schema, entity, nameof(SettingsModel.TransmittalSummaryLevel), settings.TransmittalSummaryLevel);
        WriteIfPresent(schema, entity, nameof(SettingsModel.MasterDocumentsListDocumentTypeCode), settings.MasterDocumentsListDocumentTypeCode);
        WriteIfPresent(schema, entity, nameof(SettingsModel.MasterDocumentsListFirstNumber), settings.MasterDocumentsListFirstNumber);
        WriteIfPresent(schema, entity, nameof(SettingsModel.MasterDocumentsListVolume), settings.MasterDocumentsListVolume);
        WriteIfPresent(schema, entity, nameof(SettingsModel.MasterDocumentsListLevel), settings.MasterDocumentsListLevel);

        dataStorage.SetEntity(entity);
        transaction.Commit();
    }

    private Schema EnsureLatestSchemaAndStorage(Document document)
    {
        var schema = GetSchema(_latestSchema.Guid);
        if (schema == null)
        {
            schema = CreateLatestSchema();
        }

        var dataStorage = FindDataStorageElement(document, schema);
        if (dataStorage != null)
        {
            return schema;
        }

        using var transaction = new Transaction(document, _dataStorageElementName);
        transaction.Start();
        dataStorage = DataStorage.Create(document);
        dataStorage.Name = _dataStorageElementName;
        dataStorage.SetEntity(new Entity(schema));
        transaction.Commit();

        return schema;
    }

    private Schema CreateLatestSchema()
    {
        var schemaBuilder = new SchemaBuilder(_latestSchema.Guid);
        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
        schemaBuilder.SetVendorId(_vendorId);
        schemaBuilder.SetSchemaName(_latestSchema.Name);

        foreach (var field in _simpleFields)
        {
            var fieldBuilder = schemaBuilder.AddSimpleField(field.Name, field.Type);
            fieldBuilder.SetDocumentation(field.Documentation);
        }

        foreach (var field in _mapFields)
        {
            var fieldBuilder = schemaBuilder.AddMapField(field.Name, typeof(string), typeof(string));
            fieldBuilder.SetDocumentation(field.Documentation);
        }

        return schemaBuilder.Finish();
    }

    private void DeleteLegacySchemas(Document document)
    {
        for (var i = 0; i < _knownSchemas.Length - 1; i++)
        {
            var schema = GetSchema(_knownSchemas[i].Guid);
            if (schema == null || !SchemaHasStorage(document, schema))
            {
                continue;
            }

            var dataStorage = FindDataStorageElement(document, schema);
            if (dataStorage == null)
            {
                continue;
            }

            using var transaction = new Transaction(document, _dataStorageElementName);
            transaction.Start();
            dataStorage.DeleteEntity(schema);
            transaction.Commit();
            _logger.LogDebug("Deleted schema entity {SchemaName}", schema.SchemaName);
        }
    }

    private void EnsureSchemaVersionSupported()
    {
        var newerSchemaVersion = DetectNewerSchemas();
        if (newerSchemaVersion <= _latestSchema.Version)
        {
            return;
        }

        _logger.LogWarning(
            "Transmittal settings schema version {NewerVersion} detected, but application only supports up to version {LatestVersion}",
            newerSchemaVersion,
            _latestSchema.Version);

        _messageBox.ShowOk(
            "Application version",
            "You appear to be opening a Revit file which was created or edited with a newer version of Transmittal. Please check for software updates.");

        throw new SchemaVersionTooNewException(newerSchemaVersion, _latestSchema.Version);
    }

    private int DetectNewerSchemas()
    {
        var newerSchemaVersion = -1;
        var schemas = Schema.ListSchemas();

        foreach (var schema in schemas)
        {
            if (!string.Equals(schema.VendorId, _vendorId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!schema.SchemaName.StartsWith(_schemaNamePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (!TryParseSchemaVersion(schema.SchemaName, out var version))
            {
                continue;
            }

            if (version > _latestSchema.Version && version > newerSchemaVersion)
            {
                newerSchemaVersion = version;
            }
        }

        return newerSchemaVersion;
    }

    private static bool TryParseSchemaVersion(string schemaName, out int version)
    {
        version = -1;
        if (schemaName == _schemaNamePrefix)
        {
            version = 0;
            return true;
        }

        var versionPrefix = $"{_schemaNamePrefix}V";
        if (!schemaName.StartsWith(versionPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        return int.TryParse(schemaName.Substring(versionPrefix.Length), out version);
    }

    private bool ValidateExternalDependencies()
    {
        var settings = _settingsService.GlobalSettings;

        if (settings.RecordTransmittals)
        {
            if (!CheckDatabaseFileExists(settings.DatabaseFile))
            {
                return false;
            }

            _settingsService.GetSettings();
        }

        if (settings.UseDrawingIssueStore2)
        {
            var path = settings.DrawingIssueStore2.ParseFolderName();
            if (!Directory.Exists(path))
            {
                return false;
            }
        }

        return true;
    }

    private void SyncProjectInfoFromRevit(ProjectInfo projectInfo)
    {
        var settings = _settingsService.GlobalSettings;

        settings.ProjectNumber = projectInfo.Number;
        settings.ProjectName = projectInfo.Name;
        settings.ClientName = projectInfo.ClientName;

        settings.ProjectIdentifier = Util.GetParameterValueString(projectInfo, settings.ProjectIdentifierParamGuid);
        settings.Originator = Util.GetParameterValueString(projectInfo, settings.OriginatorParamGuid);
        settings.Role = Util.GetParameterValueString(projectInfo, settings.RoleParamGuid);
    }

    private Schema GetSchema(Guid schemaGuid)
    {
        var schema = Schema.ListSchemas().FirstOrDefault(s => s.GUID == schemaGuid);
        if (schema == null)
        {
            _logger.LogDebug("Schema {SchemaGuid} not found", schemaGuid);
        }
        else
        {
            _logger.LogDebug("Schema {SchemaName} found", schema.SchemaName);
        }

        return schema;
    }

    private static bool SchemaHasStorage(Document document, Schema schema)
    {
        var collector = new FilteredElementCollector(document);
        collector.WherePasses(new ExtensibleStorageFilter(schema.GUID));
        return collector.ToElementIds().Count > 0;
    }

    private static DataStorage FindDataStorageElement(Document document, Schema schema)
    {
        var collector = new FilteredElementCollector(document)
            .OfClass(typeof(DataStorage))
            .WherePasses(new ExtensibleStorageFilter(schema.GUID))
            .Where(e => _dataStorageElementName.Equals(e.Name, StringComparison.Ordinal));

        return collector.FirstOrDefault() as DataStorage;
    }

    private static void ReadIfPresent<T>(Schema schema, Entity entity, string fieldName, Action<T> apply)
    {
        var field = schema.GetField(fieldName);
        if (field != null)
        {
            apply(entity.Get<T>(field));
        }
    }

    private static void WriteIfPresent<T>(Schema schema, Entity entity, string fieldName, T value)
    {
        var field = schema.GetField(fieldName);
        if (field != null)
        {
            entity.Set(field, value);
        }
    }

    private static IDictionary<string, string> ListToDictionary<T>(
        IEnumerable<T> items,
        Func<T, string> codeSelector,
        Func<T, string> descriptionSelector)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var item in items)
        {
            dictionary[codeSelector(item) ?? string.Empty] = descriptionSelector(item) ?? string.Empty;
        }

        return dictionary;
    }

    private static List<T> DictionaryToList<T>(IDictionary<string, string> values, Func<string, string, T> factory)
    {
        var list = new List<T>(values.Count);
        foreach (var pair in values)
        {
            list.Add(factory(pair.Key, pair.Value));
        }

        return list;
    }

    private sealed class SchemaVersionInfo
    {
        public SchemaVersionInfo(int version, string name, string guidText)
        {
            Version = version;
            Name = name;
            GuidText = guidText;
        }

        public int Version { get; }

        public string Name { get; }

        public string GuidText { get; }

        public Guid Guid => new Guid(GuidText);
    }
}

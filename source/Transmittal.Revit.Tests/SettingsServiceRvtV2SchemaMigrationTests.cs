using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Microsoft.Extensions.Logging.Abstractions;
using Nice3point.TUnit.Revit;
using Nice3point.TUnit.Revit.Executors;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Services;
using TUnit.Core.Executors;

namespace Transmittal.Revit.Tests;

public sealed class SettingsServiceRvtV2SchemaMigrationTests : RevitApiTest
{
    private const string DataStorageElementName = "TransmittalSettings";
    private const string VendorId = "Transmittal";

    private static readonly Guid SchemaGuidV1 = new("42DCEC84-45AB-4CA1-8C6B-8C4853A23BCF");
    private static readonly Guid SchemaGuidV2 = new("2896AAE5-B7E9-4854-8AC7-8D20FD59C51E");
    private static readonly Guid SchemaGuidV3 = new("8D2FD527-CF54-45DB-9248-C65F6729D18B");
    private static readonly Guid SchemaGuidV4 = new("5A3671ED-90E7-48B3-8BC1-D2C37CF31D5A");

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task GetSettingsRvt_ShouldMigrateLegacySchemaV1_ToV4()
    {
        var document = Application.NewProjectDocument(UnitSystem.Metric);
        try
        {
            const string expectedFileNameFilter = "V1Filter";
            const string expectedFileNameFilter2 = "V1Filter2";

            SeedLegacySchemaV1(document, expectedFileNameFilter, expectedFileNameFilter2);
            var sut = CreateSut(out var settingsService);

            var result = sut.GetSettingsRvt(document);

            await Assert.That(result).IsTrue();
            await Assert.That(settingsService.GlobalSettings.FileNameFilter).IsEqualTo(expectedFileNameFilter);
            await Assert.That(settingsService.GlobalSettings.FileNameFilter2).IsEqualTo(expectedFileNameFilter2);
            await Assert.That(CountElementsWithStorage(document, SchemaGuidV1)).IsEqualTo(0);
            await Assert.That(CountElementsWithStorage(document, SchemaGuidV4)).IsGreaterThan(0);
        }
        finally
        {
            document.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task GetSettingsRvt_ShouldMigrateLegacySchemaV2_ToV4()
    {
        var document = Application.NewProjectDocument(UnitSystem.Metric);
        try
        {
            const string expectedFileNameFilter = "V2Filter";
            const string expectedSheetPackageGuid = "c2f31927-ee44-4b7f-b5f3-cf4c4f7f6173";

            SeedLegacySchemaV2(document, expectedFileNameFilter, expectedSheetPackageGuid);
            var sut = CreateSut(out var settingsService);

            var result = sut.GetSettingsRvt(document);

            await Assert.That(result).IsTrue();
            await Assert.That(settingsService.GlobalSettings.FileNameFilter).IsEqualTo(expectedFileNameFilter);
            await Assert.That(settingsService.GlobalSettings.SheetPackageParamGuid).IsEqualTo(expectedSheetPackageGuid);
            await Assert.That(CountElementsWithStorage(document, SchemaGuidV2)).IsEqualTo(0);
            await Assert.That(CountElementsWithStorage(document, SchemaGuidV4)).IsGreaterThan(0);
        }
        finally
        {
            document.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task GetSettingsRvt_ShouldMigrateLegacySchemaV3_ToV4()
    {
        var document = Application.NewProjectDocument(UnitSystem.Metric);
        try
        {
            const string expectedDrawingIssueStore2 = @"C:\Temp\LegacyCdePath";

            SeedLegacySchemaV3(document, expectedDrawingIssueStore2);
            var sut = CreateSut(out var settingsService);

            var result = sut.GetSettingsRvt(document);

            await Assert.That(result).IsTrue();
            await Assert.That(settingsService.GlobalSettings.DrawingIssueStore2).IsEqualTo(expectedDrawingIssueStore2);
            await Assert.That(settingsService.GlobalSettings.UseDrawingIssueStore2).IsFalse();
            await Assert.That(CountElementsWithStorage(document, SchemaGuidV3)).IsEqualTo(0);
            await Assert.That(CountElementsWithStorage(document, SchemaGuidV4)).IsGreaterThan(0);
        }
        finally
        {
            document.Close(false);
        }
    }

    private static SettingsServiceRvtV2 CreateSut(out TestSettingsService settingsService)
    {
        settingsService = new TestSettingsService();
        var dataConnection = new TestDataConnection();
        var messageBoxService = new TestMessageBoxService();

        return new SettingsServiceRvtV2(
            dataConnection,
            settingsService,
            NullLogger<SettingsServiceRvtV2>.Instance,
            messageBoxService);
    }

    private static void SeedLegacySchemaV1(Document document, string fileNameFilter, string fileNameFilter2)
    {
        var schema = GetOrCreateSchemaV1();
        using var transaction = new Transaction(document, "Seed Legacy Schema V1");
        transaction.Start();

        var storage = DataStorage.Create(document);
        storage.Name = DataStorageElementName;

        var entity = new Entity(schema);
        entity.Set(schema.GetField(nameof(SettingsModel.FileNameFilter)), fileNameFilter);
        entity.Set(schema.GetField(nameof(SettingsModel.FileNameFilter2)), fileNameFilter2);

        storage.SetEntity(entity);
        transaction.Commit();
    }

    private static void SeedLegacySchemaV2(Document document, string fileNameFilter, string sheetPackageGuid)
    {
        var schema = GetOrCreateSchemaV2();
        using var transaction = new Transaction(document, "Seed Legacy Schema V2");
        transaction.Start();

        var storage = DataStorage.Create(document);
        storage.Name = DataStorageElementName;

        var entity = new Entity(schema);
        entity.Set(schema.GetField(nameof(SettingsModel.FileNameFilter)), fileNameFilter);
        entity.Set(schema.GetField(nameof(SettingsModel.SheetPackageParamGuid)), sheetPackageGuid);

        storage.SetEntity(entity);
        transaction.Commit();
    }

    private static void SeedLegacySchemaV3(Document document, string drawingIssueStore2)
    {
        var schema = GetOrCreateSchemaV3();
        using var transaction = new Transaction(document, "Seed Legacy Schema V3");
        transaction.Start();

        var storage = DataStorage.Create(document);
        storage.Name = DataStorageElementName;

        var entity = new Entity(schema);
        entity.Set(schema.GetField(nameof(SettingsModel.DrawingIssueStore2)), drawingIssueStore2);
        entity.Set(schema.GetField(nameof(SettingsModel.UseDrawingIssueStore2)), false);

        storage.SetEntity(entity);
        transaction.Commit();
    }

    private static int CountElementsWithStorage(Document document, Guid schemaGuid)
    {
        var collector = new FilteredElementCollector(document);
        collector.WherePasses(new ExtensibleStorageFilter(schemaGuid));
        return collector.ToElementIds().Count;
    }

    private static Schema GetOrCreateSchemaV1()
    {
        return GetOrCreateSchema(
            SchemaGuidV1,
            "TransmittalAppSettingsV1",
            new[]
            {
                (nameof(SettingsModel.FileNameFilter), typeof(string)),
                (nameof(SettingsModel.FileNameFilter2), typeof(string)),
            });
    }

    private static Schema GetOrCreateSchemaV2()
    {
        return GetOrCreateSchema(
            SchemaGuidV2,
            "TransmittalAppSettingsV2",
            new[]
            {
                (nameof(SettingsModel.FileNameFilter), typeof(string)),
                (nameof(SettingsModel.SheetPackageParamGuid), typeof(string)),
            });
    }

    private static Schema GetOrCreateSchemaV3()
    {
        return GetOrCreateSchema(
            SchemaGuidV3,
            "TransmittalAppSettingsV3",
            new[]
            {
                (nameof(SettingsModel.DrawingIssueStore2), typeof(string)),
                (nameof(SettingsModel.UseDrawingIssueStore2), typeof(bool)),
            });
    }

    private static Schema GetOrCreateSchema(Guid guid, string schemaName, IEnumerable<(string Name, Type Type)> fields)
    {
        var existing = Schema.Lookup(guid);
        if (existing != null)
        {
            return existing;
        }

        var builder = new SchemaBuilder(guid);
        builder.SetReadAccessLevel(AccessLevel.Public);
        builder.SetWriteAccessLevel(AccessLevel.Public);
        builder.SetVendorId(VendorId);
        builder.SetSchemaName(schemaName);

        foreach (var field in fields)
        {
            builder.AddSimpleField(field.Name, field.Type);
        }

        return builder.Finish();
    }

    private sealed class TestSettingsService : ISettingsService
    {
        public SettingsModel GlobalSettings { get; set; } = new();

        public int GetSettingsCallCount { get; private set; }

        public void GetSettings()
        {
            GetSettingsCallCount++;
        }

        public void UpdateSettings()
        {
        }
    }

    private sealed class TestDataConnection : IDataConnection
    {
        public bool CheckConnection(string dbFilePath)
        {
            return true;
        }

        public T CreateData<T, U>(string dbFilePath, string sqlStatement, T model, U parameters, string keyPropertyName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LoadData<T, U>(string dbFilePath, string sqlStatement, U parameters)
        {
            throw new NotImplementedException();
        }

        public void SaveData<T>(string dbFilePath, string sqlStatement, T data)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction(string dbFilePath)
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public void ExecuteInTransaction<T>(string sqlStatement, T parameters)
        {
            throw new NotImplementedException();
        }

        public void UpgradeDatabase(string dbFilePath)
        {
            throw new NotImplementedException();
        }

        public void CreateDatabaseSchema(string dbFilePath)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class TestMessageBoxService : IMessageBoxService
    {
        public bool ShowYesNo(string title, string message)
        {
            return true;
        }

        public bool ShowOkCancel(string title, string message)
        {
            return true;
        }

        public bool ShowCancel(string title, string message)
        {
            return true;
        }

        public bool ShowOk(string title, string message)
        {
            return true;
        }
    }
}

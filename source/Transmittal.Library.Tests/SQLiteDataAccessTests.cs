using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Transmittal.Library.DataAccess;
using Transmittal.Library.Services;

namespace Transmittal.Library.Tests;

public class SQLiteDataAccessTests
{
    private SQLiteDataAccess _dataAccess = null!;
    private string _dbPath = string.Empty;

    [Before(Test)]
    public async Task Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"transmittal-test-{Guid.NewGuid():N}.tdb");
        _dataAccess = CreateDataAccess();
        await Task.CompletedTask;
    }

    [After(Test)]
    public async Task Cleanup()
    {
        DeleteIfExists(_dbPath);
        DeleteIfExists($"{_dbPath}-wal");
        DeleteIfExists($"{_dbPath}-shm");
        await Task.CompletedTask;
    }

    [Test]
    public async Task CreateDatabaseSchema_ShouldCreateDatabaseFile()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        await Assert.That(File.Exists(_dbPath)).IsTrue();
    }

    [Test]
    public async Task CreateDatabaseSchema_ShouldCreateAllRequiredTables()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));

        var tables = GetTableNames(_dbPath);
        await Assert.That(tables).Contains("Company");
        await Assert.That(tables).Contains("Person");
        await Assert.That(tables).Contains("Settings");
        await Assert.That(tables).Contains("IssueFormat");
        await Assert.That(tables).Contains("DocumentStatus");
        await Assert.That(tables).Contains("Transmittal");
        await Assert.That(tables).Contains("TransmittalDistribution");
        await Assert.That(tables).Contains("TransmittalItems");
    }

    [Test]
    public async Task CreateDatabaseSchema_ShouldInsertDefaultIssueFormats()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));

        using var conn = OpenConnection(_dbPath);
        var issueFormats = conn.Query<string>("SELECT Code FROM IssueFormat ORDER BY Code;").ToList();

        await Assert.That(issueFormats.Count).IsEqualTo(3);
        await Assert.That(issueFormats).Contains("C");
        await Assert.That(issueFormats).Contains("E");
        await Assert.That(issueFormats).Contains("P");
    }

    [Test]
    public async Task CreateDatabaseSchema_ShouldInsertDefaultDocumentStatuses()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));

        using var conn = OpenConnection(_dbPath);
        var statuses = conn.Query<string>("SELECT Code FROM DocumentStatus;").ToList();

        await Assert.That(statuses.Count).IsEqualTo(11);
        await Assert.That(statuses).Contains("S0");
        await Assert.That(statuses).Contains("CR");
    }

    [Test]
    public async Task CreateDatabaseSchema_ShouldInsertDefaultSettings()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));

        using var conn = OpenConnection(_dbPath);
        var settings = conn.QuerySingle<SettingsRecord>(
            "SELECT ID, DateFormatString, ShowFileTransfer, FileTransferType FROM Settings WHERE ID = 1;");

        await Assert.That(settings.ID).IsEqualTo(1);
        await Assert.That(settings.DateFormatString).IsEqualTo("dd.MM.yy");
        await Assert.That(settings.ShowFileTransfer).IsTrue();
        await Assert.That(settings.FileTransferType).IsEqualTo(0);
    }

    [Test]
    public async Task CreateDatabaseSchema_ShouldSetDatabaseVersionToFour()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(4);
    }

    [Test]
    public async Task GetDatabaseVersion_ShouldReturnZeroForNewDatabase()
    {
        using (var conn = OpenConnection(_dbPath))
        {
            conn.Execute("CREATE TABLE Dummy (Id INTEGER PRIMARY KEY);");
        }

        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(0);
    }

    [Test]
    public async Task UpgradeDatabase_ShouldUpgradeLegacyDatabaseFromVersionZeroToFour()
    {
        CreateLegacyDatabase(_dbPath);
        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(0);

        await Task.Run(() => _dataAccess.UpgradeDatabase(_dbPath));

        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(4);
    }

    [Test]
    public async Task UpgradeDatabase_ShouldNotChangeVersionForCurrentDatabase()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(4);

        await Task.Run(() => _dataAccess.UpgradeDatabase(_dbPath));

        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(4);
    }

    [Test]
    public async Task UpgradeDatabase_ShouldPreserveExistingDataWhenUpgrading()
    {
        CreateLegacyDatabase(_dbPath);
        InsertTestData(_dbPath);

        await Task.Run(() => _dataAccess.UpgradeDatabase(_dbPath));

        using var conn = OpenConnection(_dbPath);
        var companyCount = conn.QuerySingle<int>("SELECT COUNT(*) FROM Company;");
        var personCount = conn.QuerySingle<int>("SELECT COUNT(*) FROM Person;");
        await Assert.That(companyCount).IsGreaterThan(0);
        await Assert.That(personCount).IsGreaterThan(0);
    }

    [Test]
    public async Task UpgradeDatabase_ShouldAddMissingColumnsForLegacyDatabase()
    {
        CreateLegacyDatabase(_dbPath);

        await Task.Run(() => _dataAccess.UpgradeDatabase(_dbPath));

        var settingsColumns = GetTableColumns(_dbPath, "Settings");
        var transmittalItemColumns = GetTableColumns(_dbPath, "TransmittalItems");
        var companyColumns = GetTableColumns(_dbPath, "Company");
        var personColumns = GetTableColumns(_dbPath, "Person");

        await Assert.That(settingsColumns).Contains("ClientName");
        await Assert.That(settingsColumns).Contains("UseRevit");
        await Assert.That(settingsColumns).Contains("DrawingIssueStore2");
        await Assert.That(settingsColumns).Contains("UseDrawingIssueStore2");
        await Assert.That(settingsColumns).Contains("ShowFileTransfer");
        await Assert.That(settingsColumns).Contains("FileTransferType");
        await Assert.That(settingsColumns).Contains("ProjectDirectoryDocumentTypeCode");
        await Assert.That(settingsColumns).Contains("ProjectDirectoryFirstNumber");
        await Assert.That(settingsColumns).Contains("TransmittalSheetDocumentTypeCode");
        await Assert.That(settingsColumns).Contains("TransmittalSheetFirstNumber");
        await Assert.That(settingsColumns).Contains("TransmittalSummaryDocumentTypeCode");
        await Assert.That(settingsColumns).Contains("TransmittalSummaryFirstNumber");
        await Assert.That(settingsColumns).Contains("MasterDocumentsListDocumentTypeCode");
        await Assert.That(settingsColumns).Contains("MasterDocumentsListFirstNumber");
        await Assert.That(transmittalItemColumns).Contains("DrgPackage");
        await Assert.That(companyColumns).Contains("Role");
        await Assert.That(companyColumns).Contains("OrganizationCode");
        await Assert.That(personColumns).Contains("Archive");
    }

    [Test]
    public async Task UpgradeDatabase_ShouldHandleReadOnlyLegacyDatabaseSafely()
    {
        CreateLegacyDatabase(_dbPath);
        var readOnlyPath = $"{_dbPath};Mode=ReadOnly";

        await Task.Run(() => _dataAccess.UpgradeDatabase(readOnlyPath));

        var settingsColumns = GetTableColumns(_dbPath, "Settings");
        await Assert.That(GetDatabaseVersion(_dbPath)).IsEqualTo(0);
        await Assert.That(settingsColumns.Contains("ClientName")).IsFalse();
    }

    [Test]
    public async Task CreateDatabaseSchema_CompanyTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "Company");

        await Assert.That(columns).Contains("ID");
        await Assert.That(columns).Contains("CompanyName");
        await Assert.That(columns).Contains("Role");
        await Assert.That(columns).Contains("OrganizationCode");
        await Assert.That(columns).Contains("Address");
        await Assert.That(columns).Contains("Tel");
        await Assert.That(columns).Contains("Fax");
        await Assert.That(columns).Contains("Website");
    }

    [Test]
    public async Task CreateDatabaseSchema_PersonTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "Person");

        await Assert.That(columns).Contains("ID");
        await Assert.That(columns).Contains("LastName");
        await Assert.That(columns).Contains("FirstName");
        await Assert.That(columns).Contains("Email");
        await Assert.That(columns).Contains("Tel");
        await Assert.That(columns).Contains("Mobile");
        await Assert.That(columns).Contains("Position");
        await Assert.That(columns).Contains("Notes");
        await Assert.That(columns).Contains("CompanyID");
        await Assert.That(columns).Contains("ShowInReport");
        await Assert.That(columns).Contains("Archive");
    }

    [Test]
    public async Task CreateDatabaseSchema_IssueFormatTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "IssueFormat");
        await AssertContainsColumns(columns, "ID", "Code", "Description");
    }

    [Test]
    public async Task CreateDatabaseSchema_DocumentStatusTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "DocumentStatus");
        await AssertContainsColumns(columns, "ID", "Code", "Description");
    }

    [Test]
    public async Task CreateDatabaseSchema_SettingsTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "Settings");
        await AssertContainsColumns(
            columns,
            "ID",
            "DateFormatString",
            "DrawingIssueStore",
            "DrawingIssueStore2",
            "IssueSheetStore",
            "ReportStore",
            "DirectoryStore",
            "FileNameFilter",
            "FileNameFilter2",
            "ProjectIdentifier",
            "ProjectNumber",
            "ProjectName",
            "ClientName",
            "UseExtranet",
            "UseISO19650",
            "UseDrawingIssueStore2",
            "UseRevit",
            "Originator",
            "Role",
            "ProjectIdentifierParamGuid",
            "OriginatorParamGuid",
            "RoleParamGuid",
            "SheetVolumeParamGuid",
            "SheetLevelParamGuid",
            "DocumentTypeParamGuid",
            "SheetStatusParamGuid",
            "SheetStatusDescriptionParamGuid",
            "SheetPackageParamGuid",
            "ShowFileTransfer",
            "FileTransferType",
            "ProjectDirectoryDocumentTypeCode",
            "ProjectDirectoryFirstNumber",
            "TransmittalSheetDocumentTypeCode",
            "TransmittalSheetFirstNumber",
            "TransmittalSummaryDocumentTypeCode",
            "TransmittalSummaryFirstNumber",
            "MasterDocumentsListDocumentTypeCode",
            "MasterDocumentsListFirstNumber");
    }

    [Test]
    public async Task CreateDatabaseSchema_TransmittalTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "Transmittal");
        await AssertContainsColumns(columns, "ID", "TransDate");
    }

    [Test]
    public async Task CreateDatabaseSchema_TransmittalDistributionTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "TransmittalDistribution");
        await AssertContainsColumns(columns, "TransDistID", "TransID", "PersonID", "TransFormat", "TransCopies");
    }

    [Test]
    public async Task CreateDatabaseSchema_TransmittalItemsTableShouldHaveCorrectColumns()
    {
        await Task.Run(() => _dataAccess.CreateDatabaseSchema(_dbPath));
        var columns = GetTableColumns(_dbPath, "TransmittalItems");
        await AssertContainsColumns(
            columns,
            "TransItemID",
            "TransID",
            "DrgProj",
            "DrgOriginator",
            "DrgVolume",
            "DrgLevel",
            "DrgType",
            "DrgRole",
            "DrgNumber",
            "DrgName",
            "DrgStatus",
            "DrgRev",
            "DrgDrawn",
            "DrgChecked",
            "DrgScale",
            "DrgPaper",
            "DrgPackage");
    }

    private static SQLiteDataAccess CreateDataAccess()
    {
        var logger = new Mock<ILogger<SQLiteDataAccess>>();
        var messageBox = new Mock<IMessageBoxService>();
        return new SQLiteDataAccess(logger.Object, messageBox.Object);
    }

    private static SqliteConnection OpenConnection(string dbPath)
    {
        var conn = new SqliteConnection($"Data Source={dbPath};");
        conn.Open();
        return conn;
    }

    private static int GetDatabaseVersion(string dbPath)
    {
        using var conn = OpenConnection(dbPath);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA user_version;";
        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result ?? 0);
    }

    private static List<string> GetTableNames(string dbPath)
    {
        using var conn = OpenConnection(dbPath);
        return conn.Query<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';").ToList();
    }

    private static List<string> GetTableColumns(string dbPath, string tableName)
    {
        using var conn = OpenConnection(dbPath);
        return conn.Query($"PRAGMA table_info(\"{tableName}\");")
            .Select(row => (string)row.name)
            .ToList();
    }

    private static void CreateLegacyDatabase(string dbPath)
    {
        using var conn = OpenConnection(dbPath);
        conn.Execute("PRAGMA foreign_keys = OFF;");

        conn.Execute(@"
            CREATE TABLE ""Company"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""CompanyName"" TEXT NOT NULL,
                ""Address"" TEXT,
                ""Tel"" TEXT,
                ""Fax"" TEXT,
                ""Website"" TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""Person"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""LastName"" TEXT NOT NULL,
                ""FirstName"" TEXT,
                ""Email"" TEXT,
                ""Tel"" TEXT,
                ""Mobile"" TEXT,
                ""Position"" TEXT,
                ""Notes"" TEXT,
                ""CompanyID"" INTEGER,
                ""ShowInReport"" INTEGER NOT NULL DEFAULT 1,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""IssueFormat"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""Code"" TEXT,
                ""Description"" TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""DocumentStatus"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""Code"" TEXT,
                ""Description"" TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""Settings"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""DateFormatString"" TEXT,
                ""DrawingIssueStore"" TEXT,
                ""IssueSheetStore"" TEXT,
                ""ReportStore"" TEXT,
                ""DirectoryStore"" TEXT,
                ""FileNameFilter"" TEXT,
                ""ProjectIdentifier"" TEXT,
                ""UseExtranet"" INTEGER,
                ""UseISO19650"" INTEGER,
                ""Originator"" TEXT,
                ""Role"" TEXT,
                ""ProjectIdentifierParamGuid"" TEXT,
                ""OriginatorParamGuid"" TEXT,
                ""RoleParamGuid"" TEXT,
                ""SheetVolumeParamGuid"" TEXT,
                ""SheetLevelParamGuid"" TEXT,
                ""DocumentTypeParamGuid"" TEXT,
                ""SheetStatusParamGuid"" TEXT,
                ""SheetStatusDescriptionParamGuid"" TEXT,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""Transmittal"" (
                ""ID"" INTEGER NOT NULL UNIQUE,
                ""TransDate"" TEXT NOT NULL DEFAULT CURRENT_DATE,
                PRIMARY KEY(""ID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""TransmittalDistribution"" (
                ""TransDistID"" INTEGER NOT NULL UNIQUE,
                ""TransID"" INTEGER NOT NULL,
                ""PersonID"" INTEGER NOT NULL,
                ""TransFormat"" TEXT,
                ""TransCopies"" INTEGER,
                PRIMARY KEY(""TransDistID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            CREATE TABLE ""TransmittalItems"" (
                ""TransItemID"" INTEGER NOT NULL UNIQUE,
                ""TransID"" INTEGER NOT NULL,
                ""DrgProj"" TEXT,
                ""DrgOriginator"" TEXT,
                ""DrgVolume"" TEXT,
                ""DrgLevel"" TEXT,
                ""DrgType"" TEXT,
                ""DrgRole"" TEXT,
                ""DrgNumber"" TEXT NOT NULL,
                ""DrgName"" TEXT,
                ""DrgStatus"" TEXT,
                ""DrgRev"" TEXT,
                ""DrgDrawn"" TEXT,
                ""DrgChecked"" TEXT,
                ""DrgScale"" TEXT,
                ""DrgPaper"" TEXT,
                PRIMARY KEY(""TransItemID"" AUTOINCREMENT)
            );");

        conn.Execute(@"
            INSERT INTO Settings (ID, DateFormatString, UseExtranet, UseISO19650)
            VALUES (1, 'dd.MM.yy', 0, 0);");
    }

    private static void InsertTestData(string dbPath)
    {
        using var conn = OpenConnection(dbPath);
        conn.Execute("INSERT INTO Company (CompanyName) VALUES ('ACME');");
        conn.Execute("INSERT INTO Person (LastName, FirstName, CompanyID) VALUES ('Doe', 'Jane', 1);");
    }

    private static void DeleteIfExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }

            File.Delete(path);
        }
        catch (IOException)
        {
            // Ignore transient file locks from SQLite connection pooling during test teardown.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore transient file locks from SQLite connection pooling during test teardown.
        }
    }

    private static async Task AssertContainsColumns(List<string> actualColumns, params string[] expectedColumns)
    {
        foreach (var column in expectedColumns)
        {
            await Assert.That(actualColumns).Contains(column);
        }
    }

    private sealed class SettingsRecord
    {
        public int ID { get; set; }
        public string DateFormatString { get; set; } = string.Empty;
        public bool ShowFileTransfer { get; set; }
        public int FileTransferType { get; set; }
    }
}



using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Windows.Markup;
using Transmittal.Library.Extensions;
using Transmittal.Library.Messages;
using Transmittal.Library.Services;

namespace Transmittal.Library.DataAccess;

public class SQLiteDataAccess : IDataConnection
{
    private readonly ILogger<SQLiteDataAccess> _logger;
    private readonly IMessageBoxService _messageBox;

    private SqliteConnection _connection;
    private SqliteTransaction _transaction;

    private const int _latestSchemaVersion = 4;

    public SQLiteDataAccess(ILogger<SQLiteDataAccess> logger, 
        IMessageBoxService messageBox)
    {
        _logger = logger;
        _messageBox = messageBox;
    }

    public bool CheckConnection(string dbFilePath)
    {
        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};Mode=ReadOnly;"))
        {
            try
            {
                dbConnection.Open();
                return true;
            }
            catch (SqliteException ex)
            {
                _logger.LogError(ex, "Failed to connect to database");
                return false;
            }
        }
    }

    public T CreateData<T, U>(string dbFilePath, string sqlStatement, T model, U parameters, string keyPropertyName)
    {
        int maxRetries = 3; // Maximum number of retries
        int retryDelay = 1000; // Delay between retries in milliseconds
        int attempt = 0;
        bool schemaRecoveryAttempted = false;

        while (attempt < maxRetries)
        {
            try
            {
                using (var dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
                {
                    dbConnection.Open();

                    // Set busy timeout to wait for the database to become available
                    dbConnection.Execute("PRAGMA busy_timeout = 10000;"); // Wait up to 10 seconds

                    var recordId = dbConnection.QuerySingle<int>(sqlStatement, parameters);

                    // Set the key property of the model
                    model.GetType().GetProperty(keyPropertyName).SetValue(model, recordId);

                    return model; // Success, return the model
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                // Log the busy error and retry
                _logger.LogWarning(ex, "Database is busy. Retrying operation (Attempt {Attempt}/{MaxRetries})...", attempt + 1, maxRetries);
                attempt++;

                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelay); // Wait before retrying
                }
                else
                {
                    _logger.LogError(ex, "Database operation failed after {MaxRetries} attempts.", maxRetries);
                    throw; // Re-throw the exception after max retries
                }
            }
            catch (SqliteException ex) when (IsMissingColumnException(ex) && !schemaRecoveryAttempted)
            {
                _logger.LogWarning(ex, "Missing-column error detected during CreateData. Reapplying latest schema and retrying once.");
                ReapplyLatestSchema(dbFilePath);
                schemaRecoveryAttempted = true;
            }
            catch (Exception ex)
            {
                // Log and re-throw other exceptions
                _logger.LogError(ex, "An error occurred during database operation.");
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without returning a result.");
    }
     
    public IEnumerable<T> LoadData<T, U>(string dbFilePath, string sqlStatement, U parameters)
    {
        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};Mode=ReadOnly;"))
        {
            dbConnection.Open();
            var rows = dbConnection.Query<T>(sqlStatement, parameters);
            return rows;
        }
    }

    public void SaveData<T>(string dbFilePath, string sqlStatement, T data)
    {
        int maxRetries = 3; // Maximum number of retries
        int retryDelay = 1000; // Delay between retries in milliseconds
        int attempt = 0;
        bool schemaRecoveryAttempted = false;

        while (attempt < maxRetries)
        {
            try
            {
                using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
                {
                    dbConnection.Open();

                    // Set busy timeout to wait for the database to become available
                    dbConnection.Execute("PRAGMA busy_timeout = 10000;"); // Wait up to 10 seconds

                    dbConnection.Execute(sqlStatement, data);
                    return; // Success, exit the method
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                // Log the busy error and retry
                _logger.LogWarning(ex, "Database is busy. Retrying operation (Attempt {Attempt}/{MaxRetries})...", attempt + 1, maxRetries);
                attempt++;

                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelay); // Wait before retrying
                }
                else
                {
                    _logger.LogError(ex, "Database operation failed after {MaxRetries} attempts.", maxRetries);
                    throw; // Re-throw the exception after max retries
                }
            }
            catch (SqliteException ex) when (IsMissingColumnException(ex) && !schemaRecoveryAttempted)
            {
                _logger.LogWarning(ex, "Missing-column error detected during SaveData. Reapplying latest schema and retrying once.");
                ReapplyLatestSchema(dbFilePath);
                schemaRecoveryAttempted = true;
            }
            catch (Exception ex)
            {
                // Log and re-throw other exceptions
                _logger.LogError(ex, "An error occurred during database operation.");
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without completing the operation.");
    }


    public void BeginTransaction(string dbFilePath)
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};");
            _connection.Open();
        }

        _transaction = _connection.BeginTransaction();
    }

    public void CommitTransaction()
    {
        _transaction?.Commit();
        _transaction = null;
    }

    public void RollbackTransaction()
    {
        _transaction?.Rollback();
        _transaction = null;
    }

    public void ExecuteInTransaction<T>(string sqlStatement, T parameters)
    {
        if (_connection == null || _transaction == null)
        {
            throw new InvalidOperationException("Transaction has not been started.");
        }

        using (var command = _connection.CreateCommand())
        {
            command.Transaction = _transaction;
            command.CommandText = sqlStatement;

            foreach (var property in typeof(T).GetProperties())
            {
                command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(parameters) ?? DBNull.Value);
            }

            command.ExecuteNonQuery();
        }
    }

    public void UpgradeDatabase(string dbFilePath)
    {
        int currentVersion = GetDatabaseVersion(dbFilePath);
        _logger.LogInformation("Current database version: {Version}", currentVersion);

        try
        {
            if (currentVersion == 0)
            {

                // Legacy database (pre-versioning) - run legacy upgrade code
                _logger.LogInformation("Detected legacy database (version 0). Running legacy upgrade code.");
                RunLegacyUpgrade(dbFilePath);
                // After legacy upgrade, set version to 3
                SetDatabaseVersion(dbFilePath, 3);
                currentVersion = 3;
                _logger.LogInformation("Legacy database upgraded and version set to 3");
            }

            if (currentVersion == 3)
            {
                ApplySchemaV4(dbFilePath);
                SetDatabaseVersion(dbFilePath, 4);
                currentVersion = 4;
                _logger.LogInformation("Database upgraded from v3 to v4");
            }

            if (currentVersion == 4)
            {
                _logger.LogInformation("Database v4 upgrade check completed");
            }

            if (currentVersion > _latestSchemaVersion)
            {
                // The database was created or upgraded by a newer application version, so warn the user about the version mismatch.
                _logger.LogWarning("Database version {CurrentVersion} is newer than application version {LatestVersion}", currentVersion, _latestSchemaVersion);
                _messageBox.ShowOk("Application version", "You appear to be opening a database which is newer than your current application version. Please check for software updates.");
            }

        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_READONLY)
        {
            _logger.LogWarning(ex, "Database upgrade skipped because the database is read-only: {DbFilePath}", dbFilePath);
            _messageBox.ShowOk("Database upgrade", "The selected database is read-only and cannot be upgraded. Please remove read-only permissions and try again.");
            return;
        }
    }

    private void RunLegacyUpgrade(string dbFilePath)
    {
        Dictionary<string, string> columnsToAdd = new Dictionary<string, string>();

        //first check of the required columns exist before adding to the dictionary to create them.
        //added at v1.2.0
        if(!ColumnExists(dbFilePath, "ClientName", "Settings"))
        {
            _logger.LogDebug("Column ClientName does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("ClientName", "ALTER TABLE Settings ADD COLUMN ClientName TEXT");
        }

        //added at v1.2.2
        if (!ColumnExists(dbFilePath, "FileNameFilter2", "Settings"))
        {
            _logger.LogDebug("Column FileNameFilter2 does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("FileNameFilter2", "ALTER TABLE Settings ADD COLUMN FileNameFilter2 TEXT");
        }

        //added at v2.0.0
        if(!ColumnExists(dbFilePath, "DrgPackage", "TransmittalItems"))
        {
            _logger.LogDebug("Column DrgPackage does not exist in TransmittalItems table. Adding to columns to add dictionary");
            columnsToAdd.Add("DrgPackage", "ALTER TABLE TransmittalItems ADD COLUMN DrgPackage TEXT");
        }
        if(!ColumnExists(dbFilePath, "UseRevit", "Settings"))
        {
            _logger.LogDebug("Column UseRevit does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("UseRevit", "ALTER TABLE Settings ADD COLUMN UseRevit INTEGER");
        }
        if(!ColumnExists(dbFilePath, "SheetPackageParamGuid", "Settings"))
        {
            _logger.LogDebug("Column SheetPackageParamGuid does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("SheetPackageParamGuid", "ALTER TABLE Settings ADD COLUMN SheetPackageParamGuid TEXT");
        }

        //added at v2.0.4
        if (!ColumnExists(dbFilePath, "ProjectNumber", "Settings"))
        {
            _logger.LogDebug("Column ProjectNumber does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("ProjectNumber", "ALTER TABLE Settings ADD COLUMN ProjectNumber TEXT");
        }
        if (!ColumnExists(dbFilePath, "ProjectName", "Settings"))
        {
            _logger.LogDebug("Column ProjectName does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("ProjectName", "ALTER TABLE Settings ADD COLUMN ProjectName TEXT");
        }

        //added at v2.1.0
        if (!ColumnExists(dbFilePath, "Role", "Company"))
        {
            _logger.LogDebug("Column Role does not exist in Company table. Adding to columns to add dictionary");
            columnsToAdd.Add("Role", "ALTER TABLE Company ADD COLUMN Role TEXT");
        }

        //added at v2.2.0
        if (!ColumnExists(dbFilePath, "Archive", "Person"))
        {
            _logger.LogDebug("Column Archive does not exist in Person table. Adding to columns to add dictionary");
            columnsToAdd.Add("Archive", "ALTER TABLE Person ADD COLUMN Archive INTEGER NOT NULL DEFAULT 0");
        }

        //added at v3.0.0
        if (!ColumnExists(dbFilePath, "DrawingIssueStore2", "Settings"))
        {
            _logger.LogDebug("Column DrawingIssueStore2 does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("DrawingIssueStore2", "ALTER TABLE Settings ADD COLUMN DrawingIssueStore2 TEXT");
        }
        if (!ColumnExists(dbFilePath, "UseDrawingIssueStore2", "Settings"))
        {
            _logger.LogDebug("Column UseDrawingIssueStore2 does not exist in Settings table. Adding to columns to add dictionary");
            columnsToAdd.Add("UseDrawingIssueStore2", "ALTER TABLE Settings ADD COLUMN UseDrawingIssueStore2 INTEGER");
        }

       
        if (columnsToAdd.Count == 0)
        {
            _logger.LogDebug("No columns to add to database");
            return;
        }

        int maxRetries = 3;
        int retryDelay = 1000;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
                {
                    dbConnection.Open();

                    dbConnection.Execute("PRAGMA busy_timeout = 10000;");

                    foreach (var column in columnsToAdd)
                    {
                        try
                        {
                            dbConnection.Execute(column.Value);
                            _logger.LogInformation("Successfully added column [{ColumnName}] to table.", column.Key);
                        }
                        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
                        {
                            _logger.LogWarning(ex, "Database is busy while adding column [{ColumnName}].", column.Key);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to add column [{ColumnName}].", column.Key);
                            throw;
                        }
                    }

                    return;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Database is busy. Retrying operation (Attempt {Attempt}/{MaxRetries})...", attempt + 1, maxRetries);
                attempt++;

                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelay);
                }
                else
                {
                    _logger.LogError(ex, "Database upgrade failed after {MaxRetries} attempts.", maxRetries);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database upgrade.");
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without completing the operation.");
    }

    public void CreateDatabaseSchema(string dbFilePath)
    {
        ApplySchemaV3(dbFilePath);
        ApplySchemaV4(dbFilePath);
        SetDatabaseVersion(dbFilePath, 4);
        _logger.LogInformation("Database schema v4 created and version set to 4");
    }

    private void ReapplyLatestSchema(string dbFilePath)
    {
        ApplySchemaV4(dbFilePath);
        SetDatabaseVersion(dbFilePath, _latestSchemaVersion);
        _logger.LogInformation("Latest schema reapplied after missing-column error.");
    }

    private void ApplySchemaV3(string dbFilePath)
    {
        int maxRetries = 3;
        int retryDelay = 1000;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
                {
                    dbConnection.Open();

                    // Set busy timeout to wait for the database to become available
                    dbConnection.Execute("PRAGMA busy_timeout = 10000;");

                    // Enable foreign keys
                    dbConnection.Execute("PRAGMA foreign_keys = ON;");

                    // Create Company table
                    dbConnection.Execute(@"
                        CREATE TABLE ""Company"" (
                            ""ID""	INTEGER NOT NULL UNIQUE,
                            ""CompanyName""	TEXT NOT NULL,
                            ""Role""	TEXT,
                            ""Address""	TEXT,
                            ""Tel""	TEXT,
                            ""Fax""	TEXT,
                            ""Website""	TEXT,
                            PRIMARY KEY(""ID"" AUTOINCREMENT)
                        );");

                    // Create Person table
                    dbConnection.Execute(@"
                        CREATE TABLE ""Person"" (
                            ""ID""	INTEGER NOT NULL UNIQUE,
                            ""LastName""	TEXT NOT NULL,
                            ""FirstName""	TEXT,
                            ""Email""	TEXT,
                            ""Tel""	TEXT,
                            ""Mobile""	TEXT,
                            ""Position""	TEXT,
                            ""Notes""	TEXT,
                            ""CompanyID""	INTEGER,
                            ""ShowInReport""	INTEGER NOT NULL DEFAULT 1,
                            ""Archive""	INTEGER NOT NULL DEFAULT 0,
                            PRIMARY KEY(""ID"" AUTOINCREMENT)
                        );");

                    // Create IssueFormat table
                    dbConnection.Execute(@"
                        CREATE TABLE ""IssueFormat"" (
                            ""ID""	INTEGER NOT NULL UNIQUE,
                            ""Code""	TEXT,
                            ""Description""	TEXT,
                            PRIMARY KEY(""ID"" AUTOINCREMENT)
                        );");

                    // Create DocumentStatus table
                    dbConnection.Execute(@"
                        CREATE TABLE ""DocumentStatus"" (
                            ""ID""	INTEGER NOT NULL UNIQUE,
                            ""Code""	TEXT,
                            ""Description""	TEXT,
                            PRIMARY KEY(""ID"" AUTOINCREMENT)
                        );");

                    // Create Settings table - correct column order to match template
                    dbConnection.Execute(@"
                        CREATE TABLE ""Settings"" (
                            ""ID""	INTEGER NOT NULL UNIQUE,
                            ""DateFormatString""	TEXT,
                            ""DrawingIssueStore""	TEXT,
                            ""DrawingIssueStore2""	TEXT,
                            ""IssueSheetStore""	TEXT,
                            ""ReportStore""	TEXT,
                            ""DirectoryStore""	TEXT,
                            ""FileNameFilter""	TEXT,
                            ""FileNameFilter2""	TEXT,
                            ""ProjectIdentifier""	TEXT,
                            ""ProjectNumber""	TEXT,
                            ""ProjectName""	TEXT,
                            ""ClientName""	TEXT,
                            ""UseExtranet""	INTEGER,
                            ""UseISO19650""	INTEGER,
                            ""UseDrawingIssueStore2""	INTEGER,
                            ""UseRevit""	INTEGER,
                            ""Originator""	TEXT,
                            ""Role""	TEXT,
                            ""ProjectIdentifierParamGuid""	TEXT,
                            ""OriginatorParamGuid""	TEXT,
                            ""RoleParamGuid""	TEXT,
                            ""SheetVolumeParamGuid""	TEXT,
                            ""SheetLevelParamGuid""	TEXT,
                            ""DocumentTypeParamGuid""	TEXT,
                            ""SheetStatusParamGuid""	TEXT,
                            ""SheetStatusDescriptionParamGuid""	TEXT,
                            ""SheetPackageParamGuid""	TEXT,
                            PRIMARY KEY(""ID"" AUTOINCREMENT)
                        );");

                    // Create Transmittal table - with DEFAULT CURRENT_DATE for TransDate
                    dbConnection.Execute(@"
                        CREATE TABLE ""Transmittal"" (
                            ""ID""	INTEGER NOT NULL UNIQUE,
                            ""TransDate""	TEXT NOT NULL DEFAULT CURRENT_DATE,
                            PRIMARY KEY(""ID"" AUTOINCREMENT)
                        );");

                    // Create TransmittalDistribution table - without defaults on TransFormat/TransCopies
                    dbConnection.Execute(@"
                        CREATE TABLE ""TransmittalDistribution"" (
                            ""TransDistID""	INTEGER NOT NULL UNIQUE,
                            ""TransID""	INTEGER NOT NULL,
                            ""PersonID""	INTEGER NOT NULL,
                            ""TransFormat""	TEXT,
                            ""TransCopies""	INTEGER,
                            PRIMARY KEY(""TransDistID"" AUTOINCREMENT)
                        );");

                    // Create TransmittalItems table - DrgOriginator is TEXT, match column order
                    dbConnection.Execute(@"
                        CREATE TABLE ""TransmittalItems"" (
                            ""TransItemID""	INTEGER NOT NULL UNIQUE,
                            ""TransID""	INTEGER NOT NULL,
                            ""DrgProj""	TEXT,
                            ""DrgOriginator""	TEXT,
                            ""DrgVolume""	TEXT,
                            ""DrgLevel""	TEXT,
                            ""DrgType""	TEXT,
                            ""DrgRole""	TEXT,
                            ""DrgNumber""	TEXT NOT NULL,
                            ""DrgName""	TEXT,
                            ""DrgStatus""	TEXT,
                            ""DrgRev""	TEXT,
                            ""DrgDrawn""	TEXT,
                            ""DrgChecked""	TEXT,
                            ""DrgScale""	TEXT,
                            ""DrgPaper""	TEXT,
                            ""DrgPackage""	TEXT,
                            PRIMARY KEY(""TransItemID"" AUTOINCREMENT)
                        );");

                    // Insert default Settings record
                    dbConnection.Execute(@"
                        INSERT INTO Settings (ID, DateFormatString, UseExtranet, UseISO19650, UseRevit, UseDrawingIssueStore2)
                        VALUES (1, 'dd.MM.yy', 0, 0, 0, 0);");

                    // Insert default IssueFormats
                    dbConnection.Execute("INSERT INTO IssueFormat (Code, Description) VALUES ('P', 'Paper');");
                    dbConnection.Execute("INSERT INTO IssueFormat (Code, Description) VALUES ('C', 'Cloud');");
                    dbConnection.Execute("INSERT INTO IssueFormat (Code, Description) VALUES ('E', 'Email');");

                    // Insert default DocumentStatuses
                    var documentStatuses = new[]
                    {
                        ("S0", "PRELIMINARY WIP"),
                        ("S1", "FOR CO-ORDINATION"),
                        ("S2", "FOR INFORMATION"),
                        ("S3", "FOR REVIEW AND COMMENT"),
                        ("S4", "FOR STAGE APPROVAL"),
                        ("S5", "FOR STAGE APPROVAL"),
                        ("A3", "CONTRACTUAL STAGE 3"),
                        ("A4", "CONTRACTUAL STAGE 4"),
                        ("A5", "CONTRACTUAL STAGE 5"),
                        ("A6", "CONTRACTUAL STAGE 6"),
                        ("CR", "AS BUILT")
                    };

                    foreach (var status in documentStatuses)
                    {
                        dbConnection.Execute("INSERT INTO DocumentStatus (Code, Description) VALUES (@Code, @Description);",
                            new { Code = status.Item1, Description = status.Item2 });
                    }

                    _logger.LogInformation("Schema v3 created successfully at {DbFilePath}", dbFilePath);
                    return;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Database is busy. Retrying operation (Attempt {Attempt}/{MaxRetries})...", attempt + 1, maxRetries);
                attempt++;

                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelay);
                }
                else
                {
                    _logger.LogError(ex, "Database schema creation failed after {MaxRetries} attempts.", maxRetries);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database schema creation.");
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without completing the operation.");
    }

    private void ApplySchemaV4(string dbFilePath)
    {
        var columnsToAdd = new Dictionary<string, string>();

        if (!ColumnExists(dbFilePath, "OrganisationCode", "Company"))
        {
            columnsToAdd.Add("OrganisationCode", "ALTER TABLE Company ADD COLUMN OrganisationCode TEXT");
        }
        if (!ColumnExists(dbFilePath, "ShowFileTransfer", "Settings"))
        {
            columnsToAdd.Add("ShowFileTransfer", "ALTER TABLE Settings ADD COLUMN ShowFileTransfer INTEGER NOT NULL DEFAULT 1");
        }
        if (!ColumnExists(dbFilePath, "FileTransferType", "Settings"))
        {
            columnsToAdd.Add("FileTransferType", "ALTER TABLE Settings ADD COLUMN FileTransferType INTEGER NOT NULL DEFAULT 0");
        }
        if (!ColumnExists(dbFilePath, "ProjectDirectoryDocumentTypeCode", "Settings"))
        {
            columnsToAdd.Add("ProjectDirectoryDocumentTypeCode", "ALTER TABLE Settings ADD COLUMN ProjectDirectoryDocumentTypeCode TEXT NOT NULL DEFAULT 'DY'");
        }
        if (!ColumnExists(dbFilePath, "ProjectDirectoryFirstNumber", "Settings"))
        {
            columnsToAdd.Add("ProjectDirectoryFirstNumber", "ALTER TABLE Settings ADD COLUMN ProjectDirectoryFirstNumber TEXT NOT NULL DEFAULT '0001'");
        }
        if (!ColumnExists(dbFilePath, "ProjectDirectoryVolume", "Settings"))
        {
            columnsToAdd.Add("ProjectDirectoryVolume", "ALTER TABLE Settings ADD COLUMN ProjectDirectoryVolume TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "ProjectDirectoryLevel", "Settings"))
        {
            columnsToAdd.Add("ProjectDirectoryLevel", "ALTER TABLE Settings ADD COLUMN ProjectDirectoryLevel TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSheetDocumentTypeCode", "Settings"))
        {
            columnsToAdd.Add("TransmittalSheetDocumentTypeCode", "ALTER TABLE Settings ADD COLUMN TransmittalSheetDocumentTypeCode TEXT NOT NULL DEFAULT 'TL'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSheetFirstNumber", "Settings"))
        {
            columnsToAdd.Add("TransmittalSheetFirstNumber", "ALTER TABLE Settings ADD COLUMN TransmittalSheetFirstNumber TEXT NOT NULL DEFAULT '0000'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSheetVolume", "Settings"))
        {
            columnsToAdd.Add("TransmittalSheetVolume", "ALTER TABLE Settings ADD COLUMN TransmittalSheetVolume TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSheetLevel", "Settings"))
        {
            columnsToAdd.Add("TransmittalSheetLevel", "ALTER TABLE Settings ADD COLUMN TransmittalSheetLevel TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSummaryDocumentTypeCode", "Settings"))
        {
            columnsToAdd.Add("TransmittalSummaryDocumentTypeCode", "ALTER TABLE Settings ADD COLUMN TransmittalSummaryDocumentTypeCode TEXT NOT NULL DEFAULT 'MX'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSummaryFirstNumber", "Settings"))
        {
            columnsToAdd.Add("TransmittalSummaryFirstNumber", "ALTER TABLE Settings ADD COLUMN TransmittalSummaryFirstNumber TEXT NOT NULL DEFAULT '0001'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSummaryVolume", "Settings"))
        {
            columnsToAdd.Add("TransmittalSummaryVolume", "ALTER TABLE Settings ADD COLUMN TransmittalSummaryVolume TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "TransmittalSummaryLevel", "Settings"))
        {
            columnsToAdd.Add("TransmittalSummaryLevel", "ALTER TABLE Settings ADD COLUMN TransmittalSummaryLevel TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "MasterDocumentsListDocumentTypeCode", "Settings"))
        {
            columnsToAdd.Add("MasterDocumentsListDocumentTypeCode", "ALTER TABLE Settings ADD COLUMN MasterDocumentsListDocumentTypeCode TEXT NOT NULL DEFAULT 'MX'");
        }
        if (!ColumnExists(dbFilePath, "MasterDocumentsListFirstNumber", "Settings"))
        {
            columnsToAdd.Add("MasterDocumentsListFirstNumber", "ALTER TABLE Settings ADD COLUMN MasterDocumentsListFirstNumber TEXT NOT NULL DEFAULT '0002'");
        }
        if (!ColumnExists(dbFilePath, "MasterDocumentsListVolume", "Settings"))
        {
            columnsToAdd.Add("MasterDocumentsListVolume", "ALTER TABLE Settings ADD COLUMN MasterDocumentsListVolume TEXT NOT NULL DEFAULT 'XX'");
        }
        if (!ColumnExists(dbFilePath, "MasterDocumentsListLevel", "Settings"))
        {
            columnsToAdd.Add("MasterDocumentsListLevel", "ALTER TABLE Settings ADD COLUMN MasterDocumentsListLevel TEXT NOT NULL DEFAULT 'XX'");
        }

        if (columnsToAdd.Count == 0)
        {
            _logger.LogDebug("ApplySchemaV4: No schema changes required");
            return;
        }

        int maxRetries = 3;
        int retryDelay = 1000;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
                {
                    dbConnection.Open();
                    dbConnection.Execute("PRAGMA busy_timeout = 10000;");

                    foreach (var column in columnsToAdd)
                    {
                        dbConnection.Execute(column.Value);
                        _logger.LogInformation("Successfully added column [{ColumnName}] to table.", column.Key);
                    }

                    return;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Database is busy. Retrying operation (Attempt {Attempt}/{MaxRetries})...", attempt + 1, maxRetries);
                attempt++;

                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelay);
                }
                else
                {
                    _logger.LogError(ex, "Database v4 schema update failed after {MaxRetries} attempts.", maxRetries);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during v4 schema update.");
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without completing v4 schema update.");
    }

    private bool ColumnExists(string dbFilePath, string columnName, string tableName)
    {
        string sql = $"SELECT INSTR(sql, '{columnName}') FROM sqlite_master WHERE type='table' AND name='{tableName}';";

        var result = LoadData<int, dynamic>(dbFilePath, sql, null).FirstOrDefault();

        if (result == 0)
        {
            return false;
        }

        return true;
    }

    private int GetDatabaseVersion(string dbFilePath)
    {
        try
        {
            using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
            {
                dbConnection.Open();
                var version = dbConnection.QuerySingleOrDefault<int>("PRAGMA user_version;");
                _logger.LogDebug("Database version retrieved: {Version}", version);
                return version;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve database version, assuming version 0");
            return 0;
        }
    }

    private void SetDatabaseVersion(string dbFilePath, int version)
    {
        int maxRetries = 3;
        int retryDelay = 1000;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
                {
                    dbConnection.Open();
                    using (var cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA busy_timeout = 10000;";
                        cmd.ExecuteNonQuery();
                    }
                    // Set the user version directly with DbCommand (not Dapper)
                    using (var cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"PRAGMA user_version = {version};";
                        cmd.ExecuteNonQuery();
                    }
                    _logger.LogInformation("Database version set to {Version}", version);
                    return;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_BUSY)
            {
                _logger.LogWarning(ex, "Database is busy while setting version. Retrying (Attempt {Attempt}/{MaxRetries})...", attempt + 1, maxRetries);
                attempt++;

                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelay);
                }
                else
                {
                    _logger.LogError(ex, "Failed to set database version after {MaxRetries} attempts.", maxRetries);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting database version.");
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without setting version.");
    }

    private static bool IsMissingColumnException(SqliteException ex)
    {
        if (ex.SqliteErrorCode != SQLitePCL.raw.SQLITE_ERROR || string.IsNullOrWhiteSpace(ex.Message))
        {
            return false;
        }

        return ex.Message.Contains("no such column", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("has no column named", StringComparison.OrdinalIgnoreCase);
    }


}

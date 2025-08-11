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

    private SqliteConnection _connection;
    private SqliteTransaction _transaction;

    public SQLiteDataAccess(ILogger<SQLiteDataAccess> logger, 
        IMessageBoxService messageBox)
    {
        _logger = logger;
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

        int maxRetries = 3; // Maximum number of retries
        int retryDelay = 1000; // Delay between retries in milliseconds
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
                {
                    dbConnection.Open();

                    // Set busy timeout to wait for the database to become available
                    dbConnection.Execute("PRAGMA busy_timeout = 10000;"); // Wait up to 10 seconds

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
                            throw; // Re-throw to trigger retry logic
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to add column [{ColumnName}].", column.Key);
                            throw; // Re-throw to ensure the operation is not silently ignored
                        }
                    }

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
                    _logger.LogError(ex, "Database upgrade failed after {MaxRetries} attempts.", maxRetries);
                    throw; // Re-throw the exception after max retries
                }
            }
            catch (Exception ex)
            {
                // Log and re-throw other exceptions
                _logger.LogError(ex, "An error occurred during database upgrade.");
                
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error: Retry loop exited without completing the operation.");
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
}

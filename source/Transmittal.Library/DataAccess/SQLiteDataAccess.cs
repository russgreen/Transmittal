using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;
using System.IO;
using Transmittal.Library.Extensions;
using Transmittal.Library.Messages;

namespace Transmittal.Library.DataAccess;

public class SQLiteDataAccess : IDataConnection
{
    private readonly ILogger<SQLiteDataAccess> _logger;

    public SQLiteDataAccess(ILogger<SQLiteDataAccess> logger)
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
        WaitForLockFileToClear(dbFilePath);
        CreateLockFile(dbFilePath);
        
        using (var dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
        {
            dbConnection.Open();
            //var recordId = dbConnection.ExecuteScalar<int>(sqlStatement, parameters);
            var recordId = dbConnection.QuerySingle<int>(sqlStatement, parameters);

            DeleteLockFile(dbFilePath);

            //establish Id parameter of T (will not be the same name in every model)
            model.GetType().GetProperty(keyPropertyName).SetValue(model, recordId);

            return model;
        }
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
        WaitForLockFileToClear(dbFilePath);
        CreateLockFile(dbFilePath);
        
        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
        {
            dbConnection.Open();
            dbConnection.Execute(sqlStatement, data);
        }

        DeleteLockFile(dbFilePath);
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

        if (columnsToAdd.Count == 0)
        {
            _logger.LogDebug("No columns to add to database");
            return;
        }

        WaitForLockFileToClear(dbFilePath);
        CreateLockFile(dbFilePath);

        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()};"))
        {
            dbConnection.Open();

            foreach (var column in columnsToAdd)
            {
                try
                {
                    dbConnection.Execute(column.Value);
                }
                catch(Exception ex)
                {
                   _logger.LogDebug(ex, "Failed to create column[{column}]. Most likely it already exists, which is fine.", column.Key);
                }
            }
        }

        DeleteLockFile(dbFilePath);
    }


    /// <summary>
    /// To prevent concurrent write attempts on the database a lock file will be created
    /// </summary>
    /// <param name="dbFilePath"></param>
    private void WaitForLockFileToClear(string dbFilePath)
    {
        bool loggedWaitingMessage = false;

        var lockFilePath = $"{dbFilePath.ParsePathWithEnvironmentVariables()}.lock";
        while (File.Exists(lockFilePath))
        {
            if(!loggedWaitingMessage)
            {
                _logger.LogInformation("Waiting for lock file [{lockFilePath}] to clear", lockFilePath);

                //send a message that a lock file exists
                WeakReferenceMessenger.Default.Send(new LockFileMessage(lockFilePath));

                loggedWaitingMessage = true;
            }
            Thread.Sleep(100);
        }

        //lock file has been cleared
        WeakReferenceMessenger.Default.Send(new LockFileMessage(""));

        _logger.LogInformation("[{lockFilePath}] cleared", lockFilePath);
    }

    private void CreateLockFile(string dbFilePath)
    {
        var lockFilePath = $"{dbFilePath.ParsePathWithEnvironmentVariables()}.lock";

        if (!File.Exists(lockFilePath))
        {
            using (FileStream fs = File.Create(lockFilePath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine($"Database locked by {Environment.UserName} on {File.GetCreationTime(lockFilePath)}");
                }
            }
        }

        _logger.LogDebug("Created lock file [{lockFilePath}]", lockFilePath);
    }

    private void DeleteLockFile(string dbFilePath)
    {
        var lockFilePath = $"{dbFilePath.ParsePathWithEnvironmentVariables()}.lock";
        File.Delete(lockFilePath);
        _logger.LogDebug("Deleted lock file [{lockFilePath}]", lockFilePath);
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

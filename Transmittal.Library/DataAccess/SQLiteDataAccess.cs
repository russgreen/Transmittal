using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;
using System.IO;
using Transmittal.Library.Extensions;

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
        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
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
        
        using (var dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
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
        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
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
        Dictionary<string, string> columnsToAdd = new Dictionary<string, string>
        {
            {
                //added at v1.2.0
                "ClientName",
                "ALTER TABLE Settings ADD COLUMN ClientName TEXT"
            },
            {
                //added at v1.2.2
                "FileNameFilter2",
                "ALTER TABLE Settings ADD COLUMN FileNameFilter2 TEXT"
            },
            {
                //added at v2.0.0
                "DrgPackage",
                "ALTER TABLE TransmittalItems ADD COLUMN DrgPackage TEXT"
            },
            {
                //added at v2.0.0
                "UseRevit",
                "ALTER TABLE Settings ADD COLUMN UseRevit INTEGER"
            },
            {
                //added at v2.0.0
                "SheetPackageParamName",
                "ALTER TABLE Settings ADD COLUMN SheetPackageParamName TEXT"
            },
            {
                //added at v2.0.0
                "SheetPackageParamGuid",
                "ALTER TABLE Settings ADD COLUMN SheetPackageParamGuid TEXT"
            },
        };

        WaitForLockFileToClear(dbFilePath);
        CreateLockFile(dbFilePath);

        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
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


    // To prevent concurrent write attempts on the database a lock file will be created
    private void WaitForLockFileToClear(string dbFilePath)
    {
        bool loggedWaitingMessage = false;

        var lockFilePath = $"{dbFilePath.ParsePathWithEnvironmentVariables()}.lock";
        while (File.Exists(lockFilePath))
        {
            if(!loggedWaitingMessage)
            {
                _logger.LogInformation("Waiting for lock file [{lockFilePath}] to clear", lockFilePath);
                loggedWaitingMessage = true;
            }
            Thread.Sleep(100);
        }
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
}

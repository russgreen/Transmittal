using Dapper;
using Humanizer;
using Microsoft.Data.Sqlite;
using System.Data;
using Transmittal.Library.Extensions;

namespace Transmittal.Library.DataAccess;

public class SQLiteDataAccess : IDataConnection
{
    public bool CheckConnection(string dbFilePath)
    {
        using (IDbConnection dbConnection = new SqliteConnection($"Data Source={dbFilePath.ParsePathWithEnvironmentVariables()}"))
        {
            try
            {
                dbConnection.Open();
                return true;
            }
            catch (SqliteException)
            {
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
                "ClientName",
                "ALTER TABLE Settings ADD COLUMN ClientName TEXT"
            },
            {
                "FileNameFilter2",
                "ALTER TABLE Settings ADD COLUMN FileNameFilter2 TEXT"
            }
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
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create column[{column.Key}]. Most likely it already exists, which is fine.");
                }
            }
        }

        DeleteLockFile(dbFilePath);
    }


    // To prevent concurrent write attempts on the database a lock file will be created
    private void WaitForLockFileToClear(string dbFilePath)
    {
        var lockFilePath = $"{dbFilePath.ParsePathWithEnvironmentVariables()}.lock";
        while (File.Exists(lockFilePath))
        {
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
    }

    private void DeleteLockFile(string dbFilePath)
    {
        var lockFilePath = $"{dbFilePath.ParsePathWithEnvironmentVariables()}.lock";
        File.Delete(lockFilePath);
    }
}

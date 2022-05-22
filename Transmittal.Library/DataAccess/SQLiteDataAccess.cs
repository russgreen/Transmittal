using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Transmittal.Library.DataAccess;

public class SQLiteDataAccess : IDataConnection
{
    public bool CheckConnection(string dbFilePath)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbFilePath))
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
        using (var dbConnection = new SqliteConnection(dbFilePath))
        {
            dbConnection.Open();
            var recordId = dbConnection.ExecuteScalar<int>(sqlStatement, parameters);

            //establish Id parameter of T (will not be the same name in every model)
            model.GetType().GetProperty(keyPropertyName).SetValue(model, recordId);

            return model;
        }
    }

    public IEnumerable<T> LoadData<T, U>(string dbFilePath, string sqlStatement, U parameters)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbFilePath))
        {
            dbConnection.Open();
            var rows = dbConnection.Query<T>(sqlStatement, parameters);
            return rows;
        }
    }

    public void SaveData<T>(string dbFilePath, string sqlStatement, T data)
    {
        using (IDbConnection dbConnection = new SqliteConnection(dbFilePath))
        {
            dbConnection.Open();
            dbConnection.Execute(sqlStatement, data);
        }
    }
}

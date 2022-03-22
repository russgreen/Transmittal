using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Transmittal.DataAccess;

internal class SQLiteDataAccess : IDataConnection
{
    private readonly string _cnnString;

    public SQLiteDataAccess(string cnnString)
    {
        _cnnString = cnnString;
    }

    public bool CheckConnection()
    {
        using (IDbConnection dbConnection = new SqliteConnection(_cnnString))
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

    public IEnumerable<T> LoadData<T, U>(string sqlStatement, U parameters)
    {
        using (IDbConnection dbConnection = new SqliteConnection(_cnnString))
        {
            dbConnection.Open();
            var rows = dbConnection.Query<T>(sqlStatement, parameters);
            return rows;
        }
    }

    public void SaveData<T>(string sqlStatement, T data)
    {
        using (IDbConnection dbConnection = new SqliteConnection(_cnnString))
        {
            dbConnection.Open();
            dbConnection.Execute(sqlStatement, data);
        }
    }

    public T CreateData<T, U>(string sqlStatement, T model, U parameters, string keyPropertyName)
    {
        using (var dbConnection = new SqliteConnection(_cnnString))
        {
            var recordId = dbConnection.ExecuteScalar<int>(sqlStatement, parameters);

            //establish Id parameter of T (will not be the same name in every model)
            model.GetType().GetProperty(keyPropertyName).SetValue(model, recordId);

            return model;
        }
    }


}

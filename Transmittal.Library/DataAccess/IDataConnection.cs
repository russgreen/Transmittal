namespace Transmittal.Library.DataAccess;

public interface IDataConnection
{
    /// <summary>
    /// Check if the connection to the database is working
    /// </summary>
    /// <returns></returns>
    bool CheckConnection(string dbFilePath);

    T CreateData<T, U>(string dbFilePath, string sqlStatement, T model, U parameters, string keyPropertyName);

    IEnumerable<T> LoadData<T, U>(string dbFilePath, string sqlStatement, U parameters);

    void SaveData<T>(string dbFilePath, string sqlStatement, T data);
}
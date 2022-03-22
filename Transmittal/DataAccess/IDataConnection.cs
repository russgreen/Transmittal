namespace Transmittal.DataAccess;

internal interface IDataConnection
{
    /// <summary>
    /// Check if the connection to the database is working
    /// </summary>
    /// <returns></returns>
    bool CheckConnection();

    T CreateData<T, U>(string sqlStatement, T model, U parameters, string keyPropertyName);

    IEnumerable<T> LoadData<T, U>(string sqlStatement, U parameters);

    void SaveData<T>(string sqlStatement, T data);
}
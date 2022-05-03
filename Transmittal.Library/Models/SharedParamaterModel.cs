namespace Transmittal.Library.Models;
public class SharedParamaterModel
{
    public SharedParamaterModel(string name, string guid)
    {
        Name = name;
        Guid = guid;
    }

    public string Name { get; set; }
    public string Guid { get; set; }
}

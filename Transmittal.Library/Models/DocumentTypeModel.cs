namespace Transmittal.Library.Models;
public class DocumentTypeModel
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string DisplayName
    {
        get
        {
            return $"{Code} - {Description}";
        }
    }
}
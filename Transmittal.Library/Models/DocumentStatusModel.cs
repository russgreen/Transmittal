namespace Transmittal.Library.Models;
public class DocumentStatusModel
{
    public int ID { get; set; }
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

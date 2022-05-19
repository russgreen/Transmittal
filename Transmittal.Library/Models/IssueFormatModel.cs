namespace Transmittal.Library.Models;
public class IssueFormatModel
{
    public int ID { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }

    public IssueFormatModel()
    {

    }

    public IssueFormatModel(string Code, string Description)
    {
        this.Code = Code;
        this.Description = Description;
    }

}

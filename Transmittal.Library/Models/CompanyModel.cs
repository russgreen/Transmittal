using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;

public class CompanyModel : BaseModel
{
    public int ID { get; set; }
    [Required(ErrorMessage = "A company name is required")]
    public string CompanyName { get; set; } = String.Empty; 
    public string Role { get; set; }
    public string Address { get; set; }
    public string Tel { get; set; }
    public string Fax { get; set; }
    public string Website { get; set; }
    public List<PersonModel> Contacts { get; set; }
}

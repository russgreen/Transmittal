using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class CompanyModel
{
    public int ID { get; set; }
    [Required]
    [StringLength(50)]
    public string CompanyName { get; set; }
    public string Address { get; set; }
    [StringLength(50)]
    public string Tel { get; set; }
    [StringLength(50)]
    public string Fax { get; set; }
    [StringLength(250)]
    public string Website { get; set; }
    public List<PersonModel> Contacts { get; set; }
}

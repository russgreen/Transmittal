using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class ProjectDirectoryModel
{
    public int ID { get; set; }
    [Required]
    public int PersonID { get; set; }
    [Required]
    public bool ShowInReport { get; set; }
    public string DisplayName
    {
        get
        {
            return $"{Person.FullNameReversed} ({Company.CompanyName})";
        }
    }
    public CompanyModel Company { get; set; }
    public PersonModel Person { get; set; } = new PersonModel();

}

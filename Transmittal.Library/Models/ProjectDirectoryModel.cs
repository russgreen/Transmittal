using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class ProjectDirectoryModel : BaseModel
{
    public int ID { get; set; }
    public string DisplayName => $"{Person.FullNameReversed} ({Company.CompanyName})";

    public CompanyModel Company { get; set; }
    public PersonModel Person { get; set; } 

}

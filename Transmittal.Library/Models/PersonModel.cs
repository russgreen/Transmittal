using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class PersonModel : BaseModel
{
    public int ID { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2)]
    public string LastName { get; set; } = String.Empty; 
    [Required(ErrorMessage = "At least provide an intial")]
    [MinLength(1)]
    public string FirstName { get; set; } = String.Empty;
    /// <summary>
    /// FirstName LastName
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    /// <summary>
    /// Lastname, Firstname
    /// </summary>
    public string FullNameReversed => $"{LastName}, {FirstName}";
    [EmailAddress]
    //[RegularExpressionAttribute(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    public string Email { get; set; }
    public string Tel { get; set; }
    public string Mobile { get; set; }
    public string Position { get; set; }
    public string Notes { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a company for the person")]
    public int CompanyID { get; set; }
    public bool ShowInReport { get; set; } = true;
}

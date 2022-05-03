using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class PersonModel
{
    public int ID { get; set; }
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    [StringLength(50)]
    public string FirstName { get; set; }
    /// <summary>
    /// FirstName LastName
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    /// <summary>
    /// Lastname, Firstname
    /// </summary>
    public string FullNameReversed => $"{LastName}, {FirstName}";
    [StringLength(50)]
    [RegularExpressionAttribute(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    public string Email { get; set; }
    [StringLength(50)]
    public string Tel { get; set; }
    [StringLength(50)]
    public string Mobile { get; set; }
    [Required]
    public int CompanyID { get; set; }
    public string Position { get; set; }
    public string Notes { get; set; }
}

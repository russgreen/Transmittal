using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class TransmittalDistributionModel : BaseModel
{
    public int TransDistID { get; set; }
    public int TransID { get; set; }
    [Required]
    [StringLength(10)]
    public string TransFormat { get; set; } = "E";
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
    public int TransCopies { get; set; } = 1;
    public int PersonID { get; set; }
}

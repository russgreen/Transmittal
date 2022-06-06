using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class TransmittalItemModel : BaseModel
{
    public int TransItemID { get; set; }
    public int TransID { get; set; }
    [Required]
    [StringLength(120)]
    public string DrgNumber { get; set; }
    [Required]
    [StringLength(10)]
    public string DrgRev { get; set; } = "P01";
    [Required]
    [StringLength(250)]
    public string DrgName { get; set; }
    [StringLength(50)]
    public string DrgPaper { get; set; }
    [StringLength(120)]
    public string DrgScale { get; set; }
    [Required]
    [StringLength(120)]
    public string DrgDrawn { get; set; } = "-";//= DefaultUserInitials();
    [StringLength(120)]
    public string DrgChecked { get; set; } = "-";
    [Required]
    [StringLength(10)]
    public string DrgProj { get; set; } = "0000";
    [StringLength(5)]
    public string DrgOriginator { get; set; } = "XXX"; //= GlobalConfig.GlobalSettings.BS1192Originator;
    [Required]
    [StringLength(2)]
    public string DrgVolume { get; set; } = "ZZ";
    [Required]
    [StringLength(2)]
    public string DrgLevel { get; set; } = "00";
    [Required]
    [StringLength(2)]
    public string DrgType { get; set; } = "DR";
    [Required]
    [StringLength(2)]
    public string DrgRole { get; set; } = "A";//= GlobalConfig.GlobalSettings.BS1192Role;
    /// <summary>
    /// ISO19650 Suitability Code
    /// </summary>
    [Required]
    [StringLength(5)]
    public string DrgStatus { get; set; } = "S0";
}

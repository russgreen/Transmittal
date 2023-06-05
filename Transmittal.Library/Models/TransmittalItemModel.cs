using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public partial class TransmittalItemModel : ObservableValidator
{
    [ObservableProperty]
    private int _transItemID;

    [ObservableProperty]
    private int _transID;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drgNumber;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drgRev = "P01";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drgName;

    [ObservableProperty]
    [StringLength(50)]
    private string _drgPaper;

    [ObservableProperty]
    [StringLength(120)]
    private string _drgScale;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drgDrawn = "-";//= DefaultUserInitials();

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [StringLength(120)]
    private string _drgChecked = "-";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drgProj = "0000";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [StringLength(5)]
    private string _drgOriginator = "XXX"; //= GlobalConfig.GlobalSettings.BS1192Originator;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [StringLength(4)]
    private string _drgVolume = "ZZ";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [StringLength(4)]
    private string _drgLevel = "00";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [StringLength(2)]
    private string _drgType = "DR";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [StringLength(2)]
    private string _drgRole = "A";//= GlobalConfig.GlobalSettings.BS1192Role;

    /// <summary>
    /// ISO19650 Suitability Code
    /// </summary>
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string _drgStatus = "S0";

    [ObservableProperty]
    private string _drgStatusDescription;
}

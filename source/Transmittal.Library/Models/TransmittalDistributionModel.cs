using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public partial class TransmittalDistributionModel : ProjectDirectoryModel
{
    public int TransDistID { get; set; }
    public int TransID { get; set; }



    [ObservableProperty]
    [Required]
    private string _transFormat = "E";

    [ObservableProperty]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
    private  int _transCopies = 1;

    [ObservableProperty]
    private int _personID;
}

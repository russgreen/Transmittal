using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public partial class PersonModel : ObservableValidator
{
    public int ID { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(FullNameReversed))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2)]
    private string _lastName = String.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(FullNameReversed))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "At least provide an intial")]
    [MinLength(1)]
    private string _firstName = String.Empty;
    /// <summary>
    /// FirstName LastName
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    /// <summary>
    /// Lastname, Firstname
    /// </summary>
    public string FullNameReversed => $"{LastName}, {FirstName}";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [EmailAddress]
    //[RegularExpressionAttribute(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$")]
    private string _email;

    [ObservableProperty]
    private string _tel;

    [ObservableProperty]
    private string _mobile;

    [ObservableProperty]
    private string _position;

    [ObservableProperty]
    private string _notes;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a company for the person")]
    private int _companyID;

    [ObservableProperty]
    private bool _showInReport = true;

    [ObservableProperty]
    private bool _archive = false;
}

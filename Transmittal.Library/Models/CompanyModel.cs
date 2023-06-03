using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;

public partial class CompanyModel : ObservableValidator
{
    [ObservableProperty]
    private int _iD;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "A company name is required")]
    private string _companyName;

    [ObservableProperty]
    private string _role;

    [ObservableProperty]
    private string _address;

    [ObservableProperty]
    private string _tel;

    [ObservableProperty]
    private string _fax;

    [ObservableProperty]
    private string _website;

    [ObservableProperty]
    private List<PersonModel> _contacts;
}

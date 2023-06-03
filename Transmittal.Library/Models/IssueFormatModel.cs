using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Transmittal.Library.Validation;

namespace Transmittal.Library.Models;
public partial class IssueFormatModel : ObservableValidator
{
    [ObservableProperty]
    private int _iD;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "A unique code is required.")]
    private string _code;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Description is required.")]
    private string _description;

    public IssueFormatModel()
    {

    }

    public IssueFormatModel(string code, string description)
    {
        this.Code = code;
        this.Description = description;
    }

}

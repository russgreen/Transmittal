using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public partial class DocumentStatusModel : ObservableValidator
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

    public string DisplayName
    {
        get
        {
            return $"{Code} - {Description}";
        }
    }

    public DocumentStatusModel()
    {

    }

    public DocumentStatusModel(string Code, string Description)
    {
        this.Code = Code;
        this.Description = Description;
    }
}

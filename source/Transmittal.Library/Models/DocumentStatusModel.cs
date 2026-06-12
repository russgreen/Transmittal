using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public partial class DocumentStatusModel : CodeDescriptionModel
{
    [ObservableProperty]
    private int _iD;

    public DocumentStatusModel()
    {

    }

    public DocumentStatusModel(string Code, string Description)
    {
        this.Code = Code;
        this.Description = Description;
    }
}

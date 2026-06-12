using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Library.Models;

public partial class CodeDescriptionModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "A unique code is required.")]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _code;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Description is required.")]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _description;

    public string DisplayName => $"{Code} - {Description}";
}

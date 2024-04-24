using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public partial class ProjectDirectoryModel : ObservableValidator
{
    [ObservableProperty]
    private int _iD;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private CompanyModel _company;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private PersonModel _person; 

    public string DisplayName => $"{Person.FullNameReversed} ({Company.CompanyName})";

}

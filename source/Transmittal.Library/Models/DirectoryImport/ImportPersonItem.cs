using CommunityToolkit.Mvvm.ComponentModel;

namespace Transmittal.Library.Models.DirectoryImport;

/// <summary>
/// A person read from the source database, with the user's import selection.
/// </summary>
public partial class ImportPersonItem : ObservableObject
{
    public ImportPersonItem(PersonModel person, ImportCompanyItem parent)
    {
        Person = person;
        Parent = parent;
    }

    public PersonModel Person { get; }

    public ImportCompanyItem Parent { get; }

    /// <summary>
    /// True when an equivalent contact (same email, or same name under the same company) is already in the target database.
    /// </summary>
    public bool ExistsInTarget { get; set; }

    public string DisplayName => ExistsInTarget
        ? $"{Person.FullNameReversed} (already exists)"
        : Person.FullNameReversed;

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        Parent?.OnPersonSelectionChanged(value);
    }
}

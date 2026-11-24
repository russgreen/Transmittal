using CommunityToolkit.Mvvm.ComponentModel;

namespace Transmittal.Library.Models.DirectoryImport;

/// <summary>
/// A company read from the source database together with its people and the user's import selection.
/// Ticking the company ticks all of its people. Ticking a person ticks the company, but leaves the other people alone.
/// </summary>
public partial class ImportCompanyItem : ObservableObject
{
    private bool _syncing;

    public ImportCompanyItem(CompanyModel company)
    {
        Company = company;
    }

    public CompanyModel Company { get; }

    public List<ImportPersonItem> People { get; } = new();

    /// <summary>
    /// True when a company with the same name is already in the target database.
    /// </summary>
    public bool ExistsInTarget { get; set; }

    public string DisplayName => ExistsInTarget
        ? $"{Company.CompanyName} (already exists)"
        : Company.CompanyName;

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            foreach (var person in People)
            {
                person.IsSelected = value;
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    internal void OnPersonSelectionChanged(bool personSelected)
    {
        if (_syncing || !personSelected || IsSelected)
        {
            return;
        }

        _syncing = true;
        try
        {
            IsSelected = true;
        }
        finally
        {
            _syncing = false;
        }
    }
}

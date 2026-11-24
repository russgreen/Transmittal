namespace Transmittal.Library.Models.DirectoryImport;

public class DirectoryImportResult
{
    /// <summary>Companies that were not in the target and were added.</summary>
    public int CompaniesCreated { get; set; }

    /// <summary>Companies that matched an existing target company by name and were reused.</summary>
    public int CompaniesReused { get; set; }

    /// <summary>People added to the target.</summary>
    public int PeopleCreated { get; set; }

    /// <summary>Selected people skipped because an equivalent contact is already in the target.</summary>
    public int PeopleSkipped { get; set; }

    /// <summary>Companies or people that could not be written to the target.</summary>
    public int Failures { get; set; }
}

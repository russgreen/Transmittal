namespace Transmittal.Library.Models.DirectoryImport;

/// <summary>
/// The directory (companies and people) read from another Transmittal database.
/// </summary>
public class DirectoryImportSource
{
    public string FilePath { get; set; } = string.Empty;

    public string ProjectNumber { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public List<ImportCompanyItem> Companies { get; } = new();
}

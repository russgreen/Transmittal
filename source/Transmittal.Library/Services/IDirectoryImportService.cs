using System.IO;
using Transmittal.Library.Models.DirectoryImport;

namespace Transmittal.Library.Services;

/// <summary>
/// Copies companies and people from another Transmittal database into the open one,
/// without duplicating anything that is already there.
/// </summary>
public interface IDirectoryImportService
{
    /// <summary>
    /// Reads the directory from another database without modifying it.
    /// Items already in the open database are flagged and left unticked.
    /// </summary>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidOperationException">The file is the open database, or is not a Transmittal database.</exception>
    DirectoryImportSource LoadSource(string sourcePath);

    /// <summary>
    /// Adds the ticked companies and people to the open database.
    /// Companies are matched by name (case-insensitive) and reused. People are skipped when the
    /// same email, or the same name under the same company, is already present.
    /// </summary>
    DirectoryImportResult Import(DirectoryImportSource source);
}

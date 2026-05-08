using System.IO;

namespace Transmittal.Browser.Models;

public sealed class TransferFileItem
{
    public required string FilePath { get; init; }
    public string FileName => Path.GetFileName(FilePath);
    public bool Exists => File.Exists(FilePath);
}

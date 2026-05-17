using System.IO;

namespace Transmittal.Browser.Models;

public sealed class TransferFileItem
{
    public required string FilePath { get; init; }
    public string FileName => Path.GetFileName(FilePath);
    public bool Exists => File.Exists(FilePath);

    public string? IconUri => Path.GetExtension(FilePath).ToLowerInvariant() switch
    {
        ".pdf" => "pack://application:,,,/Transmittal.Browser;component/Resources/pdfFile.png",
        ".dwg" => "pack://application:,,,/Transmittal.Browser;component/Resources/dwgFile.png",
        ".dwf" => "pack://application:,,,/Transmittal.Browser;component/Resources/dwfFile.png",
        _ => null
    };

    public bool HasIcon => IconUri is not null;
}

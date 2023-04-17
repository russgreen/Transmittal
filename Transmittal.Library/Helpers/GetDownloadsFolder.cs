using System.Runtime.InteropServices;

namespace Transmittal.Library.Helpers;
public static class GetDownloadsFolder
{
    public static string GetDownloadsPath()
    {
        return SHGetKnownFolderPath(new("374DE290-123F-4565-9164-39C4925E467B"), 0);
    }

    [DllImport("shell32",
        CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    private static extern string SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
        nint hToken = 0);
}

using System.Diagnostics;
using System.Windows.Interop;

namespace Transmittal;
/// <summary>
/// Wrapper class for converting
/// IntPtr to IWin32Window.
/// </summary>
public class WindowHandle : IWin32Window
{
    private readonly IntPtr _hwnd;

    public WindowHandle(IntPtr h)
    {
        Debug.Assert(IntPtr.Zero != h, "expected non-null window handle");
        _hwnd = h;
    }

    public IntPtr Handle
    {
        get
        {
            return _hwnd;
        }
    }
}

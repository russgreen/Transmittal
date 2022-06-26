using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace AssociateFileExtensions;

internal class Program
{
    static void Main(string[] args)
    {

        //get executable path
        var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

#if DEBUG
        path = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, @"..\..\..\..\Transmittal.Desktop\bin\x64\Debug\net48"));
#endif

        SetAssociation(".tdb", "Transmittal.Database",  Path.Combine(path, "Transmittal.Desktop.exe"), "Transmittal Database File");
    }

    private static void SetAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
    {
        // The stuff that was above here is basically the same
        RegistryKey BaseKey;
        RegistryKey OpenMethod;
        RegistryKey Shell;
        RegistryKey CurrentUser;

        BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
        BaseKey.SetValue("", KeyName);

        OpenMethod = Registry.ClassesRoot.CreateSubKey(KeyName);
        OpenMethod.SetValue("", FileDescription);
        OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
        Shell = OpenMethod.CreateSubKey("Shell");
        Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " --database=\"%1\"");
        BaseKey.Close();
        OpenMethod.Close();
        Shell.Close();

        // Delete the key instead of trying to change it
        CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
        CurrentUser.DeleteSubKey("UserChoice", false);
        CurrentUser.Close();

        // Tell explorer the file association has been changed
        SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
}

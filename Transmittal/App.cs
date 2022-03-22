using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Transmittal;
public class App : IExternalApplication
{
    // get the absolute path of this assembly
    public static readonly string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

    // class instance
    public static App ThisApp;

    public static UIControlledApplication CachedUiCtrApp;
    public static UIApplication CachedUiApp;
    public static ControlledApplication CtrApp;
    public static Autodesk.Revit.DB.Document RevitDocument;

    private AppDocEvents _appEvents;
    private readonly string _tabName = "Transmittal";
    public Result OnStartup(UIControlledApplication application)
    {
        ThisApp = this;
        CachedUiCtrApp = application;
        CtrApp = application.ControlledApplication;

        var panel = RibbonPanel(application);

        AddAppDocEvents();

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        RemoveAppDocEvents();

        return Result.Succeeded;
    }

    #region Event Handling
    private void AddAppDocEvents()
    {
        _appEvents = new AppDocEvents();
        _appEvents.EnableEvents();
    }
    private void RemoveAppDocEvents()
    {
        _appEvents.DisableEvents();
    }


    #endregion

    #region Ribbon Panel

    private RibbonPanel RibbonPanel(UIControlledApplication application)
    {

        try
        {
            CachedUiCtrApp.CreateRibbonTab(_tabName);
        }
        catch { }

        RibbonPanel panel = CachedUiCtrApp.CreateRibbonPanel(_tabName, "Transmittal_Panel");
        panel.Title = "Transmittal";

        PushButton button = (PushButton)panel.AddItem(
            new PushButtonData(
                "Command",
                "Command",
                Assembly.GetExecutingAssembly().Location,
                "Transmittal.Command"));
        button.ToolTip = "Execute the Transmittal command";
        button.LargeImage = PngImageSource("Transmittal.Resources.Transmittal_Button.png");

        return panel;
    }

    private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
    {
        var stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
        System.Windows.Media.ImageSource imageSource;
        try
        {
            imageSource = BitmapFrame.Create(stream);
        }
        catch
        {
            imageSource = null;
        }

        return imageSource;
    }
    #endregion
}

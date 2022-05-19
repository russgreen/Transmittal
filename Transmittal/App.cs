using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Reflection;
using System.Windows.Media.Imaging;
using Transmittal.Library.Services;
using Transmittal.Library.DataAccess;
using Transmittal.Services;
using Microsoft.Extensions.DependencyInjection;

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

    public static IServiceProvider ServiceProvider;

    private AppDocEvents _appEvents;
    private readonly string _tabName = "Transmittal";
    
    public Result OnStartup(UIControlledApplication application)
    {
        ThisApp = this;
        CachedUiCtrApp = application;
        CtrApp = application.ControlledApplication;

        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddTransient<ISettingsServiceRvt, SettingsServiceRvt>()
            .AddTransient<IDataConnection, SQLiteDataAccess>()
            .AddTransient<IExportPDFService, ExportPDFService>()
            .AddTransient<IExportDWGService, ExportDWGService>()
            .AddTransient<IExportDWFService, ExportDWFService>()
            .AddTransient<IContactDirectoryService, ContactDirectoryService>()
            .AddTransient<ITransmittalService, TransmittalService>()
            .BuildServiceProvider());

        // building the ribbon panel
        var panel = RibbonPanel(application);

        AddAppDocEvents();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("##SyncfusionLicense##");

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
        
        PushButton buttonTransmittal = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandTransmittal),
                "Transmittal",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandTransmittal)}"));
        buttonTransmittal.ToolTip = "Execute the Transmittal command";
        buttonTransmittal.LargeImage = PngImageSource("Transmittal.Resources.Transmittal_Button.png");

        PushButton buttonDirectory = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandDirectory),
                "Project Directory",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandDirectory)}"));
        buttonDirectory.ToolTip = "Edit the the project directory";
        buttonDirectory.LargeImage = PngImageSource("Transmittal.Resources.Directory_Button.png");

        PushButton buttonTransmittalHistory = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandTransmittalsArchive),
                "Transmittal Archive",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandTransmittalsArchive)}"));
        buttonTransmittalHistory.ToolTip = "View the transmittal archive";
        buttonTransmittalHistory.LargeImage = PngImageSource("Transmittal.Resources.Archive_Button.png");

        PushButton buttonSettings = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandSettings),
                "Settings",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandSettings)}"));
        buttonSettings.ToolTip = "Edit the settings";
        buttonSettings.LargeImage = PngImageSource("Transmittal.Resources.Settings_Button.png");

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

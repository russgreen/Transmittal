using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Reflection;
using System.Windows.Media.Imaging;
using Transmittal.Library.Services;
using Transmittal.Library.DataAccess;
using Transmittal.Services;
using Microsoft.Extensions.DependencyInjection;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

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

    private RibbonPanel _ribbonPanel;
    
    public Result OnStartup(UIControlledApplication application)
    {
        ThisApp = this;
        CachedUiCtrApp = application;
        CtrApp = application.ControlledApplication;

        CachedUiCtrApp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);

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
        _ribbonPanel = RibbonPanel(application);

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

    private void OnViewActivated(object sender, ViewActivatedEventArgs e)
    {
        _ribbonPanel.Enabled = true;

        if (e.Document.IsFamilyDocument)
        {
            _ribbonPanel.Enabled = false;
        }

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
        buttonTransmittal.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/transmittal/"));

        PushButton buttonDirectory = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandDirectory),
                $"Project Directory",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandDirectory)}"));
        buttonDirectory.ToolTip = "Edit the the project directory";
        buttonDirectory.LargeImage = PngImageSource("Transmittal.Resources.Directory_Button.png");
        buttonDirectory.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/directory/"));

        PushButton buttonTransmittalArchive = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandTransmittalsArchive),
                $"Transmittal Archive",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandTransmittalsArchive)}"));
        buttonTransmittalArchive.ToolTip = "View the transmittal archive";
        buttonTransmittalArchive.LargeImage = PngImageSource("Transmittal.Resources.Archive_Button.png");
        buttonTransmittalArchive.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/archive/"));

        PushButton buttonSettings = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.CommandSettings),
                "Settings",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.CommandSettings)}"));
        buttonSettings.ToolTip = "Edit the settings";
        buttonSettings.LargeImage = PngImageSource("Transmittal.Resources.Settings_Button.png");
        buttonSettings.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

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

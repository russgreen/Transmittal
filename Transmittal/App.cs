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

    public static IServiceProvider ServiceProvider;

    private AppDocEvents _appEvents;
    private string _tabName = "Transmittal";

    private RibbonPanel _ribbonPanel;
    
    public Result OnStartup(UIControlledApplication application)
    {
        ThisApp = this;
        CachedUiCtrApp = application;
        CtrApp = application.ControlledApplication;

        CachedUiCtrApp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);

        Host.StartHost();

        //allow end users to customise the ribbon tab name
        var currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var customTabNameFile = System.IO.Path.Combine(currentPath, "ribbontab.txt");

        if(System.IO.File.Exists(customTabNameFile))
        {
            _tabName = System.IO.File.ReadLines(customTabNameFile).First();
        }

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
                nameof(Transmittal.Commands.CommandTransmittal),
                "Transmittal",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandTransmittal)}"));
        buttonTransmittal.ToolTip = "Execute the Transmittal command";
        buttonTransmittal.LargeImage = PngImageSource("Transmittal.Resources.Transmittal_Button.png");
        buttonTransmittal.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/transmittal/"));


        PushButton buttonDirectory = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandDirectory),
                $"Project{Environment.NewLine}Directory",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandDirectory)}"));
        buttonDirectory.ToolTip = "Edit the the project directory";
        buttonDirectory.LargeImage = PngImageSource("Transmittal.Resources.Directory_Button.png");
        buttonDirectory.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/directory/"));

        PushButton buttonTransmittalArchive = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandTransmittalsArchive),
                $"Transmittal{Environment.NewLine}Archive",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandTransmittalsArchive)}"));
        buttonTransmittalArchive.ToolTip = "View the transmittal archive";
        buttonTransmittalArchive.LargeImage = PngImageSource("Transmittal.Resources.Archive_Button.png");
        buttonTransmittalArchive.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/archive/"));

        SplitButtonData splitButtonData = new SplitButtonData("SettingsSplit", "Settings");
        SplitButton splitButton = panel.AddItem(splitButtonData) as SplitButton;

        PushButton pushButton = splitButton.AddPushButton(
            new PushButtonData("Settings", "Settings",
            Assembly.GetExecutingAssembly().Location,
            $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandSettings)}"));
        pushButton.ToolTip = "Edit the settings";
        pushButton.LargeImage = PngImageSource("Transmittal.Resources.Settings_Button.png");
        pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

        pushButton = splitButton.AddPushButton(
            new PushButtonData("ImportSettings", "Import Settings",
            Assembly.GetExecutingAssembly().Location,
            $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandImportSettings)}"));
        pushButton.ToolTip = "Import settings from template file";
        pushButton.LargeImage = PngImageSource("Transmittal.Resources.Import_Button.png");
        pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

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

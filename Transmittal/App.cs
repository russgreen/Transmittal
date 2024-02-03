using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Nice3point.Revit.Toolkit.External;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Transmittal;

public class App : ExternalApplication
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
    private string _tabName = "Transmittal";

    private RibbonPanel _ribbonPanel;

    public override void OnStartup()
    {
        ThisApp = this;
        CachedUiCtrApp = Application;
        CtrApp = Application.ControlledApplication;

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
        _ribbonPanel = RibbonPanel(Application);

        AddAppDocEvents();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("##SyncfusionLicense##");
    }

    public override void OnShutdown()
    {
        RemoveAppDocEvents();

        Host.StopHost();
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

        var panel = CachedUiCtrApp.CreateRibbonPanel(_tabName, "Transmittal_Panel");
        panel.Title = "Transmittal";
        
        var buttonTransmittal = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandTransmittal),
                "Transmittal",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandTransmittal)}"));
        buttonTransmittal.ToolTip = "Execute the Transmittal command";
        buttonTransmittal.LargeImage = PngImageSource("Transmittal.Resources.Transmittal_Button.png");
        buttonTransmittal.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/transmittal/"));


        var buttonDirectory = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandDirectory),
                $"Project{Environment.NewLine}Directory",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandDirectory)}"));
        buttonDirectory.ToolTip = "Edit the the project directory";
        buttonDirectory.LargeImage = PngImageSource("Transmittal.Resources.Directory_Button.png");
        buttonDirectory.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/directory/"));

        var buttonTransmittalArchive = (PushButton)panel.AddItem(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandTransmittalsArchive),
                $"Transmittal{Environment.NewLine}Archive",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandTransmittalsArchive)}"));
        buttonTransmittalArchive.ToolTip = "View the transmittal archive";
        buttonTransmittalArchive.LargeImage = PngImageSource("Transmittal.Resources.Archive_Button.png");
        buttonTransmittalArchive.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/archive/"));

        var splitButtonData = new SplitButtonData("SettingsSplit", "Settings");
        var splitButton = panel.AddItem(splitButtonData) as SplitButton;
        splitButton.IsSynchronizedWithCurrentItem = false;

        var pushButton = splitButton.AddPushButton(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandSettings),  
                "Settings",
            Assembly.GetExecutingAssembly().Location,
            $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandSettings)}"));
        pushButton.ToolTip = "Edit the settings";
        pushButton.LargeImage = PngImageSource("Transmittal.Resources.Settings_Button.png");
        pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

        pushButton = splitButton.AddPushButton(
            new PushButtonData(
                nameof(Transmittal.Commands.CommandImportSettings), 
                "Import Settings",
            Assembly.GetExecutingAssembly().Location,
            $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandImportSettings)}"));
        pushButton.ToolTip = "Import settings from template file";
        pushButton.LargeImage = PngImageSource("Transmittal.Resources.Import_Button.png");
        pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

        pushButton = splitButton.AddPushButton(
        new PushButtonData(nameof(Transmittal.Commands.CommandAbout), 
            "About",
        Assembly.GetExecutingAssembly().Location,
        $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandAbout)}"));
        pushButton.ToolTip = "About Transmittal";
        pushButton.LargeImage = PngImageSource("Transmittal.Resources.About_Button.png");
        pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/archive/"));

        return panel;
    }

    private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedPath);
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

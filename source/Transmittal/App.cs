using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Nice3point.Revit.Toolkit.External;
using SQLitePCL;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Transmittal;

public class App : ExternalApplication
{
    // get the absolute path of this assembly
    //public static readonly string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
    public static readonly string DesktopAssemblyFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Transmittal");

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
        var customTabNameFile = System.IO.Path.Combine(App.DesktopAssemblyFolder, "ribbontab.txt");

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

        var buttonDataDirectory = new PushButtonData(
                nameof(Transmittal.Commands.CommandDirectory),
                $"Project{Environment.NewLine}Directory",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandDirectory)}");
        buttonDataDirectory.ToolTip = "Edit the the project directory";
        buttonDataDirectory.Image = PngImageSource("Transmittal.Resources.Directory_Button_Small.png");
        buttonDataDirectory.LargeImage = PngImageSource("Transmittal.Resources.Directory_Button.png");
        buttonDataDirectory.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/directory/"));

        var buttonDataArchive = new PushButtonData(
                nameof(Transmittal.Commands.CommandTransmittalsArchive),
                $"Transmittal{Environment.NewLine}Archive",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandTransmittalsArchive)}");
        buttonDataArchive.ToolTip = "View the transmittal archive";
        buttonDataArchive.Image = PngImageSource("Transmittal.Resources.Archive_Button_Small.png");
        buttonDataArchive.LargeImage = PngImageSource("Transmittal.Resources.Archive_Button.png");
        buttonDataArchive.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/archive/"));

        var splitButtonData = new SplitButtonData("SettingsSplit", "Settings");

        var buttonDataSettings = new PushButtonData(
                nameof(Transmittal.Commands.CommandSettings),
                $"Settings",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandSettings)}");
        buttonDataSettings.ToolTip = "Edit the settings";
        buttonDataSettings.Image = PngImageSource("Transmittal.Resources.Settings_Button_Small.png");
        buttonDataSettings.LargeImage = PngImageSource("Transmittal.Resources.Settings_Button.png");
        buttonDataSettings.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

        var buttonDataImportSettings = new PushButtonData(
                nameof(Transmittal.Commands.CommandImportSettings),
                $"Import{Environment.NewLine}Settings",
                Assembly.GetExecutingAssembly().Location,
                $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandImportSettings)}");
        buttonDataImportSettings.ToolTip = "Import settings from template file";
        buttonDataImportSettings.Image = PngImageSource("Transmittal.Resources.Import_Button_Small.png");
        buttonDataImportSettings.LargeImage = PngImageSource("Transmittal.Resources.Import_Button.png");
        buttonDataImportSettings.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/settings/"));

        var buttonDataAbout = new PushButtonData(nameof(Transmittal.Commands.CommandAbout),
            "About",
        Assembly.GetExecutingAssembly().Location,
        $"{nameof(Transmittal)}.{nameof(Transmittal.Commands)}.{nameof(Transmittal.Commands.CommandAbout)}");
        buttonDataAbout.ToolTip = "About Transmittal";
        buttonDataAbout.Image = PngImageSource("Transmittal.Resources.About_Button_Small.png");
        buttonDataAbout.LargeImage = PngImageSource("Transmittal.Resources.About_Button.png");
        buttonDataAbout.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://russgreen.github.io/Transmittal/archive/"));

        if (_tabName != "Transmittal")
        {

            var stackedItems = panel.AddStackedItems(buttonDataDirectory, buttonDataArchive, splitButtonData);

            var split = stackedItems.Last() as SplitButton;
            split.IsSynchronizedWithCurrentItem = false;

            split.AddPushButton(buttonDataSettings);
            split.AddPushButton(buttonDataImportSettings);
            split.AddPushButton(buttonDataAbout);
        }
        else
        {
            panel.AddItem(buttonDataDirectory);
            panel.AddItem(buttonDataArchive);

            var splitButton = panel.AddItem(splitButtonData) as SplitButton;
            splitButton.IsSynchronizedWithCurrentItem = false;

            splitButton.AddPushButton(buttonDataSettings);
            splitButton.AddPushButton(buttonDataImportSettings);
            splitButton.AddPushButton(buttonDataAbout);
        }
            

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

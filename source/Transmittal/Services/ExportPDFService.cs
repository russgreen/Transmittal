using Autodesk.Revit.DB;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using Transmittal.Extensions;
using Transmittal.Library.Extensions;
using Transmittal.Library.Services;

namespace Transmittal.Services;
internal class ExportPDFService : IExportPDFService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ExportPDFService> _logger;

    public ExportPDFService(ISettingsService settingsService, 
        ILogger<ExportPDFService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

#if REVIT2022_OR_GREATER
    public string ExportPDF(string exportFileName, string folderPath, Document exportDocument, ViewSet views, PDFExportOptions pdfExportOptions, bool RecordError = true)
    {
        var fullPath = string.Empty;

        Transaction trans = null;
        try
        {
            trans = new Transaction(App.RevitDocument, "Export PDF");
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);
            trans.Start();

            // configure filename path for final PDF save location
            fullPath = Path.Combine(folderPath, exportFileName);

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            var sheet = views.OfType<ViewSheet>().FirstOrDefault();

            var viewIDs = new List<ElementId>();
            viewIDs.Add(sheet.Id);

            var paper = sheet.GetTitleBlockFamilyInstance().GetPaperSizeModel();

            pdfExportOptions.FileName = exportFileName;

            if (paper.Width > paper.Height)
            {
                pdfExportOptions.PaperOrientation = PageOrientationType.Landscape;
            }
            else
            {
                pdfExportOptions.PaperOrientation = PageOrientationType.Portrait;
            }

            App.RevitDocument.Export(folderPath, viewIDs, pdfExportOptions);
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "Error exporting pdf file");
        }
        finally
        {
            trans.RollBack();
        }

        if (!fullPath.EndsWith(".pdf"))
        {
            fullPath = $"{fullPath}.pdf";
        }

        return fullPath;
    }

#endif

    public string PrintPDF(string exportFileName, string folderPath, Document exportDocument, ViewSet views, PDFExportOptions pdfExportOptions, bool RecordError = true)
    {
        var fullPath = string.Empty;
        string currentDefaultPrinterName;
        string pdfPrinterName = "PDF24";

        PDF24Settings pDF24Settings = GetCurrentPDF24Settings();
       
        ViewSheet sheet = null;
        foreach (var view in views)
        {
            if (view is ViewSheet)
            {
                sheet = view as ViewSheet;
                break; //there is only one sheet in the set
            }
        }

        Transaction trans = null;
        try
        {
            currentDefaultPrinterName = System.Printing.LocalPrintServer.GetDefaultPrintQueue().FullName;
            SetDefaultPrinter(pdfPrinterName);

            trans = new Transaction(App.RevitDocument, "Export PDF");
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);
            trans.Start();

            fullPath = Path.Combine(folderPath, exportFileName);

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists($"{fullPath}.pdf") == true)
            {
                try
                {
                    File.Delete($"{fullPath}.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting file pdf");
                    //TODO append a date time to the file name
                    exportFileName = $"{exportFileName} ({DateTime.Now.ToLongTimeString().Replace(":", "")})";
                    fullPath = Path.Combine(folderPath, exportFileName);
                }
            }

            if (!fullPath.EndsWith(".pdf"))
            {
                fullPath = $"{fullPath}.pdf";
            }

            var paper = sheet.GetTitleBlockFamilyInstance().GetPaperSizeModel();

            PDF24Settings transmittalPDF24Settings = new()
            {
                AutoSaveDir = folderPath,
                AutoSaveFileCmd = "",
                AutoSaveFilename = exportFileName,
                AutoSaveOpenDir = false,
                AutoSaveOverwrite = true,
                AutoSaveOverwriteFile = true,
                AutoSaveProfile = "default/best",
                AutoSaveShowProgress = false,
                AutoSaveUseFileCmd = false,
                FilenameErasements = "",
                Handler = "autoSave",
                LoadInCreatorIfOpen = false,
                ShellCmd = ""
            };

            SetPDF24Settings(transmittalPDF24Settings);

            //set the print manager 
            PrintManager printManager = App.RevitDocument.PrintManager;
            printManager.PrintSetup.CurrentPrintSetting = printManager.PrintSetup.InSession;
            printManager.SelectNewPrintDriver(pdfPrinterName);
            printManager.PrintToFile = true;
            printManager.PrintToFileName = fullPath;
            printManager.PrintRange = Autodesk.Revit.DB.PrintRange.Select;
            printManager.Apply();

            var printViewSet = new ViewSet();
            printViewSet.Insert(sheet);
            var viewSheetSetting = printManager.ViewSheetSetting;
            viewSheetSetting.CurrentViewSheetSet.Views = printViewSet;
            viewSheetSetting.SaveAs("transmittal");

            printManager.PrintSetup.InSession.PrintParameters.ZoomType = ZoomType.Zoom;
            printManager.PrintSetup.InSession.PrintParameters.Zoom = 100;
            if (paper.Width > paper.Height)
            {
                // pParams.PageOrientation = PageOrientationType.Landscape
                printManager.PrintSetup.InSession.PrintParameters.PageOrientation = PageOrientationType.Landscape;
            }
            else
            {
                // pParams.PageOrientation = PageOrientationType.Portrait
                printManager.PrintSetup.InSession.PrintParameters.PageOrientation = PageOrientationType.Portrait;
            }

            printManager.PrintSetup.InSession.PrintParameters.PaperPlacement = PaperPlacementType.Center;
            printManager.PrintSetup.InSession.PrintParameters.ColorDepth = pdfExportOptions.ColorDepth;
            printManager.PrintSetup.InSession.PrintParameters.RasterQuality = pdfExportOptions.RasterQuality;
            printManager.PrintSetup.InSession.PrintParameters.HiddenLineViews = HiddenLineViewsType.VectorProcessing;
            printManager.PrintSetup.InSession.PrintParameters.ViewLinksinBlue = pdfExportOptions.ViewLinksInBlue;
            printManager.PrintSetup.InSession.PrintParameters.HideReforWorkPlanes = pdfExportOptions.HideReferencePlane;
            printManager.PrintSetup.InSession.PrintParameters.HideUnreferencedViewTags = pdfExportOptions.HideUnreferencedViewTags;
            printManager.PrintSetup.InSession.PrintParameters.HideCropBoundaries = pdfExportOptions.HideCropBoundaries;
            printManager.PrintSetup.InSession.PrintParameters.HideScopeBoxes = pdfExportOptions.HideScopeBoxes;
            printManager.PrintSetup.InSession.PrintParameters.ReplaceHalftoneWithThinLines = pdfExportOptions.ReplaceHalftoneWithThinLines;
            printManager.PrintSetup.InSession.PrintParameters.MaskCoincidentLines = pdfExportOptions.MaskCoincidentLines;

            string PaperSource = "<default tray>";
            foreach (Autodesk.Revit.DB.PaperSource ps in printManager.PaperSources)
            {
                if (ps.Name.Equals(PaperSource))
                {
                    printManager.PrintSetup.CurrentPrintSetting.PrintParameters.PaperSource = ps;
                    break;
                }
            }

            //string PaperSize = Util.GetPapersize(Width, Height);
            foreach (Autodesk.Revit.DB.PaperSize ps in printManager.PaperSizes)
            {
                // TODO - Handle custom paper sizes
                if (ps.Name.Equals(paper.Name))//PaperSize))
                {
                    // pParams.PaperSize = ps
                    printManager.PrintSetup.InSession.PrintParameters.PaperSize = ps;
                    break;
                }
                else
                {
                    // TODO - Pick the next largest papersize to ensure it fits.
                }
            }

            try
            {
                printManager.PrintSetup.SaveAs("transmittal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving print setup");
            }

            printManager.Apply();
            printManager.SubmitPrint(); // (sheet)

            //wait for the print to finish
            while (!File.Exists(fullPath))
            {
                System.Threading.Thread.Sleep(500);
            }

            viewSheetSetting.Delete();

            SetDefaultPrinter(currentDefaultPrinterName);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating pdf");
        }
        finally
        {

            trans.RollBack();
        }

        SetPDF24Settings(pDF24Settings);

        return fullPath;
    }

    private PDF24Settings GetCurrentPDF24Settings()
    {
        PDF24Settings settings;

        try
        {
            settings = new()
            {
                AutoSaveDir = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveDir", "").ToString(),
                AutoSaveFileCmd = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveFileCmd", "").ToString(),
                AutoSaveFilename = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveFilename", "").ToString(),
                AutoSaveProfile = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveProfile", "").ToString(),
                FilenameErasements = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "FilenameErasements", "").ToString(),
                Handler = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "Handler", "").ToString(),
                ShellCmd = Registry.GetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "ShellCmd", "").ToString(),

                AutoSaveOpenDir = GetRegistryVal_Bool(@"Software\PDF24\Services\PDF", "AutoSaveOpenDir"),
                AutoSaveOverwrite = GetRegistryVal_Bool(@"Software\PDF24\Services\PDF", "AutoSaveOverwrite"),
                AutoSaveOverwriteFile = GetRegistryVal_Bool(@"Software\PDF24\Services\PDF", "AutoSaveOverwriteFile"),
                AutoSaveShowProgress = GetRegistryVal_Bool(@"Software\PDF24\Services\PDF", "AutoSaveShowProgress"),
                AutoSaveUseFileCmd = GetRegistryVal_Bool(@"Software\PDF24\Services\PDF", "AutoSaveUseFileCmd"),
                LoadInCreatorIfOpen = GetRegistryVal_Bool(@"Software\PDF24\Services\PDF", "LoadInCreatorIfOpen")
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting PDF24 settings from registry");

            settings = new()
            {
                AutoSaveDir = @"%USERPROFILE%\Documents\Transmittal\Temp\PDF",
                AutoSaveFileCmd = "",
                AutoSaveFilename = "$fileName",
                AutoSaveOpenDir = false,
                AutoSaveOverwrite = true,
                AutoSaveOverwriteFile = true,
                AutoSaveProfile = "default/best",
                AutoSaveShowProgress = false,
                AutoSaveUseFileCmd = false,
                FilenameErasements = "",
                Handler = "autoSave",
                LoadInCreatorIfOpen = false,
                ShellCmd = ""
            };
        }

        return settings;
    }

    private void SetPDF24Settings(PDF24Settings transmittalPDF24Settings)
    {
        //save values to the registry key HKEY_CURRENT_USER\Software\PDF24\Services\PDF
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveDir", transmittalPDF24Settings.AutoSaveDir);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveFileCmd", transmittalPDF24Settings.AutoSaveFileCmd);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveFilename", transmittalPDF24Settings.AutoSaveFilename);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveOpenDir", Convert.ToInt32(transmittalPDF24Settings.AutoSaveOpenDir), RegistryValueKind.DWord);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveOverwrite", Convert.ToInt32(transmittalPDF24Settings.AutoSaveOverwrite), RegistryValueKind.DWord);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveOverwriteFile", Convert.ToInt32(transmittalPDF24Settings.AutoSaveOverwriteFile), RegistryValueKind.DWord);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveProfile", transmittalPDF24Settings.AutoSaveProfile);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveShowProgress", Convert.ToInt32(transmittalPDF24Settings.AutoSaveShowProgress), RegistryValueKind.DWord);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "AutoSaveUseFileCmd", Convert.ToInt32(transmittalPDF24Settings.AutoSaveUseFileCmd), RegistryValueKind.DWord);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "FilenameErasements", transmittalPDF24Settings.FilenameErasements);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "Handler", transmittalPDF24Settings.Handler);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "LoadInCreatorIfOpen", Convert.ToInt32(transmittalPDF24Settings.LoadInCreatorIfOpen), RegistryValueKind.DWord);
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\PDF24\Services\PDF", "ShellCmd", transmittalPDF24Settings.ShellCmd);
    }

    private static string GetRegistryVal_String(string regKey, string valueName)
    {
        object obj = Registry.GetValue(regKey, valueName, "");
        return obj != null ? obj.ToString() : "False";
    }

    private static bool GetRegistryVal_Bool(string regKey, string valueName)
    {
        int myRegDWordValue = 0;
        RegistryKey key = Registry.CurrentUser.OpenSubKey(regKey);
        if (key != null)
        {
            myRegDWordValue = (int?)key.GetValue(valueName) ?? 0; // replace "MyValue" with the actual value name
            key.Close();
        }
        bool myBool = Convert.ToBoolean(myRegDWordValue);

        return myBool;
    }

    private static void SetDefaultPrinter(string printername)
    {
        var type__1 = Type.GetTypeFromProgID("WScript.Network");
        var instance = Activator.CreateInstance(type__1);
        type__1.InvokeMember("SetDefaultPrinter", BindingFlags.InvokeMethod, null, instance, new object[] { printername });
    }

}

internal class PDF24Settings
{
    public string AutoSaveDir { get; set; }
    public string AutoSaveFileCmd { get; set; }
    public string AutoSaveFilename { get; set; }
    public bool AutoSaveOpenDir { get; set; }
    public bool AutoSaveOverwrite { get; set; }
    public bool AutoSaveOverwriteFile { get; set; }
    public string AutoSaveProfile { get; set; }
    public bool AutoSaveShowProgress { get; set; }
    public bool AutoSaveUseFileCmd { get; set; }
    public string FilenameErasements { get; set; }
    public string Handler { get; set; }
    public bool LoadInCreatorIfOpen { get; set; }
    public string ShellCmd { get; set; }
}

#if REVIT2021 
internal class PDFExportOptions
{
    public bool ViewLinksInBlue { get; set; }
    public bool HideReferencePlane { get; set; } = true;
    public bool HideUnreferencedViewTags { get; set; } = true;
    public bool HideCropBoundaries { get; set; }
    public bool HideScopeBoxes { get; set; } = true;
    public bool ReplaceHalftoneWithThinLines { get; set; }
    public bool MaskCoincidentLines { get; set; }
    public bool AlwaysUseRaster { get; set; }

    public ColorDepthType ColorDepth { get; set; }
    public RasterQualityType RasterQuality { get; set; }
}
#endif

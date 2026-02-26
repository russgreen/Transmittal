using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Extensions;
using Serilog.Context;
using Serilog.Core;
using Syncfusion.XlsIO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Transmittal.Extensions;
using Transmittal.Library.Extensions;
using Transmittal.Library.Messages;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Library.ViewModels;
using Transmittal.Messages;
using Transmittal.Models;
using Transmittal.Requesters;
using Transmittal.Services;
using Transmittal.Views;

namespace Transmittal.ViewModels;

internal partial class TransmittalViewModel : BaseViewModel, IStatusRequester, IRevisionRequester, IPersonRequester
{
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly ISettingsService _settingsService;
    private readonly IExportPDFService _exportPDFService;
    private readonly IExportDWGService _exportDWGService;
    private readonly IExportDWFService _exportDWFService;
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ITransmittalService _transmittalService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILogger<TransmittalViewModel> _logger;

    public string WindowTitle { get; private set; }

    [ObservableProperty]
    private string _displayMessage = string.Empty;

    private Thread _progressWindowThread;

    [ObservableProperty]
    private System.Windows.Visibility _isWindowVisible = System.Windows.Visibility.Visible;
     
    private TransmittalModel _newTransmittal = new();

    ///  DRAWING SHEETS 
    public List<DrawingSheetModel> DrawingSheets { get; private set; }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSheetsSelected))]
    private ObservableCollection<object> _selectedDrawingSheets;

    [ObservableProperty]
    private bool _isSheetsSelected = false;
    
    [ObservableProperty]
    private bool _isSelectedSheetsValid = false;

    [ObservableProperty]
    private bool _abortFlag = false;

    [ObservableProperty]
    private bool _processingsheets = false;

    [ObservableProperty]
    private bool _enablePerSheetExportFormats = false;

    /// EXPORT FORMATS 
    [ObservableProperty]
    private bool _exportPDFAvailable = true;
    [ObservableProperty]
    private bool _exportPDF = true;
    [ObservableProperty]
    private bool _exportDWG = false;
    [ObservableProperty]
    private bool _exportDWF = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExportFormatSelected))]
    private int _exportFormatCount = 1;

    [ObservableProperty]
    private bool _pDF24Available = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PDF24Selected))]
    private bool _revitPDFExporterSelected = true; //default should be to export with the Revit PDF Exporter

    public bool PDF24Selected => !RevitPDFExporterSelected;
    
    public bool IsExportFormatSelected => ExportFormatCount > 0;

    [ObservableProperty]
    private PDFExportOptions _pdfExportOptions = new();
    [ObservableProperty]
    private DWGExportOptions _dwgExportOptions = new();
    [ObservableProperty]
    private DWFExportOptions _dwfExportOptions = new();
    [ObservableProperty] 
    private PrintManager _printManager;
    [ObservableProperty]
    private PrintSetup _printSetup;

    public Array RasterQualities { get; private set; }
    public Array Colors { get; private set; }

    [ObservableProperty]
    private RasterQualityType _pdfRasterQuality;
    [ObservableProperty]
    private RasterQualityType _dwfRasterQuality;
    [ObservableProperty]
    private ColorDepthType _pdfColor;
    [ObservableProperty]
    private ColorDepthType _dwfColor;

    public Array DwfImageQualities { get; private set; }

    [ObservableProperty]
    private DWFImageQuality _dwfImageQuality;

    public List<DWGLayerMappingModel> DwgLayerMappings { get; private set; }
    [ObservableProperty]
    private DWGLayerMappingModel _dwgLayerMapping;

    public Array DwgVersions { get; private set; }
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetDwgVersionCommand))]
    private ACADVersion _dwgVersion;

    /// DISTRIBUTION
    public List<IssueFormatModel> IssueFormats { get; private set; }
    public bool CanRecordTransmittal { get; private set; }

    [ObservableProperty]
    private bool _recordTransmittal = false;
    [ObservableProperty]
    private int _copies = 1;
    [ObservableProperty]
    private IssueFormatModel _issueFormat;
    [ObservableProperty]
    private ObservableCollection<ProjectDirectoryModel> _projectDirectory;
    [ObservableProperty]
    private ObservableCollection<TransmittalDistributionModel> _distribution;
    [ObservableProperty]
    private ObservableCollection<object> _selectedProjectDirectory;
    [ObservableProperty]
    private ObservableCollection<object> _selectedDistribution;
    [ObservableProperty]
    private bool _hasDirectoryEntriesSelected = false;
    [ObservableProperty]
    private bool _hasDistributionEntriesSelected = false;

    public bool CanGenerateCDECopies { get; private set; }
    [ObservableProperty]
    private bool _generateCDECopies = false;

    [ObservableProperty]
    private bool _sendToWeTransfer = false;

    /// SUMMARY PROGRESS
    [ObservableProperty]
    private string _currentStepProgressLabel = "Exporting drawing sheets";

    [ObservableProperty]
    private double _drawingSheetsProcessed = 0;
    [ObservableProperty]
    private string _drawingSheetProgressLabel = string.Empty;

    [ObservableProperty]
    private double _sheetTaskProcessed = 0;
    [ObservableProperty]
    private string _sheetTaskProgressLabel = string.Empty;

    [ObservableProperty]
    private bool _isFinishEnabled = false;
    [ObservableProperty]
    private bool _isBackEnabled = true;

    private List<DocumentModel> _exportedFiles = new();

    public TransmittalViewModel()
    {
        // design time constructor
        _settingsServiceRvt = null;
        _settingsService = null;
        _exportPDFService = null;
        _exportDWGService = null;
        _exportDWFService = null;
        _contactDirectoryService = null;
        _transmittalService = null;
        _logger = null;
    }

    public TransmittalViewModel(ISettingsServiceRvt settingsServiceRvt,
        ISettingsService settingsService,
        IExportPDFService exportPDFService,
        IExportDWGService exportDWGService,
        IExportDWFService exportDWFService,
        IContactDirectoryService contactDirectoryService,
        ITransmittalService transmittalService,
        IMessageBoxService messageBoxService,
        ILogger<TransmittalViewModel> logger)
    {
        _settingsServiceRvt = settingsServiceRvt;
        _settingsService = settingsService;
        _exportPDFService = exportPDFService;
        _exportDWGService = exportDWGService;
        _exportDWFService = exportDWFService;
        _contactDirectoryService = contactDirectoryService;
        _transmittalService = transmittalService;
        _messageBoxService = messageBoxService;
        _logger = logger;

        var informationVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);

        WireUpSheetsPage();

        WireUpExportFormatsPage();

        WireUpDistributionPage();

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        WeakReferenceMessenger.Default.Register<CancelTransmittalMessage>(this, (r, m) =>
        {
            AbortFlag = true;
        });

        WeakReferenceMessenger.Default.Register<LockFileMessage>(this, (r, m) =>
        {
            ProcessLockFileMessage(m.Value);
        });
    }

    private void WireUpSheetsPage()
    {
        SelectedDrawingSheets = new();
        SelectedDrawingSheets.CollectionChanged += SelectedDrawingSheets_CollectionChanged;

        DrawingSheets = GetDrawingSheets();
    }

    private void WireUpExportFormatsPage()
    {
#if REVIT2022_OR_GREATER
        //we use the Revit 2022 API to export PDF so always available
        ExportPDFAvailable = true;
        ExportPDF = true;

        //if PDF24 is installed we can use it
        PDF24Available = IsPrinterInstalled("PDF24");
#else
        //we need to check PDF24 is installed
        ExportPDFAvailable = IsPrinterInstalled("PDF24");
        PDF24Available = ExportPDFAvailable;
        if (!ExportPDFAvailable)
        {
            ExportPDF = false;
            ExportFormatCount = 0;
        }
#endif

        PrintManager = App.RevitDocument.PrintManager;
        PrintSetup = PrintManager.PrintSetup;
        PrintSetup.CurrentPrintSetting.PrintParameters.HideUnreferencedViewTags = true;
        PdfExportOptions.HideUnreferencedViewTags = true;
        PdfExportOptions.AlwaysUseRaster = false;

        RasterQualities = Enum.GetValues(typeof(RasterQualityType));
        Colors = Enum.GetValues(typeof(ColorDepthType));
        DwfImageQualities = Enum.GetValues(typeof(DWFImageQuality));

        PdfRasterQuality = RasterQualityType.Presentation;
        PdfColor = ColorDepthType.Color;
        DwfRasterQuality = RasterQualityType.Presentation;
        DwfColor = ColorDepthType.Color;
        DwfImageQuality = DWFImageQuality.Default;

        DwgExportOptions.MergedViews = true; //force this to merge the views by default
        DwgLayerMappings = _exportDWGService.GetDWGLayerMappings();
        DwgLayerMapping = DwgLayerMappings.FirstOrDefault();

        DwgVersions = Enum.GetValues(typeof(ACADVersion));
        DwgVersion = ACADVersion.Default;        
    }

    private void WireUpDistributionPage()
    {
        CanRecordTransmittal = _settingsService.GlobalSettings.RecordTransmittals;

        CanGenerateCDECopies = _settingsService.GlobalSettings.UseExtranet;
        GenerateCDECopies = CanGenerateCDECopies;

        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        IssueFormat = IssueFormats.FirstOrDefault();

        if (_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            RecordTransmittal = true;

            InitializeProjectDirectory();

            InitializeDistribution();
        }

        ValidateTransmittal();
    }

    private void InitializeProjectDirectory()
    {
        ProjectDirectory = new(_contactDirectoryService.GetProjectDirectory(false)
            .OrderBy(x => x.Company.CompanyName)
            .ThenBy(x => x.Person.FullNameReversed));

        ProjectDirectory.CollectionChanged += ProjectDirectory_CollectionChanged;

        SelectedProjectDirectory = new();
        SelectedProjectDirectory.CollectionChanged += SelectedProjectDirectory_CollectionChanged;
    }

    private void InitializeDistribution()
    {
        Distribution = new();
        Distribution.CollectionChanged += Distribution_CollectionChanged;

        SelectedDistribution = new();
        SelectedDistribution.CollectionChanged += SelectedDistribution_CollectionChanged;
    }

    #region Drawing Sheets

    private List<DrawingSheetModel> GetDrawingSheets()
    {
        var drawingSheets = new List<DrawingSheetModel>();

        // get the sheets in the model
        var sheets = new FilteredElementCollector(App.RevitDocument);
        sheets.OfClass(typeof(ViewSheet));
        if (!sheets.Any())
        {
            return drawingSheets;
        }

#if REVIT2025_OR_GREATER
        // get the sheet collections in the model
        var sheetCollections = new FilteredElementCollector(App.RevitDocument);
        sheetCollections.OfClass(typeof(SheetCollection));

        Dictionary<ElementId, string> sheetDictionary = new Dictionary<ElementId, string>();
        foreach (SheetCollection sheetCollection in sheetCollections)
        {
            sheetDictionary.Add(sheetCollection.Id, sheetCollection.Name);
        }
#endif

        foreach (var sheet in sheets.Cast<ViewSheet>())
        {
            // Create a new drawing sheet model to add to the list
            DrawingSheetModel drawingSheet = new()
            {
                ID = sheet.Id,
                DrgNumber = sheet.SheetNumber,
                DrgName = sheet.Name,
                DrgRev = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString(),
                DrgScale = sheet.get_Parameter(BuiltInParameter.SHEET_SCALE).AsString()
            };

            drawingSheet.DrgVolume = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetVolumeParamGuid);
            drawingSheet.DrgLevel = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetLevelParamGuid);
            drawingSheet.DrgType = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.DocumentTypeParamGuid);
            drawingSheet.DrgStatus = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetStatusParamGuid);
            drawingSheet.DrgStatusDescription = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid);
            drawingSheet.DrgPackage = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetPackageParamGuid);

#if REVIT2025_OR_GREATER
            drawingSheet.DrgSheetCollection = GetSheetCollectionName(sheetDictionary, sheet.SheetCollectionId);
#endif

            drawingSheet.DrgOriginator = _settingsService.GlobalSettings.Originator;
            drawingSheet.DrgRole = _settingsService.GlobalSettings.Role;
                        
            drawingSheet.DrgProj = _settingsService.GlobalSettings.ProjectIdentifier;

            if(_settingsService.GlobalSettings.ProjectIdentifier == string.Empty ||
                _settingsService.GlobalSettings.ProjectIdentifier == null)
            {
                drawingSheet.DrgProj = _settingsService.GlobalSettings.ProjectNumber;
            }


            if (sheet.IsPlaceholder == false)
            {
                //TODO find a way of getting the paper size from the sheet in a more performant way
                //that does require sheet graphics to be regenerated
                //drawingSheet.DrgPaper = sheet.GetPaperSize();

                drawingSheet.IssueDate = sheet.get_Parameter(BuiltInParameter.SHEET_ISSUE_DATE).AsString();
                drawingSheet.DrgDrawn = sheet.get_Parameter(BuiltInParameter.SHEET_DRAWN_BY).AsString();
                drawingSheet.DrgChecked = sheet.get_Parameter(BuiltInParameter.SHEET_CHECKED_BY).AsString();
                //drawingSheet.RevDate = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DATE).AsString();

                var currentRevision  = App.RevitDocument.GetElement(sheet.GetCurrentRevision()) as Revision;
                drawingSheet.RevDate = currentRevision?.RevisionDate;

                drawingSheet.RevNotes = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DESCRIPTION).AsString();

                drawingSheets.Add(drawingSheet);
            }

        }

        drawingSheets
            .OrderBy(x => x.DrgNumber)
            .ToList();

        return drawingSheets;
    }

    private void SelectedDrawingSheets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ValidateSheets();
    }

    private void ValidateSheets()
    {
        IsSheetsSelected = false;
        IsSelectedSheetsValid = false;

        if (EnablePerSheetExportFormats)
        {
            var sheetsWithExportFormatsSet = DrawingSheets.Where(x => x.ExportPDF == true || x.ExportDWG == true || x.ExportDWF == true);

            if (sheetsWithExportFormatsSet.Count() > 0)
            {
                IsSheetsSelected = true;
                IsSelectedSheetsValid = true;
            }

            if (_settingsService.GlobalSettings.UseISO19650 == true)
            {
                //todo validate the selected sheets meet ISO19650 rules and all parameters have values
                foreach (var item in sheetsWithExportFormatsSet)
                {
                    if ((string.IsNullOrEmpty(item.DrgVolume)) ||
                        (string.IsNullOrEmpty(item.DrgLevel)) ||
                        (string.IsNullOrEmpty(item.DrgType)) ||
                        (string.IsNullOrEmpty(item.DrgRev)) ||
                        (string.IsNullOrEmpty(item.DrgStatus)))
                    {
                        IsSelectedSheetsValid = false;
                        break;
                    }
                }
            }

            return;
        }

       if (SelectedDrawingSheets.Count > 0)
       {
           IsSheetsSelected = true;
           IsSelectedSheetsValid = true;
       }

        if (_settingsService.GlobalSettings.UseISO19650 == true)
        {
            //todo validate the selected sheets meet ISO19650 rules and all parameters have values
            foreach (var item in SelectedDrawingSheets.Cast<TransmittalItemModel>())
            {
                if ((string.IsNullOrEmpty(item.DrgVolume)) ||
                    (string.IsNullOrEmpty(item.DrgLevel)) ||
                    (string.IsNullOrEmpty(item.DrgType)) ||
                    (string.IsNullOrEmpty(item.DrgRev)) ||
                    (string.IsNullOrEmpty(item.DrgStatus)))
                {
                    IsSelectedSheetsValid = false;
                    break;
                }
            }
        }
    }    
    
    public void StatusComplete(DocumentStatusModel model)
    {
        //get the sheets in the model
        var sheets = new FilteredElementCollector(App.RevitDocument)
            .OfClass(typeof(ViewSheet));

        //loop though the selected items in the grid
        foreach (var sheetModel in SelectedDrawingSheets.Cast<DrawingSheetModel>())
        {
            //set the model values
            sheetModel.DrgStatus = model.Code;
            sheetModel.DrgStatusDescription = model.Description;

            //update the sheet status in revit
            foreach (var sheet in sheets.Cast<ViewSheet>())
            {
                if ((sheetModel.DrgNumber ?? "") == (sheet.SheetNumber ?? ""))
                {
                    if (sheet.CanBePrinted)
                    {
                        SetSheetStatus(sheet, model.Code, model.Description);
                    }
                }
            }

        }

        ValidateSheets();
    }

    private void SetSheetStatus(ViewSheet sheet, string status, string description)
    {
        Transaction trans = null;
        var paramList = new List<Parameter>();
        var paramSet = sheet.Parameters;
        var enumerator = paramSet.GetEnumerator();
        enumerator.Reset();

        while (enumerator.MoveNext())
        {
            paramList.Add((Parameter)enumerator.Current);
        }

        foreach (Parameter param in paramList)
        {
            if (param.IsShared == true)
            {
                if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusParamGuid)
                {
                    {
                        try
                        {
                            trans = new Transaction(App.RevitDocument, "Set Sheet Status");
                            var failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningSwallower());
                            trans.SetFailureHandlingOptions(failOpt);
                            trans.Start();

                            param.Set(status);
                            
                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.RollBack();
                        }
                    }
                }

                if (param.IsShared == true)
                {
                    if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)
                    {
                        try
                        {
                            trans = new Transaction(App.RevitDocument, "Set Sheet Status Description");
                            var failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningSwallower());
                            trans.SetFailureHandlingOptions(failOpt);
                            trans.Start();

                            param.Set(description);

                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.RollBack();
                        }
                    }
                }
            }
        }
    }

    public void RevisionComplete(RevisionDataModel model)
    {
        //get the sheets in the model
        var sheets = new FilteredElementCollector(App.RevitDocument)
            .OfClass(typeof(ViewSheet));

        //loop throught the selected items in the grid
        foreach (var sheetModel in SelectedDrawingSheets.Cast<DrawingSheetModel>())
        {
            foreach (var sheet in sheets.Cast<ViewSheet>())
            {
                if ((sheetModel.DrgNumber ?? "") == (sheet.SheetNumber ?? ""))
                {
                    if (sheet.CanBePrinted)
                    {
                        AddAdditionalRevisionsToSheet(sheet, model.Sequence.ToString());

                        sheetModel.DrgRev = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
                        sheetModel.RevNotes = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DESCRIPTION).AsString();
                    }
                }
            }
        }

        ValidateSheets();
    }

    public void UpdateSheet(DrawingSheetModel sheetModel)
    {
        _logger.LogDebug("UpdateSelectedSheet {sheetModel}", sheetModel);

        Transaction trans = new(App.RevitDocument, "Update sheet parameter values");
        var failOpt = trans.GetFailureHandlingOptions();
        failOpt.SetFailuresPreprocessor(new WarningSwallower());
        trans.SetFailureHandlingOptions(failOpt);
        trans.Start();

        var sheets = new FilteredElementCollector(App.RevitDocument)
            .OfClass(typeof(ViewSheet));

        foreach (var sheet in sheets.Cast<ViewSheet>())
        {
            if ((sheetModel.DrgNumber ?? "") == (sheet.SheetNumber ?? ""))
            {
                try
                {
                    sheet.FindParameter(BuiltInParameter.SHEET_NAME).Set(sheetModel.DrgName);
                    sheet.FindParameter(BuiltInParameter.SHEET_ISSUE_DATE).Set(sheetModel.IssueDate);
                    sheet.FindParameter(BuiltInParameter.SHEET_DRAWN_BY).Set(sheetModel.DrgDrawn);
                    sheet.FindParameter(BuiltInParameter.SHEET_CHECKED_BY).Set(sheetModel.DrgChecked);

                    var paramGuids = new Dictionary<string, string>
                    {
                        { _settingsService.GlobalSettings.SheetVolumeParamGuid, sheetModel.DrgVolume },
                        { _settingsService.GlobalSettings.SheetLevelParamGuid, sheetModel.DrgLevel },
                        { _settingsService.GlobalSettings.DocumentTypeParamGuid, sheetModel.DrgType },
                        { _settingsService.GlobalSettings.SheetPackageParamGuid, sheetModel.DrgPackage }
                    };

                    foreach (var paramGuid in paramGuids)
                    {
                        var param = sheet.FindParameter(new Guid(paramGuid.Key));
                        if (param is not null)
                        {
                            param.Set(paramGuid.Value);
                        }
                    }
                }
                catch
                {
                    trans.RollBack();
                    return;
                }

                break;
            }
        }

        trans.Commit();
    }

    private void AddAdditionalRevisionsToSheet(ViewSheet sheet, string revSeq)
    {
        Transaction trans = new(App.RevitDocument, "Add Revision to Sheet");
        var failOpt = trans.GetFailureHandlingOptions();
        failOpt.SetFailuresPreprocessor(new WarningSwallower());
        trans.SetFailureHandlingOptions(failOpt);
        trans.Start();

        var doc = sheet.Document;
        var revisions = sheet.GetAdditionalRevisionIds();

        // Find revisions whose description matches input string
        var collector = new FilteredElementCollector(doc);
        collector.OfCategory(BuiltInCategory.OST_Revisions);
        collector.WhereElementIsNotElementType();
        if (revisions.Count > 0)
        {
            collector.Excluding(revisions);
        }

        foreach (var revision in collector.Cast<Revision>())
        {
            if ((revision.SequenceNumber.ToString() ?? "") == (revSeq ?? ""))
            {
                revisions.Add(revision.Id);
            }
        }

        if (revisions.Count > 0)
        {
            // Apply the new list of revisions
            sheet.SetAdditionalRevisionIds(revisions);
        }

        trans.Commit();
    }

#if REVIT2025_OR_GREATER
    private string GetSheetCollectionName(Dictionary<ElementId, string> sheetDictionary, ElementId sheetCollectionId)
    {
        if (sheetDictionary.TryGetValue(sheetCollectionId, out string sheetCollectionName))
        {
            return sheetCollectionName;
        }
        else
        {
            return string.Empty; // or handle the case where the ID is not found
        }
    }
#endif

    private void ToggleSheetExport(
        Func<DrawingSheetModel, bool?> getFlag,
        Action<DrawingSheetModel, bool?> setFlag,
        Action<bool> setAggregate)
    {
        if (SelectedDrawingSheets.Count == 0)
        {
            return;
        }

        foreach (var item in SelectedDrawingSheets.Cast<DrawingSheetModel>())
        {
            // true -> false, false/null -> true
            setFlag(item, getFlag(item) != true);
        }

        // Aggregate is true if any selected sheet explicitly has true
        setAggregate(DrawingSheets.Any(x => getFlag(x) == true));

        ValidateSheets();
    }

    [RelayCommand]
    private void TogglePDF() =>
        ToggleSheetExport(x => x.ExportPDF, (x, v) => x.ExportPDF = v, v => ExportPDF = v);

    [RelayCommand]
    private void ToggleDWG() =>
        ToggleSheetExport(x => x.ExportDWG, (x, v) => x.ExportDWG = v, v => ExportDWG = v);

    [RelayCommand]
    private void ToggleDWF() =>
        ToggleSheetExport(x => x.ExportDWF, (x, v) => x.ExportDWF = v, v => ExportDWF = v);

    partial void OnEnablePerSheetExportFormatsChanged(bool value)
    {
        ValidateSheets();

        if(value == false)
        {
            ExportPDF = true;
        }
    }

    #endregion

    #region Export Formats
    [RelayCommand]
    private void GetFormatCount()
    {
        bool[] formats = { ExportPDF, ExportDWG, ExportDWF };

        ExportFormatCount = formats.Sum(x => x ? 1 : 0);
    }

    [RelayCommand]
    private void SetDwgVersion()
    {
        DwgExportOptions.FileVersion = (ACADVersion)DwgVersion;
    }

    #endregion

    #region Distribution
    
    public void PersonComplete(PersonModel model)
    {
        _contactDirectoryService.CreatePerson(model);

        ProjectDirectoryModel projectDirectoryModel = new()
        {
            Person = model,
            Company = _contactDirectoryService.GetCompany(model.CompanyID)
        };

        ProjectDirectory.Add(projectDirectoryModel);
    }
    
    private void ValidateTransmittal()
    {
        IsFinishEnabled = true;

        if (RecordTransmittal == true && (Distribution is null || Distribution.Count == 0))
        {
            IsFinishEnabled = false;
        }
    }

    partial void OnCopiesChanging(int value)
    {
        if (value < 1)
        {
            Copies = 1;
        }
    }

    partial void OnRecordTransmittalChanged(bool value)
    {
        ValidateTransmittal();
    }

    private void ProjectDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        
    }

    private void Distribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ValidateTransmittal();
    }

    private void SelectedProjectDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDirectoryEntriesSelected = true;

        if (SelectedProjectDirectory == null || SelectedProjectDirectory.Count == 0)
        {
            HasDirectoryEntriesSelected = false;
        }
    }

    private void SelectedDistribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDistributionEntriesSelected = true;

        if (SelectedDistribution == null || SelectedDistribution.Count == 0)
        {
            HasDistributionEntriesSelected = false;
        }
    }

    [RelayCommand]
    private void AddToDistribition()
    {
        foreach (ProjectDirectoryModel directoryContact in SelectedProjectDirectory.Cast<ProjectDirectoryModel>().ToList())
        {
            if (directoryContact != null)
            {
                TransmittalDistributionModel distributionRecord = new()
                {
                    
                    Company = directoryContact.Company,
                    Person = directoryContact.Person,
                    PersonID = directoryContact.Person.ID,
                    TransCopies = Copies,
                    TransFormat = IssueFormat.Code
                };

                ProjectDirectory.Remove(directoryContact);
                Distribution.Add(distributionRecord);
            }
        }
    }

    [RelayCommand]
    private void RemoveFromDistribution()
    {
        foreach (TransmittalDistributionModel distributionRecord in SelectedDistribution.Cast<TransmittalDistributionModel>().ToList())
        {
            if (distributionRecord != null)
            {
                ProjectDirectoryModel directoryContact = new()
                {
                    Company = distributionRecord.Company,
                    Person = distributionRecord.Person
                };

                Distribution.Remove(distributionRecord);
                ProjectDirectory.Add(directoryContact);
            }
        }
    }


    #endregion

    [RelayCommand]
    private void ProcessSheets()
    {
        ProcessSheetsInternal();
    }

    private void ProcessSelectedSheets()
    {
        IsBackEnabled = false;
        IsFinishEnabled = false;

        IsWindowVisible = System.Windows.Visibility.Hidden;

        OpenProgressWindow();

        DispatcherHelper.DoEvents();

        try
        {
            var sheets = new FilteredElementCollector(App.RevitDocument);
            sheets.OfClass(typeof(ViewSheet));

            foreach (var drawingSheet in SelectedDrawingSheets.Cast<DrawingSheetModel>())
            {
                foreach (var sheet in sheets.Cast<ViewSheet>())
                {
                    // abort if cancel was clicked
                    if (AbortFlag == true)
                    {
                        CloseProgress();
                        this.OnClosingRequest();
                        return;
                    }

                    if (drawingSheet.DrgNumber == sheet.SheetNumber)
                    {
                        //TODO - test if this check is required.....left in for now...
                        if (sheet.CanBePrinted == true)
                        {
                            var views = new ViewSet();
                            views.Insert(sheet);

                            string fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(
                                 _settingsService.GlobalSettings.ProjectNumber,
                                 _settingsService.GlobalSettings.ProjectIdentifier,
                                 _settingsService.GlobalSettings.ProjectName,
                                 drawingSheet.DrgOriginator,
                                 drawingSheet.DrgVolume,
                                 drawingSheet.DrgLevel,
                                 drawingSheet.DrgType,
                                 drawingSheet.DrgRole,
                                 sheet.SheetNumber,
                                 drawingSheet.DrgName,
                                 drawingSheet.DrgRev,
                                 drawingSheet.DrgStatus,
                                 drawingSheet.DrgStatusDescription);

                            DrawingSheetProgressLabel = $"Processing sheet : {fileName}";
                            SheetTaskProcessed = 0;
                            SheetTaskProgressLabel = string.Empty;
                            SendProgressMessage();

                            if (ExportPDF == true)
                            {
                                ExportSheetToPDF(drawingSheet, views, fileName);
                            }

                            if (AbortFlag == true)
                            {
                                CloseProgress();
                                this.OnClosingRequest();
                                return;
                            }

                            if (ExportDWG == true)
                            {
                                ExportSheetToDWG(drawingSheet, views, fileName);
                            }

                            if (AbortFlag == true)
                            {
                                CloseProgress();
                                this.OnClosingRequest();
                                return;
                            }

                            if (ExportDWF == true)
                            {
                                ExportSheetToDWF(drawingSheet, sheet, views, fileName);
                            }

                            if (AbortFlag == true)
                            {
                                CloseProgress();
                                this.OnClosingRequest();
                                return;
                            }

                            if (RecordTransmittal == true)
                            {
                                // Mark sheets issued date.
                                // Currently disabled is not required for transmittal
                                //SetIssueDate(sheet);

                                // Mark revisions issued
                                SetRevisionsIssued(sheet);
                            }

                            DrawingSheetsProcessed += 1;
                            SendProgressMessage();
                        }
                    }
                }
            }

            CurrentStepProgressLabel = "Recording transmittal...";
            SendProgressMessage();

            if (SendToWeTransfer == true)
            {
                SendFilesToWeTransfer();
            }

            if (RecordTransmittal == true)
            {
                RecordTransmittalInDatabase();

                CurrentStepProgressLabel = "Displaying report...";
                SendProgressMessage();

                LaunchTransmittalReport();

                CopyDistributionToClipboard();
            }

            if (GenerateCDECopies == true)
            {
                CopyFilesForCDE();

                if (!_settingsService.GlobalSettings.UseDrawingIssueStore2)
                {
                    GenerateCDEImportFile();
                }
            }

            OpenExplorerToExportedFilesLocations();

            //just pause before closing the window
            Thread.Sleep(5000);

            CloseProgress();

            this.OnClosingRequest();
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sheets");


            Autodesk.Revit.UI.TaskDialog.Show("Error",
                $"There has been an error processing sheet exports. {Environment.NewLine} {ex}",
                Autodesk.Revit.UI.TaskDialogCommonButtons.Ok);

            CloseProgress();

            this.OnClosingRequest();
            return;
        }
    }

    private void ProcessSheetsInternal()
    {
        IsBackEnabled = false;
        IsFinishEnabled = false;
        IsWindowVisible = System.Windows.Visibility.Hidden;

        OpenProgressWindow();
        DispatcherHelper.DoEvents();

        try
        {
            var sheets = new FilteredElementCollector(App.RevitDocument)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .ToList();

            var targetSheets = EnablePerSheetExportFormats
                ? DrawingSheets.Where(x => x.ExportPDF == true || x.ExportDWG == true || x.ExportDWF == true).ToList()
                : SelectedDrawingSheets.Cast<DrawingSheetModel>().ToList();

            foreach (var drawingSheet in targetSheets)
            {
                if (AbortFlag) { CloseProgress(); OnClosingRequest(); return; }

                var sheet = sheets.FirstOrDefault(x => x.SheetNumber == drawingSheet.DrgNumber);
                if (sheet is null || !sheet.CanBePrinted) continue;

                var views = new ViewSet();
                views.Insert(sheet);

                string fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(
                    _settingsService.GlobalSettings.ProjectNumber,
                    _settingsService.GlobalSettings.ProjectIdentifier,
                    _settingsService.GlobalSettings.ProjectName,
                    drawingSheet.DrgOriginator,
                    drawingSheet.DrgVolume,
                    drawingSheet.DrgLevel,
                    drawingSheet.DrgType,
                    drawingSheet.DrgRole,
                    sheet.SheetNumber,
                    drawingSheet.DrgName,
                    drawingSheet.DrgRev,
                    drawingSheet.DrgStatus,
                    drawingSheet.DrgStatusDescription);

                DrawingSheetProgressLabel = $"Processing sheet : {fileName}";
                SheetTaskProcessed = 0;
                SheetTaskProgressLabel = string.Empty;
                SendProgressMessage();

                bool exportPdf = EnablePerSheetExportFormats ? drawingSheet.ExportPDF == true : ExportPDF;
                bool exportDwg = EnablePerSheetExportFormats ? drawingSheet.ExportDWG == true : ExportDWG;
                bool exportDwf = EnablePerSheetExportFormats ? drawingSheet.ExportDWF == true : ExportDWF;

                if (exportPdf)
                {
                    ExportSheetToPDF(drawingSheet, views, fileName);
                }

                if (AbortFlag) 
                { 
                    CloseProgress(); 
                    OnClosingRequest(); 
                    return; 
                }

                if (exportDwg)
                {
                    ExportSheetToDWG(drawingSheet, views, fileName);
                }

                if (AbortFlag) 
                { 
                    CloseProgress(); 
                    OnClosingRequest(); 
                    return; 
                }

                if (exportDwf)
                {
                    ExportSheetToDWF(drawingSheet, sheet, views, fileName);
                }

                if (AbortFlag) 
                { 
                    CloseProgress(); 
                    OnClosingRequest(); 
                    return; 
                }

                if (RecordTransmittal)
                {
                    SetRevisionsIssued(sheet);
                }

                DrawingSheetsProcessed += 1;
                SendProgressMessage();
            }

            CurrentStepProgressLabel = "Recording transmittal...";
            SendProgressMessage();

            if (SendToWeTransfer) SendFilesToWeTransfer();

            if (RecordTransmittal)
            {
                RecordTransmittalInDatabase();
                CurrentStepProgressLabel = "Displaying report...";
                SendProgressMessage();
                LaunchTransmittalReport();
                CopyDistributionToClipboard();
            }

            if (GenerateCDECopies)
            {
                CopyFilesForCDE();
                if (!_settingsService.GlobalSettings.UseDrawingIssueStore2)
                {
                    GenerateCDEImportFile();
                }
            }

            OpenExplorerToExportedFilesLocations();
            Thread.Sleep(5000);
            CloseProgress();
            OnClosingRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sheets");
            Autodesk.Revit.UI.TaskDialog.Show(
                "Error",
                $"There has been an error processing sheet exports. {Environment.NewLine} {ex}",
                Autodesk.Revit.UI.TaskDialogCommonButtons.Ok);

            CloseProgress();
            OnClosingRequest();
        }
    }

    private void ExportSheetToPDF(DrawingSheetModel drawingSheet, ViewSet views, string fileName)
    {
        SheetTaskProgressLabel = "Exporting PDF...";
        SendProgressMessage();

        var filePath = string.Empty;

#if REVIT2025_OR_GREATER
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.PDF.ToString(), drawingSheet.DrgPackage, drawingSheet.DrgSheetCollection);
#else
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.PDF.ToString(), drawingSheet.DrgPackage);
#endif

#if REVIT2021
        PdfExportOptions.RasterQuality = PdfRasterQuality;
        PdfExportOptions.ColorDepth = PdfColor;

        filePath = _exportPDFService.PrintPDF(fileName,
            folderPath,
            App.RevitDocument,
            views,
            PdfExportOptions);
#else
        if (RevitPDFExporterSelected == true)
        {
            filePath = _exportPDFService.ExportPDF(fileName,
                folderPath,
                App.RevitDocument,
                views,
                PdfExportOptions);
        }
        else
        {
            filePath = _exportPDFService.PrintPDF(fileName,
                folderPath,
                App.RevitDocument,
                views,
                PdfExportOptions);
        }

#endif

        DocumentModel pdf = new DocumentModel(drawingSheet);
        pdf.FilePath = filePath;
        _exportedFiles.Add(pdf);

        //TODO - actually check if the export worked OK
        SheetTaskProgressLabel = "Exporting PDF...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage();
    }

    private void ExportSheetToDWG(DrawingSheetModel drawingSheet, ViewSet views, string fileName)
    {
        SheetTaskProgressLabel = "Exporting DWG...";
        SendProgressMessage();

        DwgExportOptions.FileVersion = (ACADVersion)DwgVersion;
        DwgExportOptions.LayerMapping = DwgLayerMapping.Name;

#if REVIT2025_OR_GREATER
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWG.ToString(), drawingSheet.DrgPackage, drawingSheet.DrgSheetCollection);
#else
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWG.ToString(), drawingSheet.DrgPackage);
#endif

        var filePath = _exportDWGService.ExportDWG($"{fileName}.dwg",
            folderPath,
            DwgExportOptions,
            views,
            App.RevitDocument);

        DocumentModel dwg = new DocumentModel(drawingSheet);
        dwg.FilePath = filePath;
        _exportedFiles.Add(dwg);

        //TODO - actually check if the export worked OK
        SheetTaskProgressLabel = "Exporting DWG...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage();
    }

    private void ExportSheetToDWF(DrawingSheetModel drawingSheet, ViewSheet sheet, ViewSet views, string fileName)
    {
        SheetTaskProgressLabel = "Exporting DWF...";
        SendProgressMessage();

        var sheetsize = sheet.GetExportPaperFormat();

#if REVIT2025_OR_GREATER
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWF.ToString(), drawingSheet.DrgPackage, drawingSheet.DrgSheetCollection);
#else
        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWF.ToString(), drawingSheet.DrgPackage);
#endif

        var filePath = _exportDWFService.ExportDWF($"{fileName}.dwf",
            folderPath,
            sheetsize,
            PrintSetup,
            DwfExportOptions,
            App.RevitDocument,
            views);

        DocumentModel dwf = new DocumentModel(drawingSheet);
        dwf.FilePath = filePath;
        _exportedFiles.Add(dwf);

        //TODO - actually check if the export worked OK
        SheetTaskProgressLabel = "Exporting DWF...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage();
    }

    private void CopyFilesForCDE()
    {
        string folderPath = GetCDEFolderPath();

        _logger.LogInformation("Copying {count} exported files to {folderPath}", _exportedFiles.Count, folderPath);

        foreach (var document in _exportedFiles)
        {
            var fileInfo = new System.IO.FileInfo(document.FilePath);

            string fileName = _settingsService.GlobalSettings.FileNameFilter2.ParseFilename(
                                _settingsService.GlobalSettings.ProjectNumber,
                                _settingsService.GlobalSettings.ProjectIdentifier,
                                _settingsService.GlobalSettings.ProjectName,
                                document.DrgOriginator,
                                document.DrgVolume,
                                document.DrgLevel,
                                document.DrgType,
                                document.DrgRole,
                                document.DrgNumber,
                                document.DrgName,
                                document.DrgRev,
                                document.DrgStatus,
                                document.DrgStatusDescription);

            document.FileName = $"{fileName}{fileInfo.Extension}";

            if (_settingsService.GlobalSettings.UseDrawingIssueStore2)
            {
                var format = "PDF";

                switch (fileInfo.Extension.ToLower()) 
                {
                    case ".dwg" :
                        format = "DWG";
                        break;

                    case ".dwf":
                        format = "DWF";
                        break;
                }

                folderPath = _settingsService.GlobalSettings.DrawingIssueStore2;

#if REVIT2025_OR_GREATER
                folderPath = folderPath.ParseFolderName(format, document.DrgPackage, document.DrgSheetCollection);
#else
                folderPath = folderPath.ParseFolderName(format, document.DrgPackage);
#endif

                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }
            }            

            var fullPath = System.IO.Path.Combine(folderPath, fileName + fileInfo.Extension);

            _logger.LogInformation("Exported file to copy : {file}", document.FileName);

            fileInfo.CopyTo(fullPath, true);

        }

        Process.Start("explorer.exe", $"/root, {folderPath}");

    }

    private void GenerateCDEImportFile()
    {
        string folderPath = GetCDEFolderPath();
        string excelFilePath = Path.Combine(folderPath, "ImportData.xlsx");
        bool columnHeaders = true;

        using (ExcelEngine excelEngine = new())
        {
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;

            IWorkbook workbook = application.Workbooks.Create(1);

            FileInfo fileInfo = new FileInfo(excelFilePath);
            if (fileInfo.Exists && fileInfo.IsReadOnly == false)
            {
                workbook = application.Workbooks.Open(excelFilePath);
                columnHeaders = false;
            }

            IWorksheet worksheet = workbook.Worksheets[0];

            ExcelImportDataOptions importDataOptions = new()
            {
                FirstRow = worksheet.UsedRange.LastRow + 1,
                FirstColumn = 1,
                IncludeHeader = columnHeaders,
                PreserveTypes = false
            };

            worksheet.ImportData(_exportedFiles, importDataOptions);
            worksheet.UsedRange.AutofitColumns();

            try
            {
                workbook.SaveAs(excelFilePath);
            }
            catch
            {
                workbook.SaveAs(Path.Combine(folderPath, $"ImportData_{DateTime.Now.TimeOfDay.Hours}{DateTime.Now.TimeOfDay.Minutes}{DateTime.Now.TimeOfDay.Seconds}.xlsx"));
            }

        }
    }

    private string GetCDEFolderPath()
    {
        var extranetFolderName = "CDE";

        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(extranetFolderName);

        if (!_settingsService.GlobalSettings.DrawingIssueStore.Contains("<Format>"))
        {
            folderPath = System.IO.Path.Combine(folderPath, extranetFolderName);
        }

        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        return folderPath;
    }

    private void SendFilesToWeTransfer()
    {
        _logger.LogInformation("Adding {count} exported files to WeTransfer for upload", _exportedFiles.Count);

        if (_exportedFiles.Count == 0)
        {
            return;
        }

        var filesForWeTransfer = _exportedFiles
            .Where(x => x.FilePath != null)
            .Select(x => x.FilePath)
            .ToList();

        var filesForWeTransferString = string.Join(";", filesForWeTransfer);


#if DEBUG
        var currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPath, @"..\..\..\"));

        var pathToExe = System.IO.Path.Combine(newPath, @$"Transmittal.Desktop\bin\Debug", "Transmittal.Desktop.exe");
#else
        var pathToExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Transmittal", "Transmittal.Desktop.exe");
#endif

        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = pathToExe,
            Arguments = $"\"--wetransfer={filesForWeTransferString}\""
        };

        Process.Start(processStartInfo);
    }

    private void OpenExplorerToExportedFilesLocations()
    {
        var paths = GetUniqueExportedFolderPaths();
        foreach (var path in paths)
        {
            Process.Start("explorer.exe", $"/root, \"{path}\"");
        }

    }

    private void SetRevisionsIssued(ViewSheet sheet)
    {
        try
        {
            ICollection<ElementId> revisions = sheet.GetAllRevisionIds();

            // Find revisions whose description matches input string
            var collector = new FilteredElementCollector(App.RevitDocument, revisions)
                    .OfCategory(BuiltInCategory.OST_Revisions)
                    .WhereElementIsNotElementType();

            if (revisions.Count > 0)
            {
                foreach (var revision in collector.Cast<Revision>())
                {
                    if (revision.Issued == false)
                    {
                        Transaction trans = null;
                        try
                        {
                            trans = new Transaction(App.RevitDocument, "Set Revision Issued");
                            var failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningSwallower());
                            trans.SetFailureHandlingOptions(failOpt);
                            trans.Start();

                            revision.Issued = true;

                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.RollBack();
                        }
                    }
                }
            }
        }
        catch
        {
            // TODO improve error handling
        }
    }

    private void SetIssueDate(ViewSheet sheet)
    {
        string issueDate = DateTime.Now.ToString(_settingsService.GlobalSettings.DateFormatString);

        Transaction trans = null;
        try
        {
            trans = new Transaction(App.RevitDocument, "Set Issue Date");
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);
            trans.Start();
            sheet.get_Parameter(BuiltInParameter.SHEET_ISSUE_DATE).Set(issueDate);
            trans.Commit();
        }
        catch
        {
            trans.RollBack();
        }
    }

    private void RecordTransmittalInDatabase()
    {
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();

        if (RecordTransmittal == false)
        {
            return;
        }

        _newTransmittal.TransDate = DateTime.Now;
        _transmittalService.CreateTransmittal(_newTransmittal);

        foreach (var item in SelectedDrawingSheets.Cast<TransmittalItemModel>())
        {
            item.TransID = _newTransmittal.ID;

            //check if we're using the project identifier on this project
            if (_settingsService.GlobalSettings.ProjectIdentifier is null || _settingsService.GlobalSettings.ProjectIdentifier == string.Empty)
            {
                item.DrgProj = _settingsService.GlobalSettings.ProjectNumber;
            }
            else
            {
                item.DrgProj = _settingsService.GlobalSettings.ProjectIdentifier;
            }

            item.DrgOriginator = _settingsService.GlobalSettings.Originator;
            item.DrgRole = _settingsService.GlobalSettings.Role;
            //_transmittalService.CreateTransmittalItem(item);
        }

        _transmittalService.CreateTransmittalItems(SelectedDrawingSheets.Cast<TransmittalItemModel>().ToList());

        foreach (var dist in Distribution)
        {
            dist.TransID = _newTransmittal.ID;
            //_transmittalService.CreateTransmittalDist(dist);
        }

        _transmittalService.CreateTransmittalDistributions(Distribution.ToList());

        stopWatch.Stop();
        _logger.LogDebug("Record in database in {time}ms", stopWatch.ElapsedMilliseconds);
    }

    private void LaunchTransmittalReport()
    {
        if (RecordTransmittal == false)
        {
            return;
        }

        //get the database file from the current model
        var dbFile = _settingsService.GlobalSettings.DatabaseFile;

        //launch the desktop UI
#if DEBUG
        var currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPath, @"..\..\..\"));

        var pathToExe = System.IO.Path.Combine(newPath, @$"Transmittal.Desktop\bin\Debug", "Transmittal.Desktop.exe");
#else
        var pathToExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Transmittal", "Transmittal.Desktop.exe");
#endif

        if(File.Exists(pathToExe))
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = pathToExe;
            processStartInfo.Arguments = $"--transmittal={_newTransmittal.ID} \"--database={dbFile}\"";

            Process.Start(processStartInfo);   
        }
     
    }

    private void CopyDistributionToClipboard()
    {
        if (!Distribution.Any())
        {
            return;
        }

        var emailAddresses = new StringBuilder();

        foreach (var distributionModel in Distribution)
        {
            if (distributionModel.Person.Email.IsValidEmailAddress())
            {
                emailAddresses.Append(distributionModel.Person.Email);
                emailAddresses.Append("; ");
            }
        }

        if (emailAddresses.Length > 0)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    System.Windows.Clipboard.SetText(emailAddresses.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount > 3)
                    {
                        _logger.LogError(ex, "Error copying email addresses to clipboard");
                        _logger.LogError("Email addresses : {emailAddresses}", emailAddresses.ToString());
                        break;
                    }
                    Thread.Sleep(100);
                }
            }
        }





    }

    private void OpenProgressWindow()
    {
        // Create a thread
        _progressWindowThread = new Thread(new ThreadStart(() =>
        {
            // Create and show the Window
            ProgressView progressWindow = new();

            progressWindow.Closed += (s, e) => 
            Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

            progressWindow.Show();

            // Start the Dispatcher Processing
            Dispatcher.Run();
        }));

        // Set the apartment state
        _progressWindowThread.SetApartmentState(ApartmentState.STA);

        // Make the thread a background thread
        _progressWindowThread.IsBackground = true;

        // Start the thread
        _progressWindowThread.Start();
    }

    private void SendProgressMessage()
    {
        WeakReferenceMessenger.Default.Send(
            new ProgressUpdateMessage(
                new ProgressMessageModel 
                { 
                    CurrentStepProgressLabel = CurrentStepProgressLabel,
                    DrawingSheetsToProcess = SelectedDrawingSheets.Count,
                    DrawingSheetsProcessed = DrawingSheetsProcessed,
                    DrawingSheetProgressLabel = DrawingSheetProgressLabel,
                    SheetTasksToProcess = ExportFormatCount,
                    SheetTaskProcessed = SheetTaskProcessed,
                    SheetTaskProgressLabel = SheetTaskProgressLabel
                }));
    }

    private void CloseProgress()
    {
        Dispatcher.FromThread(_progressWindowThread).InvokeShutdown();
    }

    private void ProcessLockFileMessage(string value)
    {
        if (value == "")
        {
            DisplayMessage = "";
            return;
        }

        //so we have a lock file
        DisplayMessage = $"Waiting for database .lock file to clear. Check if .lock needs to be manually deleted.";

        DispatcherHelper.DoEvents();

    } 

    private bool IsPrinterInstalled(string PrinterName)
    {
        bool retval = false;
        foreach (var ptName in PrinterSettings.InstalledPrinters)
        {
            if ((ptName.ToString() ?? "") == (PrinterName ?? ""))
            {
                var pt = new PrinterSettings
                {
                    PrinterName = ptName.ToString()
                };

                if (pt.IsValid)
                {
                    retval = true;
                    break;
                }
            }
        }

        return retval;
    }

    /// <summary>
    /// Returns a sorted, de-duplicated list of directories that contain the exported files.
    /// Safely skips null / empty / invalid paths.
    /// </summary>
    private IReadOnlyList<string> GetUniqueExportedFolderPaths()
    {
        if (_exportedFiles == null || _exportedFiles.Count == 0)
        {
            return Array.Empty<string>();
        }

        var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var doc in _exportedFiles)
        {
            // Defensive: DocumentModel should have FilePath set after export
            var filePath = doc?.FilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                continue;
            }

            string dir;
            try
            {
                dir = Path.GetDirectoryName(filePath);
            }
            catch
            {
                continue; // skip invalid paths
            }

            if (string.IsNullOrWhiteSpace(dir))
            {
                continue;
            }

            // Normalize to full path to avoid duplicates caused by relative segments
            try
            {
                dir = Path.GetFullPath(dir);
            }
            catch
            {
                // If GetFullPath fails, keep original
            }

            folders.Add(dir);
        }

        return folders.OrderBy(x => x).ToList();
    }

    partial void OnSendToWeTransferChanged(bool oldValue, bool newValue)
    {
        if (newValue == true)
        {
            if (!_messageBoxService.ShowYesNo(
                "WeTransfer Preview Feature",
                "Any running browsers will be closed when using this feature. Would you still like to continue using it?"))
            {
                SendToWeTransfer = oldValue;
            }
        }
    }
}

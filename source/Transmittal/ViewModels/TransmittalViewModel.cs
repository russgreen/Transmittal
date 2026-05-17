using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nice3point.Revit.Extensions;
using Serilog.Context;
using Serilog.Core;
using Syncfusion.XlsIO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using Transmittal.Commands;
using Transmittal.Extensions;
using Transmittal.Library.Enums;
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
    private readonly IExportFileCheckService _exportFileCheckService;
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

    [ObservableProperty]
    private int _sheetsToExport = 0;

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

    [ObservableProperty]
    private ObservableCollection<DWGLayerMappingModel> _dwgLayerMappings;
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
    private Library.Enums.FileTransferType _fileTransferType = Library.Enums.FileTransferType.WeTransfer;

    [ObservableProperty]
    private bool _sendFileTransfer = false;

    [ObservableProperty]
    private bool _showFileTransfer = true;

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
    [ObservableProperty]
    private ObservableCollection<ExportFileCheckResult> _exportFileCheckResults = new();

    private List<DocumentModel> _exportedFiles = new();
    private List<string> _additionalExportFiles = new();
    private readonly Dictionary<string, string> _existingFilesToReuse = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _fileCheckCancellationTokenSource;
    private bool _useExistingExportedFiles;
    private DWGLayerMappingModel _customDwgLayerMapping;
    private int _nextCustomDwgLayerMappingId = 1000;
    private bool _suppressDwgLayerMappingChange;
    private DWGLayerMappingModel _previousDwgLayerMapping;

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
        IExportFileCheckService exportFileCheckService,
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
        _exportFileCheckService = exportFileCheckService;
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
        foreach (var drawingSheet in DrawingSheets)
        {
            drawingSheet.PropertyChanged += DrawingSheet_PropertyChanged;
        }

        using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Transmittal", false))
        {
            if (key != null)
            {
                EnablePerSheetExportFormats = Convert.ToBoolean(key.GetValue("EnablePerSheetExportFormats", EnablePerSheetExportFormats));
                SendFileTransfer = Convert.ToBoolean(key.GetValue("SendFileTransfer", SendFileTransfer));
            }
        }
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

        DwgExportOptions = _exportDWGService.GetDocumentDWGExportOptions(App.RevitDocument);
        InitializeDwgLayerMappings(DwgExportOptions.LayerMapping);

        DwgVersions = Enum.GetValues(typeof(ACADVersion));
        DwgVersion = DwgExportOptions.FileVersion;
    }

    private void InitializeDwgLayerMappings(string layerMapping)
    {
        _nextCustomDwgLayerMappingId = 1000;
        var mappings = _exportDWGService.GetDWGLayerMappings();
        AddDocumentLayerMappings(mappings);
        _customDwgLayerMapping = null;

        var selectedMapping = SelectDwgLayerMappingModel(mappings, layerMapping);
        mappings.Add(CreateLoadLayerMappingAction());

        DwgLayerMappings = new ObservableCollection<DWGLayerMappingModel>(mappings);
        SetDwgLayerMappingSelection(selectedMapping ?? DwgLayerMappings.FirstOrDefault(x => x.IsActionItem == false), false);
    }

    private DWGLayerMappingModel SelectDwgLayerMappingModel(List<DWGLayerMappingModel> mappings, string layerMapping)
    {
        if (string.IsNullOrWhiteSpace(layerMapping))
        {
            return mappings.FirstOrDefault();
        }

        var existingMapping = mappings.FirstOrDefault(x => string.Equals(x.LayerMapping, layerMapping, StringComparison.OrdinalIgnoreCase));
        if (existingMapping != null)
        {
            return existingMapping;
        }

        _customDwgLayerMapping = CreateCustomLayerMapping(layerMapping);
        mappings.Add(_customDwgLayerMapping);
        return _customDwgLayerMapping;
    }

    private void AddDocumentLayerMappings(List<DWGLayerMappingModel> mappings)
    {
        var documentMappings = _exportDWGService.GetDocumentDWGLayerMappings(App.RevitDocument);
        foreach (var documentMapping in documentMappings)
        {
            if (string.IsNullOrWhiteSpace(documentMapping))
            {
                continue;
            }

            var exists = mappings.Any(x =>
                x.IsActionItem == false &&
                string.Equals(x.LayerMapping, documentMapping, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                mappings.Add(CreateCustomLayerMapping(documentMapping));
            }
        }
    }

    private DWGLayerMappingModel CreateCustomLayerMapping(string layerMapping)
    {
        var fileName = Path.GetFileName(layerMapping);

        return new DWGLayerMappingModel()
        {
            Id = _nextCustomDwgLayerMappingId++,
            Name = string.IsNullOrWhiteSpace(fileName) ? layerMapping : fileName,
            LayerMapping = layerMapping,
            IsCustom = true
        };
    }

    private DWGLayerMappingModel CreateLoadLayerMappingAction()
    {
        return new DWGLayerMappingModel()
        {
            Id = 9999,
            Name = "Load settings from file....",
            LayerMapping = string.Empty,
            IsActionItem = true
        };
    }

    private void SetDwgLayerMappingSelection(DWGLayerMappingModel mapping, bool persist)
    {
        if (mapping == null)
        {
            return;
        }

        var mappingToSelect = ResolveCurrentDwgLayerMapping(mapping);
        if (mappingToSelect == null)
        {
            return;
        }

        _suppressDwgLayerMappingChange = true;
        DwgLayerMapping = null;
        DwgLayerMapping = mappingToSelect;
        _suppressDwgLayerMappingChange = false;

        ApplyDwgLayerMapping(mappingToSelect, persist);
    }

    private DWGLayerMappingModel ResolveCurrentDwgLayerMapping(DWGLayerMappingModel mapping)
    {
        if (mapping == null)
        {
            return null;
        }

        if (DwgLayerMappings == null || DwgLayerMappings.Count == 0)
        {
            return mapping;
        }

        var idMatch = DwgLayerMappings.FirstOrDefault(x => x.Id == mapping.Id && x.IsActionItem == mapping.IsActionItem);
        if (idMatch != null)
        {
            return idMatch;
        }

        if (!string.IsNullOrWhiteSpace(mapping.LayerMapping))
        {
            var layerMatch = DwgLayerMappings.FirstOrDefault(x =>
                x.IsActionItem == mapping.IsActionItem &&
                string.Equals(x.LayerMapping, mapping.LayerMapping, StringComparison.OrdinalIgnoreCase));

            if (layerMatch != null)
            {
                return layerMatch;
            }
        }

        return DwgLayerMappings.FirstOrDefault(x => x.IsActionItem == false);
    }

    private void ApplyDwgLayerMapping(DWGLayerMappingModel mapping, bool persist)
    {
        if (mapping == null || mapping.IsActionItem)
        {
            return;
        }

        _previousDwgLayerMapping = mapping;
        DwgExportOptions.LayerMapping = mapping.LayerMapping;

        if (persist)
        {
            _exportDWGService.SaveDocumentDWGExportOptions(App.RevitDocument, DwgExportOptions);
        }
    }

    partial void OnDwgLayerMappingChanged(DWGLayerMappingModel value)
    {
        if (_suppressDwgLayerMappingChange || value == null)
        {
            return;
        }

        if (value.IsActionItem)
        {
            var previousMapping = _previousDwgLayerMapping ?? DwgLayerMappings?.FirstOrDefault(x => x.IsActionItem == false);
            if (previousMapping != null)
            {
                SetDwgLayerMappingSelection(previousMapping, false);
            }

            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                SelectCustomLayerMappingFile(previousMapping);
            }), DispatcherPriority.Background);

            return;
        }

        ApplyDwgLayerMapping(value, true);
    }

    private void SelectCustomLayerMappingFile(DWGLayerMappingModel previousMapping)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Layer Mapping Files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Select layer mapping file"
        };

        if (!string.IsNullOrWhiteSpace(_customDwgLayerMapping?.LayerMapping))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(_customDwgLayerMapping.LayerMapping);
        }
        else if (!string.IsNullOrWhiteSpace(DwgExportOptions?.LayerMapping) && File.Exists(DwgExportOptions.LayerMapping))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(DwgExportOptions.LayerMapping);
        }

        if (dialog.ShowDialog() == true)
        {
            _nextCustomDwgLayerMappingId = 1000;
            var mappings = _exportDWGService.GetDWGLayerMappings();
            AddDocumentLayerMappings(mappings);
            _customDwgLayerMapping = SelectDwgLayerMappingModel(mappings, dialog.FileName);
            mappings.Add(CreateLoadLayerMappingAction());
            DwgLayerMappings = new ObservableCollection<DWGLayerMappingModel>(mappings);

            SetDwgLayerMappingSelection(_customDwgLayerMapping, true);
            return;
        }

        var mappingToRestore = previousMapping ?? DwgLayerMappings?.FirstOrDefault(x => x.IsActionItem == false);
        if (mappingToRestore != null)
        {
            SetDwgLayerMappingSelection(mappingToRestore, false);
        }
    }

    private void WireUpDistributionPage()
    {
        CanRecordTransmittal = _settingsService.GlobalSettings.RecordTransmittals;

        CanGenerateCDECopies = _settingsService.GlobalSettings.UseExtranet;
        GenerateCDECopies = CanGenerateCDECopies;

        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        IssueFormat = IssueFormats.FirstOrDefault();

        FileTransferType = _settingsService.GlobalSettings.FileTransferType;
        ShowFileTransfer = _settingsService.GlobalSettings.ShowFileTransfer;

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
        QueueExistingFileCheck();
    }

    private void DrawingSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not DrawingSheetModel drawingSheet)
        {
            return;
        }

        if (!SelectedDrawingSheets.Contains(drawingSheet))
        {
            return;
        }

        QueueExistingFileCheck();
    }

    private void ValidateSheets()
    {
        IsSheetsSelected = false;
        IsSelectedSheetsValid = false;

        if(SelectedDrawingSheets.Count == 0)
        {
            SheetsToExport = 0;
            return;
        }

        IsSheetsSelected = true;

        if (EnablePerSheetExportFormats)
        {
            var sheetsWithExportFormatsSet = DrawingSheets.Where(x => x.ExportPDF == true || x.ExportDWG == true || x.ExportDWF == true);

            SheetsToExport = sheetsWithExportFormatsSet.Count();

            if (sheetsWithExportFormatsSet.Count() > 0)
            {
                IsSelectedSheetsValid = true;
            }

            if (_settingsService.GlobalSettings.UseISO19650 == true)
            {
                //TODO validate the selected sheets meet ISO19650 rules and all parameters have values
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
        
        IsSelectedSheetsValid = true;
        SheetsToExport = SelectedDrawingSheets.Count;

        if (_settingsService.GlobalSettings.UseISO19650 == true)
        {
            //TODO validate the selected sheets meet ISO19650 rules and all parameters have values
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
                        using (Transaction trans = new Transaction(App.RevitDocument, "Set Sheet Status"))
                        {
                            try
                            {
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
                }

                if (param.IsShared == true)
                {
                    if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)
                    {
                        using (Transaction trans = new Transaction(App.RevitDocument, "Set Sheet Status Description"))
                        {
                            try
                            {
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
                    sheet.FindParameter(BuiltInParameter.SHEET_ISSUE_DATE).Set(sheetModel.IssueDate.ToDateFormat(_settingsService.GlobalSettings.DateFormatString));
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
        QueueExistingFileCheck();
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
        QueueExistingFileCheck();

        if(value == false)
        {
            ExportPDF = true;
        }

        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Transmittal", true);
        if (key == null)
        {
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Transmittal", true);
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Transmittal", true);
        }
        
        key.SetValue("EnablePerSheetExportFormats", EnablePerSheetExportFormats, Microsoft.Win32.RegistryValueKind.String);
        
        key.Close();
    }

    partial void OnExportPDFChanged(bool value)
    {
        QueueExistingFileCheck();
    }

    partial void OnExportDWGChanged(bool value)
    {
        QueueExistingFileCheck();
    }

    partial void OnExportDWFChanged(bool value)
    {
        QueueExistingFileCheck();
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
        var personToUse = model;
        var personMatch = _contactDirectoryService
            .FindPersonMatches(model.FirstName, model.LastName, model.Email, model.CompanyID)
            .FirstOrDefault();

        if (personMatch != null)
        {
            var useExisting = _messageBoxService.ShowYesNo(
                "Similar contact found",
                $"A similar contact already exists:\n\n{personMatch.FullNameReversed}\n\nUse the existing contact instead of creating a new one?");

            if (useExisting)
            {
                personToUse = personMatch;
            }
            else
            {
                _contactDirectoryService.CreatePerson(model);
            }
        }
        else
        {
            _contactDirectoryService.CreatePerson(model);
        }

        if (ProjectDirectory.Any(x => x.Person.ID == personToUse.ID))
        {
            return;
        }

        ProjectDirectoryModel projectDirectoryModel = new()
        {
            Person = personToUse,
            Company = _contactDirectoryService.GetCompany(personToUse.CompanyID)
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

    partial void OnSendFileTransferChanged(bool value)
    {
        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Transmittal", true);
        if (key == null)
        {
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Transmittal", true);
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Transmittal", true);
        }

        key.SetValue("SendFileTransfer", SendFileTransfer, Microsoft.Win32.RegistryValueKind.String);

        key.Close();
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
        using (LogContext.PushProperty("UsageTracking", true))
        {
            _logger.LogInformation("Per sheet formats : {value}", EnablePerSheetExportFormats);
        }

        var targetSheets = GetTargetSheetsForProcessing();

        if (targetSheets.Count == 0)
        {
            return;
        }

        _exportedFiles.Clear();
        _additionalExportFiles.Clear();

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

            foreach (var drawingSheet in targetSheets)
            {
                if (AbortFlag) 
                { 
                    CloseProgress(); 
                    OnClosingRequest(); 
                    return; 
                }

                var totalSheets = targetSheets.Count;

                var sheet = sheets.FirstOrDefault(x => x.CanBePrinted && x.SheetNumber == drawingSheet.DrgNumber);
                if (sheet is null)
                {
                    //the sheet must exist as we already picked it to export. this check is defensive, but included just in case....
                    continue;
                }

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
                SendProgressMessage(totalSheets);

                bool exportPdf = EnablePerSheetExportFormats ? drawingSheet.ExportPDF == true : ExportPDF;
                bool exportDwg = EnablePerSheetExportFormats ? drawingSheet.ExportDWG == true : ExportDWG;
                bool exportDwf = EnablePerSheetExportFormats ? drawingSheet.ExportDWF == true : ExportDWF;

                exportPdf = UseExistingFileIfRequested(drawingSheet, Enums.ExportFormatType.PDF, exportPdf, "PDF", totalSheets);
                exportDwg = UseExistingFileIfRequested(drawingSheet, Enums.ExportFormatType.DWG, exportDwg, "DWG", totalSheets);
                exportDwf = UseExistingFileIfRequested(drawingSheet, Enums.ExportFormatType.DWF, exportDwf, "DWF", totalSheets);

                if (exportPdf)
                {
                    ExportSheetToPDF(drawingSheet, views, fileName, totalSheets);
                }

                if (AbortFlag) 
                { 
                    CloseProgress(); 
                    OnClosingRequest(); 
                    return; 
                }

                if (exportDwg)
                {
                    ExportSheetToDWG(drawingSheet, views, fileName, totalSheets);
                }

                if (AbortFlag) 
                { 
                    CloseProgress(); 
                    OnClosingRequest(); 
                    return; 
                }

                if (exportDwf)
                {
                    ExportSheetToDWF(drawingSheet, sheet, views, fileName, totalSheets);
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
                SendProgressMessage(targetSheets.Count);
            }

            CurrentStepProgressLabel = "Recording transmittal...";
            SendProgressMessage(targetSheets.Count);

            if (SendFileTransfer)
            {
                SendFilesToTransfer();
            }

            if (RecordTransmittal)
            {
                RecordTransmittalInDatabase(targetSheets);
                CurrentStepProgressLabel = "Displaying report...";
                SendProgressMessage(targetSheets.Count);
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

    internal IReadOnlyList<ExportFileCheckResult> GetCurrentFileConflicts()
    {
        var targetSheets = GetTargetSheetsForProcessing();
        if (targetSheets.Count == 0)
        {
            SetFileCheckResults(Array.Empty<ExportFileCheckResult>());
            return Array.Empty<ExportFileCheckResult>();
        }

        var checkResults = _exportFileCheckService
            .CheckExportFilesAsync(targetSheets, EnablePerSheetExportFormats, ExportPDF, ExportDWG, ExportDWF)
            .GetAwaiter()
            .GetResult();

        SetFileCheckResults(checkResults);

        return checkResults.Where(x => x.FileExists).ToList();
    }

    internal bool ApplyFileConflictAction(FileConflictAction action, IReadOnlyCollection<ExportFileCheckResult> conflicts)
    {
        _existingFilesToReuse.Clear();
        _useExistingExportedFiles = false;

        if (action == FileConflictAction.Overwrite)
        {
            return true;
        }

        if (action == FileConflictAction.UseExisting)
        {
            _useExistingExportedFiles = true;
            foreach (var conflict in conflicts)
            {
                _existingFilesToReuse[GetFileReuseKey(conflict.SheetNumber, conflict.ExportFormat)] = conflict.OutputPath;
            }

            return true;
        }

        if (action == FileConflictAction.ReviseSheets && conflicts.Count > 0)
        {
            SelectConflictingSheets(conflicts);
            IsWindowVisible = System.Windows.Visibility.Visible;
            IsBackEnabled = true;
            IsFinishEnabled = true;
            return false;
        }

        return false;
    }

    private bool UseExistingFileIfRequested(DrawingSheetModel drawingSheet, Enums.ExportFormatType format, bool shouldExport, string formatLabel, int totalSheets)
    {
        if (!_useExistingExportedFiles || !shouldExport)
        {
            return shouldExport;
        }

        if (!TryAddExistingExportedFile(drawingSheet, format))
        {
            return shouldExport;
        }

        SheetTaskProgressLabel = $"Using existing {formatLabel}...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage(totalSheets);
        return false;
    }

    private bool TryAddExistingExportedFile(DrawingSheetModel drawingSheet, Enums.ExportFormatType format)
    {
        if (!_existingFilesToReuse.TryGetValue(GetFileReuseKey(drawingSheet.DrgNumber, format), out var existingFilePath))
        {
            return false;
        }

        if (!File.Exists(existingFilePath))
        {
            return false;
        }

        DocumentModel existingFile = new(drawingSheet)
        {
            FilePath = existingFilePath
        };
        _exportedFiles.Add(existingFile);

        if (format == Enums.ExportFormatType.DWG && (DwgExportOptions.SharedCoords || !DwgExportOptions.MergedViews))
        {
            var folderPath = Path.GetDirectoryName(existingFilePath) ?? string.Empty;
            var fileNamePrefix = Path.GetFileNameWithoutExtension(existingFilePath);

            var additionalDwgFiles = GetFilesWithPrefixExceptPrimary(folderPath, fileNamePrefix, ".dwg");
            foreach (var additionalFile in additionalDwgFiles)
            {
                _additionalExportFiles.Add(additionalFile);
            }
        }

        return true;
    }

    private void SelectConflictingSheets(IReadOnlyCollection<ExportFileCheckResult> conflicts)
    {
        var conflictSheetIds = conflicts
            .Select(x => x.SheetId.ToString())
            .ToHashSet();

        SelectedDrawingSheets.CollectionChanged -= SelectedDrawingSheets_CollectionChanged;
        SelectedDrawingSheets.Clear();

        foreach (var drawingSheet in DrawingSheets.Where(x => conflictSheetIds.Contains(x.ID.ToString())))
        {
            SelectedDrawingSheets.Add(drawingSheet);
        }

        SelectedDrawingSheets.CollectionChanged += SelectedDrawingSheets_CollectionChanged;
        ValidateSheets();
        QueueExistingFileCheck();
    }

    private void QueueExistingFileCheck()
    {
        _fileCheckCancellationTokenSource?.Cancel();
        _fileCheckCancellationTokenSource?.Dispose();
        _fileCheckCancellationTokenSource = new CancellationTokenSource();

        _ = CheckForExistingFilesAsync(_fileCheckCancellationTokenSource.Token);
    }

    private async Task CheckForExistingFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var targetSheets = GetTargetSheetsForFileCheck();
            if (targetSheets.Count == 0)
            {
                SetFileCheckResults(Array.Empty<ExportFileCheckResult>());
                return;
            }

            var results = await _exportFileCheckService.CheckExportFilesAsync(
                targetSheets,
                EnablePerSheetExportFormats,
                ExportPDF,
                ExportDWG,
                ExportDWF,
                cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            SetFileCheckResults(results);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private List<DrawingSheetModel> GetTargetSheetsForFileCheck()
    {
        if (SelectedDrawingSheets is null || SelectedDrawingSheets.Count == 0)
        {
            return new List<DrawingSheetModel>();
        }

        return EnablePerSheetExportFormats
            ? SelectedDrawingSheets
                .Cast<DrawingSheetModel>()
                .Where(x => x.ExportPDF == true || x.ExportDWG == true || x.ExportDWF == true)
                .ToList()
            : SelectedDrawingSheets
                .Cast<DrawingSheetModel>()
                .ToList();
    }

    private List<DrawingSheetModel> GetTargetSheetsForProcessing()
    {
        return EnablePerSheetExportFormats
            ? DrawingSheets.Where(x => x.ExportPDF == true || x.ExportDWG == true || x.ExportDWF == true).ToList()
            : SelectedDrawingSheets.Cast<DrawingSheetModel>().ToList();
    }

    private void SetFileCheckResults(IReadOnlyList<ExportFileCheckResult> results)
    {
        if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == true)
        {
            ExportFileCheckResults = new ObservableCollection<ExportFileCheckResult>(results);
            return;
        }

        System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
        {
            ExportFileCheckResults = new ObservableCollection<ExportFileCheckResult>(results);
        });
    }

    private static string GetFileReuseKey(string sheetNumber, Enums.ExportFormatType format)
    {
        return $"{sheetNumber}:{format}";
    }

    private void ExportSheetToPDF(DrawingSheetModel drawingSheet, ViewSet views, string fileName, int totalSheets)
    {
        SheetTaskProgressLabel = "Exporting PDF...";
        SendProgressMessage(totalSheets);

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

        //TODO actually check if the export worked OK
        SheetTaskProgressLabel = "Exporting PDF...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage(totalSheets);
    }

    private void ExportSheetToDWG(DrawingSheetModel drawingSheet, ViewSet views, string fileName, int totalSheets)
    {
        SheetTaskProgressLabel = "Exporting DWG...";
        SendProgressMessage(totalSheets);

        DwgExportOptions.FileVersion = (ACADVersion)DwgVersion;
        DwgExportOptions.LayerMapping = DwgLayerMapping?.LayerMapping ?? DwgExportOptions.LayerMapping;

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

        if (DwgExportOptions.SharedCoords || !DwgExportOptions.MergedViews)
        {
            var additionalDwgFiles = GetFilesWithPrefixExceptPrimary(folderPath, fileName, ".dwg");
            foreach (var additionalFile in additionalDwgFiles)
            {
                _additionalExportFiles.Add(additionalFile);
            }
        }

        //TODO actually check if the export worked OK
        SheetTaskProgressLabel = "Exporting DWG...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage(totalSheets);
    }

    private IReadOnlyList<string> GetFilesWithPrefixExceptPrimary(string folderPath, string fileNamePrefix, string primaryExtension = ".dwg")
    {
        if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(fileNamePrefix))
        {
            return Array.Empty<string>();
        }

        if (!Directory.Exists(folderPath))
        {
            return Array.Empty<string>();
        }

        var primaryFileName = $"{fileNamePrefix}{primaryExtension}";

        return Directory.EnumerateFiles(folderPath, $"{fileNamePrefix}*", SearchOption.TopDirectoryOnly)
            .Where(path => !Path.GetFileName(path).Equals(primaryFileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void ExportSheetToDWF(DrawingSheetModel drawingSheet, ViewSheet sheet, ViewSet views, string fileName, int totalSheets)
    {
        SheetTaskProgressLabel = "Exporting DWF...";
        SendProgressMessage(totalSheets);

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

        //TODO actually check if the export worked OK
        SheetTaskProgressLabel = "Exporting DWF...DONE";
        SheetTaskProcessed += 1;
        SendProgressMessage(totalSheets);
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

    private void SendFilesToTransfer()
    {
        _logger.LogInformation("Adding {count} exported files to file transfer service upload", _exportedFiles.Count);

        if (_exportedFiles.Count == 0)
        {
            return;
        }

        var filesForTransfer = _exportedFiles
            .Where(x => x.FilePath != null)
            .Select(x => x.FilePath)
            .ToList();

        if(_additionalExportFiles.Count > 0)
        {
            filesForTransfer.AddRange(_additionalExportFiles);
        }
        
        var filesForTransferString = string.Join(";", filesForTransfer);


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
            Arguments = $"\"--filetransfer={filesForTransferString}\""
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
                        using (Transaction trans = new Transaction(App.RevitDocument, "Set Revision Issued"))
                        {
                            try
                            {
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
        }
        catch
        {
            // TODO improve error handling
        }
    }

    private void SetIssueDate(ViewSheet sheet)
    {
        string issueDate = DateTime.Now.ToString(_settingsService.GlobalSettings.DateFormatString);

        try
        {
            using (Transaction trans = new Transaction(App.RevitDocument, "Set Issue Date"))
            {
                var failOpt = trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new WarningSwallower());
                trans.SetFailureHandlingOptions(failOpt);
                trans.Start();
                sheet.get_Parameter(BuiltInParameter.SHEET_ISSUE_DATE).Set(issueDate);
                trans.Commit();
            }
        }
        catch
        {
            // TODO improve error handling
        }
    }

    private void RecordTransmittalInDatabase(List<DrawingSheetModel> sheets)
    {
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();

        if (RecordTransmittal == false)
        {
            return;
        }

        _newTransmittal.TransDate = DateTime.Now;
        _transmittalService.CreateTransmittal(_newTransmittal);

        foreach (var sheet in sheets)
        {
            sheet.TransID = _newTransmittal.ID;

            //check if we're using the project identifier on this project
            if (_settingsService.GlobalSettings.ProjectIdentifier is null || _settingsService.GlobalSettings.ProjectIdentifier == string.Empty)
            {
                sheet.DrgProj = _settingsService.GlobalSettings.ProjectNumber;
            }
            else
            {
                sheet.DrgProj = _settingsService.GlobalSettings.ProjectIdentifier;
            }

            sheet.DrgOriginator = _settingsService.GlobalSettings.Originator;
            sheet.DrgRole = _settingsService.GlobalSettings.Role;
            //_transmittalService.CreateTransmittalItem(item);
        }

        _transmittalService.CreateTransmittalItems(sheets.Cast<TransmittalItemModel>().ToList());

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

    private void SendProgressMessage(int sheetsToProcess)
    {
        WeakReferenceMessenger.Default.Send(
            new ProgressUpdateMessage(
                new ProgressMessageModel 
                { 
                    CurrentStepProgressLabel = CurrentStepProgressLabel,
                    DrawingSheetsToProcess = sheetsToProcess,
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

}

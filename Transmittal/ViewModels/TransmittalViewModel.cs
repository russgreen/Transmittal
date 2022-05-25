using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using Transmittal.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Requesters;
using Transmittal.Services;

namespace Transmittal.ViewModels;

[INotifyPropertyChanged]
internal partial class TransmittalViewModel : CloseableViewModel, IStatusRequester, IRevisionRequester
{
    private readonly ISettingsServiceRvt _settingsServiceRvt = Ioc.Default.GetRequiredService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>(); 
    private readonly IExportPDFService _exportPDFService = Ioc.Default.GetRequiredService<IExportPDFService>();
    private readonly IExportDWGService _exportDWGService = Ioc.Default.GetRequiredService<IExportDWGService>();
    private readonly IExportDWFService _exportDWFService = Ioc.Default.GetRequiredService<IExportDWFService>();
    private readonly IContactDirectoryService _contactDirectoryService = Ioc.Default.GetRequiredService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Ioc.Default.GetRequiredService<ITransmittalService>();

    public string WindowTitle { get; private set; }

    private TransmittalModel _newTransmittal = new();    
    
    ///  DRAWING SHEETS
    public List<DrawingSheetModel> DrawingSheets { get; private set; }
    
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsSheetsSelected))]
    private ObservableCollection<DrawingSheetModel> _selectedDrawingSheets;

    [ObservableProperty]
    private bool _isSheetsSelected = false;
    
    [ObservableProperty]
    private bool _isSelectedSheetsValid = false;

    [ObservableProperty]
    private bool _abortFlag = false;
    [ObservableProperty]
    private bool _processingsheets = false;

    /// EXPORT FORMATS 
    [ObservableProperty]
    private bool _exportPDF = true;
    [ObservableProperty]
    private bool _exportDWG = false;
    [ObservableProperty]
    private bool _exportDWF = false;
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsExportFormatSelected))]
    private int _exportFormatCount = 1;
    
    public bool IsExportFormatSelected => _exportFormatCount > 0;

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
    [AlsoNotifyCanExecuteFor(nameof(SetDwgVersionCommand))]
    private ACADVersion _dwgVersion;

    /// DISTRIBUTION
    public List<IssueFormatModel> IssueFormats { get; private set; }
    public bool CanRecordTransmittal { get; private set; }
    public bool IsDistributionValid => ValidateDistribution();

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsDistributionValid))]
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
    private ObservableCollection<ProjectDirectoryModel> _selectedProjectDirectory;
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsDistributionValid))]
    private ObservableCollection<TransmittalDistributionModel> _selectedDistribution;
    [ObservableProperty]
    private bool _hasDirectoryEntriesSelected = false;
    [ObservableProperty]
    private bool _hasDistributionEntriesSelected = false;


    /// SUMMARY PROGRESS
    [ObservableProperty]
    private bool _stepOneComplete = false;
    [ObservableProperty]
    private bool _stepTwoComplete = false;
    [ObservableProperty]
    private bool _stepThreeComplete = false;

    [ObservableProperty]
    private double _drawingSheetsProcessed = 0;
    [ObservableProperty]
    private string _drawingSheetProgressLabel = string.Empty;

    [ObservableProperty]
    private double _sheetTaskProcessed = 0;
    [ObservableProperty]
    private string _sheetTaskProgressLabel = string.Empty;

    [ObservableProperty]
    private bool _isFinishEnabled = true;
    [ObservableProperty]
    private bool _isBackEnabled = true;

    public TransmittalViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);
        
        WireUpSheetsPage();

        WireUpExportFormatsPage();

        WireUpDistributionPage();
    }

    private void WireUpSheetsPage()
    {
        SelectedDrawingSheets = new();
        SelectedDrawingSheets.CollectionChanged += SelectedDrawingSheets_CollectionChanged;

        DrawingSheets = GetDrawingSheets()
            .OrderBy(x => x.DrgVolume)
            .ThenBy(x => x.DrgLevel)
            .ThenBy(x => x.DrgNumber)
            .ToList<DrawingSheetModel>();
    }

    private void WireUpExportFormatsPage()
    {
        _printManager = App.RevitDocument.PrintManager;
        PrintSetup = _printManager.PrintSetup;
        
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

        IssueFormats = _settingsService.GlobalSettings.IssueFormats;
        IssueFormat = IssueFormats.FirstOrDefault();

        if (_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            _recordTransmittal = true;

            ProjectDirectory = new(_contactDirectoryService.GetProjectDirectory());
            ProjectDirectory.CollectionChanged += ProjectDirectory_CollectionChanged;

            SelectedProjectDirectory = new();
            SelectedProjectDirectory.CollectionChanged += SelectedProjectDirectory_CollectionChanged;

            Distribution = new();
            Distribution.CollectionChanged += Distribution_CollectionChanged;

            SelectedDistribution = new();
            SelectedDistribution.CollectionChanged += SelectedDistribution_CollectionChanged;
        }
    }


    #region Drawing Sheets

    private List<DrawingSheetModel> GetDrawingSheets()
    {
        var drawingSheets = new List<DrawingSheetModel>();

        // get the sheets in the model
        var sheets = new FilteredElementCollector(App.RevitDocument);
        sheets.OfClass(typeof(ViewSheet));
        if (sheets.Count() == 0)
        {
            return drawingSheets;
        }

        // retrieve the title block instances:
        var _filteredEC = new FilteredElementCollector(App.RevitDocument);
        _filteredEC.OfCategory(BuiltInCategory.OST_TitleBlocks);
        _filteredEC.OfClass(typeof(FamilyInstance));

        foreach (ViewSheet sheet in sheets)
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
            drawingSheet.StatusDescription = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid);

            drawingSheet.DrgOriginator = _settingsService.GlobalSettings.Originator;
            drawingSheet.DrgRole = _settingsService.GlobalSettings.Role;
            drawingSheet.DrgProj = _settingsService.GlobalSettings.ProjectIdentifier;

            // get the sizes with from the titleblock instances
            var Width = default(double);
            var Height = default(double);
            foreach (FamilyInstance FI in _filteredEC)
            {
                var _p = FI.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                if (_p.AsString() == sheet.SheetNumber)
                {
                    // we have the tb instance
                    _p = FI.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                    Width = _p.AsDouble().FootToMm();
                    _p = FI.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                    Height = _p.AsDouble().FootToMm();
                }
            }

            if (sheet.IsPlaceholder == false)
            {
                drawingSheet.DrgPaper = Util.GetPapersize(Width, Height);
                drawingSheet.IssueDate = sheet.get_Parameter(BuiltInParameter.SHEET_ISSUE_DATE).AsString();
                drawingSheet.DrgDrawn = sheet.get_Parameter(BuiltInParameter.SHEET_DRAWN_BY).AsString();
                drawingSheet.DrgChecked = sheet.get_Parameter(BuiltInParameter.SHEET_CHECKED_BY).AsString();
                drawingSheet.RevNotes = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DESCRIPTION).AsString();

                drawingSheets.Add(drawingSheet);
            }

        }

        return drawingSheets;
    }

    private void SelectedDrawingSheets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ValidateSelections();
    }

    private void ValidateSelections()
    {
        IsSheetsSelected = false;
        IsSelectedSheetsValid = false;
        
        if (_selectedDrawingSheets.Count > 0)
        {
            IsSheetsSelected = true;
            IsSelectedSheetsValid = true;
        }

        if (_settingsService.GlobalSettings.UseISO19650 == true)
        {
            //todo validate the selected sheets meet ISO19650 rules and all parameters have values
            foreach (TransmittalItemModel item in _selectedDrawingSheets)
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
        var sheets = new FilteredElementCollector(App.RevitDocument);
        sheets.OfClass(typeof(ViewSheet));

        //loop throught the selected items in the grid
        foreach (DrawingSheetModel sheetModel in _selectedDrawingSheets)
        {
            //set the model values
            sheetModel.DrgStatus = model.Code;
            sheetModel.StatusDescription = model.Description;

            //update the sheet status in revit
            foreach (ViewSheet sheet in sheets)
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

        ValidateSelections();
    }

    private void SetSheetStatus(ViewSheet sheet, string status, string description)
    {
        Transaction trans = null;
        var m_paramList = new List<Parameter>();
        var m_paramSet = sheet.Parameters;
        var enumerator = m_paramSet.GetEnumerator();
        enumerator.Reset();

        while (enumerator.MoveNext())
            m_paramList.Add(enumerator.Current as Parameter);

        foreach (Parameter m_param in m_paramList)
        {
            if (m_param.IsShared == true)
            {
                if (m_param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusParamGuid)
                {
                    {
                        try
                        {
                            trans = new Transaction(App.RevitDocument, "Set Sheet Status");
                            var failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningSwallower());
                            trans.SetFailureHandlingOptions(failOpt);
                            trans.Start();
                            var cp = new ParameterHelper(m_param)
                            {
                                Value = status
                            };
                            trans.Commit();
                        }
                        catch (Exception)
                        {
                            trans.RollBack();
                        }
                    }
                }

                if (m_param.IsShared == true)
                {
                    if (m_param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)
                    {
                        try
                        {
                            trans = new Transaction(App.RevitDocument, "Set Sheet Status Description");
                            var failOpt = trans.GetFailureHandlingOptions();
                            failOpt.SetFailuresPreprocessor(new WarningSwallower());
                            trans.SetFailureHandlingOptions(failOpt);
                            trans.Start();
                            var cp = new ParameterHelper(m_param)
                            {
                                Value = description
                            };
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
        var sheets = new FilteredElementCollector(App.RevitDocument);
        sheets.OfClass(typeof(ViewSheet));

        //loop throught the selected items in the grid
        foreach (DrawingSheetModel sheetModel in SelectedDrawingSheets)
        {
            foreach (ViewSheet sheet in sheets)
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

        ValidateSelections();
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

        foreach (Revision revision in collector)
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

    
    [ICommand]
    private void ProcessSheets()
    {
        IsBackEnabled = false;
        IsFinishEnabled = false;
        
        try
            {
                var sheets = new FilteredElementCollector(App.RevitDocument);
                sheets.OfClass(typeof(ViewSheet));

                foreach (DrawingSheetModel drawingSheet in _selectedDrawingSheets)
                {
                    foreach (ViewSheet sheet in sheets)
                    {
                        // abort if cancel was clicked
                        if (_abortFlag == true)
                        {
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

                                // build the filename
                                string fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
                                     _settingsService.GlobalSettings.ProjectIdentifier,
                                     _settingsService.GlobalSettings.ProjectName,
                                     _settingsService.GlobalSettings.Originator,
                                     Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetVolumeParamGuid),
                                     Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetLevelParamGuid),
                                     Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.DocumentTypeParamGuid),
                                     _settingsService.GlobalSettings.Role,
                                     sheet.SheetNumber,
                                     sheet.Name,
                                     sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString(),
                                     Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetStatusParamGuid),
                                     Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid));

                                DrawingSheetProgressLabel = $"Processing sheet : {fileName}";
                                SheetTaskProcessed = 0;
                                SheetTaskProgressLabel = string.Empty;
                                DispatcherHelper.DoEvents();

                            if (_exportPDF == true)
                                {
    #if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
                                    _exportPDFService.ExportPDF($"{fileName}.pdf", 
                                    App.revitDocument, 
                                    sheet);
    #else
                                    _exportPDFService.ExportPDF(fileName,
                                        App.RevitDocument,
                                        views,
                                        PdfExportOptions);
    #endif

                                    //TODO - actually check if the export worked OK
                                    SheetTaskProgressLabel = "Exported PDF";
                                    SheetTaskProcessed += 1;

                                // to allow cancel button working
                                DispatcherHelper.DoEvents();
                            }

                           

                                if (_abortFlag == true)
                                {
                                    this.OnClosingRequest();
                                    return;
                                }

                                if (_exportDWG == true)
                                {
                                    DwgExportOptions.FileVersion = (ACADVersion)_dwgVersion;
                                    DwgExportOptions.LayerMapping = DwgLayerMapping.Name;

                                    _exportDWGService.ExportDWG($"{fileName}.dwg",
                                        DwgExportOptions,
                                        views,
                                        App.RevitDocument);

                                    //TODO - actually check if the export worked OK
                                    SheetTaskProgressLabel = "Exported DWG";
                                    SheetTaskProcessed += 1;

                                // to allow cancel button working
                                DispatcherHelper.DoEvents();
                            }

                            

                                if (_abortFlag == true)
                                {
                                    this.OnClosingRequest();
                                    return;
                                }

                                if (_exportDWF == true)
                                {
                                    var argsheetsize = Util.GetSheetsize(sheet, App.RevitDocument);

                                    _exportDWFService.ExportDWF($"{fileName}.dwf",
                                        argsheetsize,
                                        _printSetup,
                                        DwfExportOptions,
                                        App.RevitDocument,
                                        views);

                                    //TODO - actually check if the export worked OK
                                    SheetTaskProgressLabel = "Exported DWF";
                                    SheetTaskProcessed += 1;

                                // to allow cancel button working
                                DispatcherHelper.DoEvents();
                            }
                                                        

                                if (_abortFlag == true)
                                {
                                    this.OnClosingRequest();
                                    return;
                                }

                                if (RecordTransmittal == true)
                                {
                                    // Mark sheets issued date.
                                    SetIssueDate(sheet);

                                    // Mark revisions issued
                                    SetRevisionsIssued(sheet);
                                }

                                DrawingSheetsProcessed += 1;


                            // to allow cancel button working
                            DispatcherHelper.DoEvents();
                        }
                        }
                    }
                }

                this.OnClosingRequest();
                return;
            }
            catch (Exception ex)
            {
                //TaskDialog.Show("Error", $"There has been an error processing sheet exports. {Environment.NewLine} {ex}", TaskDialogCommonButtons.Ok);
                this.OnClosingRequest();
                return;
            }
    }
    
    private void SetRevisionsIssued(ViewSheet sheet)
    {
        try
        {
            ICollection<ElementId> revisions = sheet.GetAllRevisionIds();

            // Find revisions whose description matches input string
            var collector = new FilteredElementCollector(App.RevitDocument, revisions);
            collector.OfCategory(BuiltInCategory.OST_Revisions);
            collector.WhereElementIsNotElementType();
            if (revisions.Count > 0)
            {
                foreach (Revision revision in collector)
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
        catch (Exception ex)
        {
            trans.RollBack();
        }
    }
    #endregion

    #region Export Formats
    [ICommand]
    private void GetFormatCount()
    {
        bool[] formats = { _exportPDF, _exportDWG, _exportDWF };

        ExportFormatCount = formats.Sum(x => x ? 1 : 0);
    }

    [ICommand]
    private void SetDwgVersion()
    {
        DwgExportOptions.FileVersion = (ACADVersion)_dwgVersion;
    }

    #endregion

    #region Distribution
    private bool ValidateDistribution()
    {
        if (_recordTransmittal == true && (_distribution is null || _distribution.Count == 0))
        {
            return false;
        }

        return true;
    }

    private void ProjectDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        
    }

    private void Distribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        
    }

    private void SelectedProjectDirectory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDirectoryEntriesSelected = true;

        if (_selectedProjectDirectory == null || _selectedProjectDirectory.Count == 0)
        {
            HasDirectoryEntriesSelected = false;
        }
    }

    private void SelectedDistribution_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        HasDistributionEntriesSelected = true;

        if (_selectedDistribution == null || _selectedDistribution.Count == 0)
        {
            HasDistributionEntriesSelected = false;
        }
    }

    [ICommand]
    private void AddToDistribition()
    {

    }

    [ICommand]
    private void RemoveFromDistribution()
    {

    }


    #endregion

}

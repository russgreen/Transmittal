using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using Transmittal.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.ViewModels;
using Transmittal.Library.Services;
using Transmittal.Library.Extensions;
using Transmittal.Models;
using Transmittal.Requesters;
using Transmittal.Services;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Messaging;
using Transmittal.Messages;
using Transmittal.Views;
using Syncfusion.Windows.Tools.Controls;
using System.Windows.Threading;

namespace Transmittal.ViewModels;

//[INotifyPropertyChanged]
internal partial class TransmittalViewModel : BaseViewModel, IStatusRequester, IRevisionRequester, IPersonRequester
{
    private readonly ISettingsServiceRvt _settingsServiceRvt = Ioc.Default.GetRequiredService<ISettingsServiceRvt>();
    private readonly ISettingsService _settingsService = Ioc.Default.GetRequiredService<ISettingsService>(); 
    private readonly IExportPDFService _exportPDFService = Ioc.Default.GetRequiredService<IExportPDFService>();
    private readonly IExportDWGService _exportDWGService = Ioc.Default.GetRequiredService<IExportDWGService>();
    private readonly IExportDWFService _exportDWFService = Ioc.Default.GetRequiredService<IExportDWFService>();
    private readonly IContactDirectoryService _contactDirectoryService = Ioc.Default.GetRequiredService<IContactDirectoryService>();
    private readonly ITransmittalService _transmittalService = Ioc.Default.GetRequiredService<ITransmittalService>();

    public string WindowTitle { get; private set; }

    private Thread _progressWindowThread;

    [ObservableProperty]
    private System.Windows.Visibility _isWindowVisible = System.Windows.Visibility.Visible;

    private TransmittalModel _newTransmittal = new();    
    
    ///  DRAWING SHEETS
    public List<DrawingSheetModel> DrawingSheets { get; private set; }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSheetsSelected))]
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
    [NotifyPropertyChangedFor(nameof(IsExportFormatSelected))]
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
    [NotifyCanExecuteChangedFor(nameof(SetDwgVersionCommand))]
    private ACADVersion _dwgVersion;

    /// DISTRIBUTION
    public List<IssueFormatModel> IssueFormats { get; private set; }
    public bool CanRecordTransmittal { get; private set; }
    public bool IsDistributionValid => ValidateDistribution();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDistributionValid))]
    private bool _recordTransmittal = false;
    [ObservableProperty]
    private int _copies = 1;
    [ObservableProperty]
    private IssueFormatModel _issueFormat;
    [ObservableProperty]
    private ObservableCollection<ProjectDirectoryModel> _projectDirectory;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDistributionValid))]
    private ObservableCollection<TransmittalDistributionModel> _distribution;
    [ObservableProperty]
    private ObservableCollection<object> _selectedProjectDirectory;
    [ObservableProperty]
    private ObservableCollection<object> _selectedDistribution;
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
    private bool _isFinishEnabled = true;
    [ObservableProperty]
    private bool _isBackEnabled = true;

    private List<DocumentModel> _exportedFiles = new();

    public TransmittalViewModel()
    {
        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        _settingsServiceRvt.GetSettingsRvt(App.RevitDocument);
        
        WireUpSheetsPage();

        WireUpExportFormatsPage();

        WireUpDistributionPage();

        WeakReferenceMessenger.Default.Register<CancelTransmittalMessage>(this, (r, m) =>
        {
            _abortFlag = true;
        });
    }

    private void WireUpSheetsPage()
    {
        SelectedDrawingSheets = new();
        SelectedDrawingSheets.CollectionChanged += SelectedDrawingSheets_CollectionChanged;

        DrawingSheets = GetDrawingSheets()
            .OrderBy(x => x.DrgVolume)
            .ThenBy(x => x.DrgNumber)
            .ToList<DrawingSheetModel>();
    }

    private void WireUpExportFormatsPage()
    {
        _printManager = App.RevitDocument.PrintManager;
        PrintSetup = _printManager.PrintSetup;
        PrintSetup.CurrentPrintSetting.PrintParameters.HideUnreferencedViewTags = true;
        _pdfExportOptions.HideUnreferencedViewTags = true;

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
            drawingSheet.DrgStatusDescription = Util.GetParameterValueString(sheet, _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid);

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
            sheetModel.DrgStatusDescription = model.Description;

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
            m_paramList.Add((Parameter)enumerator.Current);

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
        foreach (DrawingSheetModel sheetModel in _selectedDrawingSheets)
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
   

    #endregion

    #region Export Formats
    [RelayCommand]
    private void GetFormatCount()
    {
        bool[] formats = { _exportPDF, _exportDWG, _exportDWF };

        ExportFormatCount = formats.Sum(x => x ? 1 : 0);
    }

    [RelayCommand]
    private void SetDwgVersion()
    {
        DwgExportOptions.FileVersion = (ACADVersion)_dwgVersion;
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
        OnPropertyChanged(nameof(IsDistributionValid));
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

    [RelayCommand]
    private void AddToDistribition()
    {
        foreach (ProjectDirectoryModel directoryContact in _selectedProjectDirectory.Cast<ProjectDirectoryModel>().ToList())
        {
            if (directoryContact != null)
            {
                TransmittalDistributionModel distributionRecord = new()
                {
                    
                    Company = directoryContact.Company,
                    Person = directoryContact.Person,
                    PersonID = directoryContact.Person.ID,
                    TransCopies = _copies,
                    TransFormat = _issueFormat.Code
                };

                _projectDirectory.Remove(directoryContact);
                _distribution.Add(distributionRecord);
            }
        }
    }

    [RelayCommand]
    private void RemoveFromDistribution()
    {
        foreach (TransmittalDistributionModel distributionRecord in _selectedDistribution.Cast<TransmittalDistributionModel>().ToList())
        {
            if (distributionRecord != null)
            {
                ProjectDirectoryModel directoryContact = new()
                {
                    Company = distributionRecord.Company,
                    Person = distributionRecord.Person
                };

                _distribution.Remove(distributionRecord);
                _projectDirectory.Add(directoryContact);
            }
        }
    }


    #endregion

    [RelayCommand]
    private void ProcessSheets()
    {
        IsBackEnabled = false;
        IsFinishEnabled = false;

        IsWindowVisible = System.Windows.Visibility.Hidden;

        OpenProgressWindow();

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
                            DispatcherHelper.DoEvents();

                            if (_exportPDF == true)
                            {
                                SheetTaskProgressLabel = "Exporting PDF...";
                                SendProgressMessage();
                                DispatcherHelper.DoEvents();

#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
                                    _exportPDFService.ExportPDF($"{fileName}.pdf", 
                                    App.revitDocument, 
                                    sheet);
#else
                                var filePath = _exportPDFService.ExportPDF(fileName,
                                    App.RevitDocument,
                                    views,
                                    PdfExportOptions);

                                DocumentModel pdf = new DocumentModel(drawingSheet);
                                pdf.FilePath = filePath;
                                _exportedFiles.Add(pdf);
#endif

                                //TODO - actually check if the export worked OK
                                SheetTaskProgressLabel = "Exporting PDF...DONE";
                                SheetTaskProcessed += 1;
                                SendProgressMessage();
                                DispatcherHelper.DoEvents();
                            }



                            if (_abortFlag == true)
                            {
                                CloseProgress();
                                this.OnClosingRequest();
                                return;
                            }

                            if (_exportDWG == true)
                            {
                                SheetTaskProgressLabel = "Exporting DWG...";
                                SendProgressMessage();
                                DispatcherHelper.DoEvents();

                                DwgExportOptions.FileVersion = (ACADVersion)_dwgVersion;
                                DwgExportOptions.LayerMapping = DwgLayerMapping.Name;

                                var filePath = _exportDWGService.ExportDWG($"{fileName}.dwg",
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
                                DispatcherHelper.DoEvents();
                            }



                            if (_abortFlag == true)
                            {
                                CloseProgress();
                                this.OnClosingRequest();
                                return;
                            }

                            if (_exportDWF == true)
                            {
                                SheetTaskProgressLabel = "Exporting DWF...";
                                SendProgressMessage();
                                DispatcherHelper.DoEvents();

                                var argsheetsize = Util.GetSheetsize(sheet, App.RevitDocument);

                                var filePath = _exportDWFService.ExportDWF($"{fileName}.dwf",
                                    argsheetsize,
                                    _printSetup,
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
                                DispatcherHelper.DoEvents();
                            }


                            if (_abortFlag == true)
                            {
                                CloseProgress();
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
                            SendProgressMessage();
                            DispatcherHelper.DoEvents();
                        }
                    }
                }
            }

            StepOneComplete = true;
            CurrentStepProgressLabel = "Recording transmittal...";
            SendProgressMessage();
            DispatcherHelper.DoEvents();

            if (RecordTransmittal == true)
            {
                RecordTransmittalInDatabase();
                StepTwoComplete = true;
                DispatcherHelper.DoEvents();
                CurrentStepProgressLabel = "Displaying report...";
                SendProgressMessage();
                LaunchTransmittalReport();
                StepThreeComplete = true;
                DispatcherHelper.DoEvents();
            }

            if(_settingsService.GlobalSettings.UseExtranet == true)
            {
                CopyFilesForExtranet();
            }

            OpenExporerToExportedFilesLocations();
             
            //just pause before closing the window
            Thread.Sleep(5000);

            CloseProgress();

            this.OnClosingRequest();
            return;
        }
        catch (Exception ex)
        {
            Autodesk.Revit.UI.TaskDialog.Show("Error", $"There has been an error processing sheet exports. {Environment.NewLine} {ex}", Autodesk.Revit.UI.TaskDialogCommonButtons.Ok);

            CloseProgress();

            this.OnClosingRequest();
            return;
        }
    }

    private void CopyFilesForExtranet()
    {
        var extranetFolderName = "Extranet";

        var folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(extranetFolderName);

        if (!_settingsService.GlobalSettings.DrawingIssueStore.Contains("<Format>"))
        {
            folderPath = System.IO.Path.Combine(folderPath, extranetFolderName);
        }

        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

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



            var fullPath = System.IO.Path.Combine(folderPath, fileName + fileInfo.Extension);

            fileInfo.CopyTo(fullPath, true);

        }

        Process.Start("explorer.exe",
    $"/root, {_settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(extranetFolderName)}");

    }

    private void OpenExporerToExportedFilesLocations()
    {
        if (_exportDWG == true)
        {
            Process.Start("explorer.exe",
                $"/root, {_settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWG.ToString())}");
        }

        if (_exportDWF == true)
        {
            Process.Start("explorer.exe",
                $"/root, {_settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWF.ToString())}");
        }

        if (_exportPDF == true)
        {
            Process.Start("explorer.exe",
                $"/root, {_settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.PDF.ToString())}");
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
        catch
        {
            trans.RollBack();
        }
    }

    private void RecordTransmittalInDatabase()
    {
        if(RecordTransmittal == false)
        {
            return;
        }

        _newTransmittal.TransDate = DateTime.Now;
        _transmittalService.CreateTransmittal(_newTransmittal);

        foreach (TransmittalItemModel item in _selectedDrawingSheets)
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
            _transmittalService.CreateTransmittalItem(item);
        }

        foreach (TransmittalDistributionModel dist in _distribution)
        {
            dist.TransID = _newTransmittal.ID;
            _transmittalService.CreateTransmittalDist(dist);
        }
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
        var newPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentPath, @"..\..\..\..\"));

        var pathToExe = System.IO.Path.Combine(newPath, @$"Transmittal.Desktop\bin\x64\Debug\net48", "Transmittal.Desktop.exe");
#else
        var pathToExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Transmittal", "Transmittal.Desktop.exe");
#endif

        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = pathToExe;
        processStartInfo.Arguments = $"--transmittal={_newTransmittal.ID} \"--database={dbFile}\"";

        Process.Start(processStartInfo);        
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

    private void CloseProgress()
    {
        Dispatcher.FromThread(_progressWindowThread).InvokeShutdown();
    }

    private void SendProgressMessage()
    {
        WeakReferenceMessenger.Default.Send(
            new ProgressUpdateMessage(
                new ProgressMessageModel 
                { 
                    CurrentStepProgressLabel = _currentStepProgressLabel,
                    DrawingSheetsToProcess = _selectedDrawingSheets.Count,
                    DrawingSheetsProcessed = _drawingSheetsProcessed,
                    DrawingSheetProgressLabel = _drawingSheetProgressLabel,
                    SheetTasksToProcess = _exportFormatCount,
                    SheetTaskProcessed = _sheetTaskProcessed,
                    SheetTaskProgressLabel = _sheetTaskProgressLabel
                }));
    }
}

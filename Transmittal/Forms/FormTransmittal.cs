using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.WinForms.DataGrid;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Transmittal.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Requesters;
using Transmittal.Services;

namespace Transmittal.Forms;
public partial class FormTransmittal : System.Windows.Forms.Form, IStatusRequester, IRevisionRequester
{
    private Array _colors = Enum.GetValues(typeof(ColorDepthType));
    private Array _rasterQualities = Enum.GetValues(typeof(RasterQualityType));

    private TransmittalModel _newTransmittal = new();
    private List<ProjectDirectoryModel> _projectDirectory;
    private List<TransmittalDistributionModel> _distribution = new();

    private List<string> _exportFormats = new List<string>();

    private bool _transmittalRecorded = false;
    private bool _abortFlag = false;
    private bool _processingsheets = false;
    private bool _hasInvalidRowSelected = false;

    private PDFExportOptions _pdfExportOptions;
    private DWGExportOptions _dwgExportOptions;
    private DWFExportOptions _dwfExportOptions;
    private PrintManager _printManager;
    private PrintSetup _printSetup;

    private readonly ISettingsService _settingsService;
    private readonly ISettingsServiceRvt _settingsServiceRvt;
    private readonly IExportPDFService _exportPDFService;
    private readonly IExportDWGService _exportDWGService;
    private readonly IExportDWFService _exportDWFService;
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ITransmittalService _transmittalService;
            
    public FormTransmittal(ExternalCommandData CommandData)
    {
        InitializeComponent();
               
        _settingsServiceRvt = App.ServiceProvider.GetRequiredService<ISettingsServiceRvt>();
        _settingsService = App.ServiceProvider.GetRequiredService<ISettingsService>();

        _exportPDFService = App.ServiceProvider.GetRequiredService<IExportPDFService>();
        _exportDWGService = App.ServiceProvider.GetRequiredService<IExportDWGService>();
        _exportDWFService = App.ServiceProvider.GetRequiredService<IExportDWFService>();
        _contactDirectoryService = App.ServiceProvider.GetRequiredService<IContactDirectoryService>();
        _transmittalService = App.ServiceProvider.GetRequiredService<ITransmittalService>();

        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        this.Text = $"Transmittal {informationVersion} ({App.RevitDocument.Title})";

        _printManager = App.RevitDocument.PrintManager;

        WireUpControls();

        CheckPDFPrinterIsInstalled();

        LoadSheetsToGrid();

        this.wizardControlPageSheets.NextEnabled = false;
        this.wizardControlPageDistribution.NextEnabled = false;

        if (_settingsService.GlobalSettings.RecordTransmittals == true)
        {
            LoadDirectoryToGrid();
        }
        else
        {
            this.checkBoxRecordIssue.Checked = false;
            this.checkBoxRecordIssue.Enabled = false;
            this.wizardControlPageDistribution.NextEnabled = true;
        }

        Application.DoEvents();
    }

    private void CheckPDFPrinterIsInstalled()
    {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        // check if Bullzip is installed
        if (Util.IsPrinterInstalled(_pdfPrinterName) == true)
        {
            this.checkBoxExportPDF.Enabled = true;
            this.checkBoxExportPDF.Checked = true;
        }
        else
        {
            this.checkBoxExportPDF.Enabled = false;
            this.checkBoxExportPDF.Checked = false;
            MessageBox.Show("You must install the Bullzip PDF Printer before using the PDF export option.", "Bullzip PDF Printer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
#else
        //from REVIT2022 we use Revit's in-built PDF exporter
#endif
    }

    private void WireUpControls()
    {
        //build the issue formats list and add to the combo
        this.comboBoxFormat.DataSource = _settingsService.GlobalSettings.IssueFormats;
        this.comboBoxFormat.DisplayMember = nameof(IssueFormatModel.Description);
        this.comboBoxFormat.ValueMember = nameof(IssueFormatModel.Code);

        //build the DWG export mappings and add to the combo
        List<DWGLayerMappingModel> dWGLayerMappings = new();
        dWGLayerMappings.Add(new DWGLayerMappingModel() { Id = 0, Name = "BS1192" });
        dWGLayerMappings.Add(new DWGLayerMappingModel() { Id = 1, Name = "AIA" });
        dWGLayerMappings.Add(new DWGLayerMappingModel() { Id = 2, Name = "CP83" });
        dWGLayerMappings.Add(new DWGLayerMappingModel() { Id = 3, Name = "ISO13567" });
        
        this.comboBoxDWGLayers.DataSource = dWGLayerMappings;
        this.comboBoxDWGLayers.DisplayMember = nameof(DWGLayerMappingModel.Name);
        this.comboBoxDWGLayers.ValueMember = nameof(DWGLayerMappingModel.Name);

        //configure export settings controls
        this.comboBoxDWFColors.DataSource = _colors;
        this.comboBoxDWFColors.SelectedItem = ColorDepthType.Color;
        this.comboBoxDWFRasterQuality.DataSource = _rasterQualities;
        this.comboBoxDWFRasterQuality.SelectedItem = RasterQualityType.High;
        this.comboBoxPDFColors.DataSource = _colors;
        this.comboBoxPDFColors.SelectedItem = ColorDepthType.Color;
        this.comboBoxPDFRasterQuality.DataSource = _rasterQualities;
        this.comboBoxPDFRasterQuality.SelectedItem = RasterQualityType.High;       
    }

    private void wizardControl1_BeforeNext(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if(this.wizardControl1.SelectedWizardPage == this.wizardControlPageFormats)
        {
            SetExportOptions();
            SetExportFormatsList();
        }
    }

    private void wizardControl1_HelpButton_Click(object sender, EventArgs e)
    {
        // open website URL
        System.Diagnostics.Process.Start("https://russgreen.github.io/transmittal/");
    }

    private void wizardControl1_FinishButton_Click(object sender, EventArgs e)
    {
        System.IO.Stream _imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Transmittal.Resources.GreenCheck.png");
        try
        {
            // disable the finish button so its not clicked again.
            this.wizardControl1.FinishButton.Enabled = false;

            //BuildIssuePaths(this.checkBoxRecordIssue.Checked);

            //process all the scheet exports and allow for the operation to be cancelled
            _abortFlag = ProcessSheets();
            if (_abortFlag == true)
            {
                return;
            }

            this.pictureBox1.Image = System.Drawing.Image.FromStream(_imageStream);
            this.pictureBox1.Refresh();
            Application.DoEvents();

            //save the transmittal to the db
            if (_settingsService.GlobalSettings.RecordTransmittals == true)
            {
                // Step 1 - Create the transmittal model
                _transmittalService.CreateTransmittal(_newTransmittal);

                this.pictureBox2.Image = System.Drawing.Image.FromStream(_imageStream);
                this.pictureBox2.Refresh();
                Application.DoEvents();

                // Step 2 - Add the transmittal items
                foreach (TransmittalItemModel item in this.sfDataGridSheets.SelectedItems)
                {
                    item.TransID = _newTransmittal.ID;

                    _transmittalService.CreateTransmittalItem(item);
                }

                this.pictureBox3.Image = System.Drawing.Image.FromStream(_imageStream);
                this.pictureBox3.Refresh();
                Application.DoEvents();

                // Step 3 - Record transmittal distribution list
                foreach (TransmittalDistributionModel dist in _distribution)
                {
                    dist.TransID = _newTransmittal.ID;

                    _transmittalService.CreateTransmittalDist(dist);
                }

                _transmittalRecorded = true;
                this.pictureBox4.Image = System.Drawing.Image.FromStream(_imageStream);
                this.pictureBox4.Refresh();
                Application.DoEvents();
            }
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error", ex.ToString(), TaskDialogCommonButtons.Ok);
        }
        finally
        {
            if (this.checkBoxRecordIssue.Checked == true && 
                _settingsService.GlobalSettings.UseExtranet == true)
            {
                CopyForExtranet();
            }

            foreach (var format in _exportFormats)
            {
                Process.Start("explorer.exe", "/root," + _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(format));
            }
            

            if (this.checkBoxRecordIssue.Checked == true & _transmittalRecorded == true)
            {
                // Step 5 - Generate transmittal sheet report

            }

            this.Close();
        }
    }

    private void wizardControl1_CancelButton_Click(object sender, EventArgs e)
    {


    }


#region Drawing Sheets    
    private void LoadSheetsToGrid()
    {
        //build the datagrid columns
        this.sfDataGridSheets.Columns.Clear();
        this.sfDataGridSheets.Columns.Add(new GridCheckBoxSelectorColumn() { MappingName = "SelectorColumn", HeaderText = string.Empty, AllowCheckBoxOnHeader = false, Width = 34, CheckBoxSize = new Size(14, 14) });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgNumber", HeaderText = "Number", Width = 100 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgRev", HeaderText = "Revision", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgName", HeaderText = "Name", Width = 250 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgVolume", HeaderText = "Volume", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgLevel", HeaderText = "Level", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgType", HeaderText = "Type", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgStatus", HeaderText = "Status", Width = 50 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "StatusDescription", HeaderText = "Status Description", Width = 120 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgPaper", HeaderText = "Paper", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgScale", HeaderText = "Scale", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "IssueDate", HeaderText = "Issue Date", Width = 100 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgDrawn", HeaderText = "Dr", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "DrgChecked", HeaderText = "Ch", Width = 75 });
        this.sfDataGridSheets.Columns.Add(new GridTextColumn() { MappingName = "RevNotes", HeaderText = "Rev Notes", MinimumWidth = 300 });

        this.sfDataGridSheets.ValidationMode = Syncfusion.WinForms.DataGrid.Enums.GridValidationMode.InView;

        //refresh the datasource
        this.sfDataGridSheets.DataSource = GetDrawingSheets()
            .OrderBy(x => x.DrgVolume)
            .ThenBy(x => x.DrgLevel)
            .ThenBy(x => x.DrgNumber)
            .ToList<DrawingSheetModel>();
        
        this.sfDataGridSheets.View.Refresh();

        ValidateSelections();
    }

    private void buttonRevise_Click(object sender, EventArgs e)
    {
        //call the revision selector form
        FormRevisions frm = new(this);
        frm.ShowDialog(this);
    }

    public void RevisionComplete(RevisionDataModel model)
    {
        //get the sheets in the model
        var sheets = new FilteredElementCollector(App.RevitDocument);
        sheets.OfClass(typeof(ViewSheet));

        //loop throught the selected items in the grid
        foreach (DrawingSheetModel sheetModel in this.sfDataGridSheets.SelectedItems)
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

    private void buttonSheetStatus_Click(object sender, EventArgs e)
    {
        //call the status selector form
        FormStatus frm = new(this);
        frm.ShowDialog(this);
    }

    public void StatusComplete(DocumentStatusModel model)
    {
        //get the sheets in the model
        var sheets = new FilteredElementCollector(App.RevitDocument);
        sheets.OfClass(typeof(ViewSheet));

        //loop throught the selected items in the grid
        foreach (DrawingSheetModel sheetModel in this.sfDataGridSheets.SelectedItems)
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

        this.sfDataGridSheets.Refresh();

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

        // Check if revision should be added
        // For Each revision As Element In collector
        // Dim seqParam As Parameter = revision.Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM)
        // Dim sequence As [String] = seqParam.AsString()
        // If sequence.Contains(toMatch) Then
        // revisions.Add(revision.Id)
        // End If
        // Next
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
    
    private void sfDataGridSheets_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
    {
        if (this.sfDataGridSheets.SelectedItems.Count > 0)
        {
            ValidateSelections();

            this.buttonRevise.Enabled = true;
            this.buttonSheetStatus.Enabled = true;
            this.wizardControlPageSheets.NextEnabled = true;
        }
        else
        {
            this.buttonRevise.Enabled = false;
            this.buttonSheetStatus.Enabled = false;
            this.wizardControlPageSheets.NextEnabled = false;
        }
    }
        
    private void ValidateSelections()
    {
        _hasInvalidRowSelected = false;

        //throw new NotImplementedException();
        if (_settingsService.GlobalSettings.UseISO19650 == true)
        {
            //todo validate the selected sheets meet ISO19650 rules and all parameters have values
            foreach (TransmittalItemModel item in this.sfDataGridSheets.SelectedItems)
            {
                if ((string.IsNullOrEmpty(item.DrgVolume)) ||
                    (string.IsNullOrEmpty(item.DrgLevel)) ||
                    (string.IsNullOrEmpty(item.DrgType)) ||
                    (string.IsNullOrEmpty(item.DrgRev)) || 
                    (string.IsNullOrEmpty(item.DrgStatus)))
                {
                    _hasInvalidRowSelected = true;
                    break;
                }
            }

            if (_hasInvalidRowSelected == true)
            {
                //we can't issue this selection
                this.checkBoxRecordIssue.Checked = false;
                this.checkBoxRecordIssue.Enabled = false;
            }
            else
            {
                this.checkBoxRecordIssue.Checked = true;
                this.checkBoxRecordIssue.Enabled = true;
            }
        }        
    }

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

            //var pVolume = default(Parameter);
            //var pLevel = default(Parameter);
            //var pType = default(Parameter);
            //var pStatus = default(Parameter);
            //var pStatusDescription = default(Parameter);

            // Sheet Volume               9c16757c-175a-451a-a5d4-c4a6ff291acb
            // Sheet Level                e51af162-9025-48a0-bd2c-bc833fab0db0	
            // Document Type              eb57d296-7d9c-459f-ace1-0bdaf95c3b29	
            // Sheet Status               3304f169-ceb9-40b9-a69d-d8f3eb0a3fb9
            // Sheet Status Description   4effad6a-f05d-43dd-afb1-c2b6c5cb5b9a

            //foreach (var param in sheet.GetParameters("Sheet Volume"))
            //{
            //    if (param.IsShared == true)
            //    {
            //        if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetVolumeParamGuid)
            //        {
            //            pVolume = param;
            //        }
            //    }
            //}

            //foreach (var param in sheet.GetParameters("Sheet Level"))
            //{
            //    if (param.IsShared == true)
            //    {
            //        if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetLevelParamGuid)
            //        {
            //            pLevel = param;
            //        }
            //    }
            //}

            //foreach (var param in sheet.GetParameters("Document Type"))
            //{
            //    if (param.IsShared == true)
            //    {
            //        if (param.GUID.ToString() == _settingsService.GlobalSettings.DocumentTypeParamGuid)
            //        {
            //            pType = param;
            //        }
            //    }
            //}

            //foreach (var param in sheet.GetParameters("Sheet Status"))
            //{
            //    if (param.IsShared == true)
            //    {
            //        if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusParamGuid)
            //        {
            //            pStatus = param;
            //        }
            //    }
            //}

            //foreach (var param in sheet.GetParameters("Sheet Status Description"))
            //{
            //    if (param.IsShared == true)
            //    {
            //        if (param.GUID.ToString() == _settingsService.GlobalSettings.SheetStatusDescriptionParamGuid)
            //        {
            //            pStatusDescription = param;
            //        }
            //    }
            //}

            //drawingSheet.DrgVolume = pVolume.AsString();
            //drawingSheet.DrgLevel = pLevel.AsString();
            //drawingSheet.DrgType = pType.AsString();
            //drawingSheet.DrgStatus = pStatus.AsString();
            //drawingSheet.StatusDescription = pStatusDescription.AsString();

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

    private bool ProcessSheets()
    {
        try
        {
            //initialise the progress bars and labels
            this.progressBarDrawingSheets.Value = 0;
            this.progressBarDrawingSheets.Visible = true;
            this.progressBarDrawingSheets.Maximum = 100;
            this.progressBarDrawingSheets.Step = (int)(100 / this.sfDataGridSheets.SelectedItems.Count);

            this.labelDrawingSheet.Text = "";
            this.labelDrawingSheet.Visible = true;
            this.labelTask.Text = "";
            this.labelTask.Visible = true;

            var sheets = new FilteredElementCollector(App.RevitDocument);
            sheets.OfClass(typeof(ViewSheet));

            foreach (DrawingSheetModel drawingSheet in this.sfDataGridSheets.SelectedItems)
            {
                foreach (ViewSheet sheet in sheets)
                {
                    // abort if cancel was clicked
                    if (_abortFlag == true)
                    {
                        return true;
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

                            this.labelDrawingSheet.Text = fileName;
                            this.labelTask.Text = "";

                            int maxStep = 0;
                            if (this.checkBoxExportDWF.Checked) { maxStep += 1; }
                            if (this.checkBoxExportDWG.Checked) { maxStep += 1; }
                            if (this.checkBoxExportPDF.Checked) { maxStep += 1; }

                            this.progressBarTask.Value = 0;
                            this.progressBarTask.Visible = true;
                            this.progressBarTask.Maximum = 100;
                            this.progressBarTask.Step = (int)(100 / maxStep);

                            Refresh();

                            if (this.checkBoxExportDWG.Checked == true)
                            {
                                _exportDWGService.ExportDWG($"{fileName}.dwg", 
                                    _dwgExportOptions, 
                                    views, 
                                    App.RevitDocument);

                                //TODO - actually check if the export worked OK
                                this.labelTask.Text = "Exported DWG";

                                // to allow cancel button working
                                Application.DoEvents();
                            }

                            this.progressBarTask.PerformStep();
                            Refresh();

                            if (_abortFlag == true)
                            {
                                return true;
                            }

                            if (this.checkBoxExportPDF.Checked == true)
                            {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
                                _exportPDFService.ExportPDF($"{fileName}.pdf", 
                                App.revitDocument, 
                                sheet);
#else
                                _exportPDFService.ExportPDF(fileName, 
                                    App.RevitDocument, 
                                    views, 
                                    _pdfExportOptions);
#endif

                                //TODO - actually check if the export worked OK
                                this.labelTask.Text = "Exported PDF";

                                // to allow cancel button working
                                Application.DoEvents();
                            }

                            this.progressBarTask.PerformStep();
                            Refresh();

                            if (_abortFlag == true)
                            {
                                return true;
                            }

                            if (this.checkBoxExportDWF.Checked == true)
                            {
                                var argsheetsize = Util.GetSheetsize(sheet, App.RevitDocument);

                                _exportDWFService.ExportDWF($"{fileName}.dwf", 
                                    argsheetsize, 
                                    _printSetup,
                                    _dwfExportOptions, 
                                    App.RevitDocument, 
                                    views);

                                //TODO - actually check if the export worked OK
                                this.labelTask.Text = "Exported DWF";

                                // to allow cancel button working
                                Application.DoEvents();
                            }

                            this.progressBarTask.PerformStep();
                            Refresh();

                            if (_abortFlag == true)
                            {
                                return true;
                            }

                            this.progressBarTask.PerformStep();
                            Refresh();

                            if (this.checkBoxRecordIssue.Checked == true)
                            {
                                // Mark sheets issued date.
                                SetIssueDate(sheet);

                                // Mark revisions issued
                                SetRevisionsIssued(sheet);
                            }

                            this.progressBarTask.PerformStep();
                            Refresh();

                            this.progressBarDrawingSheets.PerformStep();
                            Refresh();

                            // to allow cancel button working
                            Application.DoEvents();
                        }
                    }
                }
            }
            return false;
        }
        catch(Exception ex)
        {
            TaskDialog.Show("Error", $"There has been an error processing sheet exports. {Environment.NewLine} {ex}", TaskDialogCommonButtons.Ok);
            return true;
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

    private void CopyForExtranet()
    {
        // TODO make clone of issued files with <rev> omitted from file names
    }

#endregion

#region Transmittal Formats
    private void checkBoxExportPDF_CheckedChanged(object sender, EventArgs e)
    {
        this.groupBoxPDF.Enabled = this.checkBoxExportPDF.Checked;
        this.wizardControlPageFormats.NextEnabled = FormatNextEnabled();
    }

    private void checkBoxExportDWF_CheckedChanged(object sender, EventArgs e)
    {
        this.groupBoxDWF.Enabled = this.checkBoxExportDWF.Checked;
        this.wizardControlPageFormats.NextEnabled = FormatNextEnabled();
    }

    private void checkBoxExportDWG_CheckedChanged(object sender, EventArgs e)
    {
        this.groupBoxDWG.Enabled = this.checkBoxExportDWG.Checked;
        this.wizardControlPageFormats.NextEnabled = FormatNextEnabled();
    }

    private bool FormatNextEnabled()
    {
        if (this.checkBoxExportPDF.Checked)
        {
            return true;
        }
        if (this.checkBoxExportDWF.Checked)
        {
            return true;
        }
        if (this.checkBoxExportDWG.Checked)
        {
            return true;
        }

        return false;
    }

    private void SetExportFormatsList()
    {
        _exportFormats.Clear();

        if (this.checkBoxExportPDF.Checked)
        {
            _exportFormats.Add(Enums.ExportFormatType.PDF.ToString());
        }
        if (this.checkBoxExportDWF.Checked)
        {
            _exportFormats.Add(Enums.ExportFormatType.DWF.ToString());
        }
        if (this.checkBoxExportDWG.Checked)
        {
            _exportFormats.Add(Enums.ExportFormatType.DWG.ToString());
        }
    }

    private void SetExportOptions()
    {

        _printManager.PrintToFile = true;

        _printSetup = _printManager.PrintSetup;
        _printSetup.CurrentPrintSetting.PrintParameters.ZoomType = ZoomType.Zoom;
        _printSetup.CurrentPrintSetting.PrintParameters.Zoom = 100;
        _printSetup.CurrentPrintSetting.PrintParameters.PaperPlacement = PaperPlacementType.Center;
        _printSetup.CurrentPrintSetting.PrintParameters.ColorDepth = (ColorDepthType)this.comboBoxDWFColors.SelectedItem;
        _printSetup.CurrentPrintSetting.PrintParameters.ViewLinksinBlue = chkDWFViewLinksInBlue.Checked;
        _printSetup.CurrentPrintSetting.PrintParameters.HideReforWorkPlanes = chkDWFHideRefWorkPlanes.Checked;
        _printSetup.CurrentPrintSetting.PrintParameters.HideUnreferencedViewTags = chkDWFHideUnreferenceViewTags.Checked;
        _printSetup.CurrentPrintSetting.PrintParameters.HideCropBoundaries = chkDWFHideCropBoundaries.Checked;
        _printSetup.CurrentPrintSetting.PrintParameters.HideScopeBoxes = chkDWFHideScopeBox.Checked;
        _printSetup.CurrentPrintSetting.PrintParameters.HiddenLineViews = HiddenLineViewsType.VectorProcessing;
        _printSetup.CurrentPrintSetting.PrintParameters.RasterQuality = (RasterQualityType)this.comboBoxDWFRasterQuality.SelectedItem;
        
        _pdfExportOptions = new()
        {
            ZoomType = ZoomType.Zoom,
            ZoomPercentage = 100,
            PaperPlacement = PaperPlacementType.Center,
            ColorDepth = (ColorDepthType)comboBoxPDFColors.SelectedItem,
            RasterQuality = (RasterQualityType)comboBoxPDFRasterQuality.SelectedItem,
            ViewLinksInBlue = chkPDFViewLinksInBlue.Checked,
            HideReferencePlane = chkPDFHideRefWorkPlanes.Checked,
            HideUnreferencedViewTags = chkPDFHideUnreferenceViewTags.Checked,
            HideCropBoundaries = chkPDFHideCropBoundaries.Checked,
            HideScopeBoxes = chkPDFHideScopeBox.Checked,
            ReplaceHalftoneWithThinLines = chkPDFHalftone.Checked,
            MaskCoincidentLines = chkPDFCoincidentLines.Checked,
            AlwaysUseRaster = false,
            Combine = true,
        };

        _dwfExportOptions = new()
        {
            ImageFormat = DWFImageFormat.Lossless,
            ExportingAreas = chkDWFExportAreas.Checked,
            ExportObjectData = chkDWFObjectData.Checked
        };
        if (rdoDWFDefault.Checked == true)
        {
            _dwfExportOptions.ImageQuality = DWFImageQuality.Default;
        }
        if (rdoDWFLow.Checked == true)
        {
            _dwfExportOptions.ImageQuality = DWFImageQuality.Low;
        }
        if (rdoDWFMedium.Checked == true)
        {
            _dwfExportOptions.ImageQuality = DWFImageQuality.Medium;
        }
        if (rdoDWFHigh.Checked == true)
        {
            _dwfExportOptions.ImageQuality = DWFImageQuality.High;
        }

        _dwgExportOptions = new()
        {
            HideUnreferenceViewTags = chkDWGHideUnreferenceViewTags.Checked,
            HideScopeBox = chkDWGHideScopeBox.Checked,
            MergedViews = chkDWGMergedViews.Checked,
            LayerMapping = this.comboBoxDWGLayers.SelectedValue.ToString()  // "BS1192"
        };
        if (chkDWGSharedCoords.Checked == true)
        {
            _dwgExportOptions.SharedCoords = true;
        }
        else
        {
            _dwgExportOptions.SharedCoords = false;
        }
        if (rdoR2013.Checked == true)
        {
            _dwgExportOptions.FileVersion = ACADVersion.R2013;
        }
        else if (rdoR2010.Checked == true)
        {
            _dwgExportOptions.FileVersion = ACADVersion.R2010;
        }
        else if (rdoR2007.Checked == true)
        {
            _dwgExportOptions.FileVersion = ACADVersion.R2007;
        }
        else
        {
            _dwgExportOptions.FileVersion = ACADVersion.Default;
        }

    }

    #endregion

#region Distribution    
    private void LoadDirectoryToGrid()
    {
        this.sfDataGridDirectory.Columns.Clear();
        this.sfDataGridDirectory.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListModel.CompanyName", HeaderText = "Company" });
        this.sfDataGridDirectory.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListContactModel.FullNameReversed", HeaderText = "Person" });

        this.sfDataGridDistribution.Columns.Clear();
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListModel.CompanyName", HeaderText = "Company" });
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "ApprovedListContactModel.FullNameReversed", HeaderText = "Person" });
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "TransCopies", HeaderText = "Copies", Width = 60 });
        this.sfDataGridDistribution.Columns.Add(new GridTextColumn() { MappingName = "TransFormat", HeaderText = "Format", Width = 60 });

        _projectDirectory = _contactDirectoryService.GetProjectDirectory();

        WireUpDistribitionLists();
    }

    private void sfDataGridDirectory_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
    {
        if (this.sfDataGridDirectory.SelectedItems.Count > 0)
        {
            this.buttonAdd.Enabled = true;
        }
        else
        {
            this.buttonAdd.Enabled = false;
        }
    }

    private void sfDataGridDistribution_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
    {
        if (this.sfDataGridDistribution.SelectedItems.Count > 0)
        {
            this.buttonRemove.Enabled = true;
        }
        else
        {
            this.buttonRemove.Enabled = false;
        }
    }

    private void checkBoxRecordIssue_CheckedChanged(object sender, EventArgs e)
    {
        if (this.checkBoxRecordIssue.Checked)
        {
            this.sfDataGridDirectory.Enabled = true;
            this.sfDataGridDistribution.Enabled = true;
            this.buttonAddContact.Enabled = true;
            this.numericUpDownCopies.Enabled = true;
            this.comboBoxFormat.Enabled = true;
            this.wizardControlPageDistribution.NextEnabled = DistributionNextEnabled();            
        }
        else
        {
            this.sfDataGridDirectory.Enabled = false;
            this.sfDataGridDistribution.Enabled = false;
            this.buttonAddContact.Enabled = false;
            this.numericUpDownCopies.Enabled = false;
            this.comboBoxFormat.Enabled = false;
            this.wizardControlPageDistribution.NextEnabled = DistributionNextEnabled();
        }
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {
        //foreach (ProjectDirectoryModel directoryContact in this.sfDataGridDirectory.SelectedItems)
        //{
        //    //ProjectDirectoryModel directoryContact = (ProjectDirectoryModel)item;

        //    if (directoryContact != null)
        //    {
        //        TransmittalDistributionModel distributionRecord = new()
        //        {
        //            ApprovedListContactID = directoryContact.ApprovedListContactModel.ApprovedListContactID,
        //            ApprovedListModel = directoryContact.ApprovedListModel,
        //            ApprovedListContactModel = directoryContact.ApprovedListContactModel,
        //            DirectoryID = directoryContact.DirectoryID,
        //            TransCopies = (int)this.numericUpDownCopies.Value,
        //            TransFormat = (string)this.comboBoxFormat.SelectedValue
        //        };

        //        _projectDirectory.Remove(directoryContact);
        //        _distribution.Add(distributionRecord);
        //    }
        //}

        //WireUpDistribitionLists();
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
        //foreach (TransmittalDistributionModel distributionRecord in this.sfDataGridDistribution.SelectedItems)
        //{
        //    //TransmittalDistributionModel distributionRecord = (TransmittalDistributionModel)item;

        //    if (distributionRecord != null)
        //    {
        //        ProjectDirectoryModel directoryContact = new()
        //        {
        //            ApprovedListModel = distributionRecord.ApprovedListModel,
        //            ApprovedListContactModel = distributionRecord.ApprovedListContactModel,
        //            DirectoryID = distributionRecord.DirectoryID
        //        };

        //        _distribution.Remove(distributionRecord);
        //        _projectDirectory.Add(directoryContact);
        //    }
        //}

        //WireUpDistribitionLists();
    }

    private void buttonAddContact_Click(object sender, EventArgs e)
    {
        FormProjectDirectoryNew frm = new(_projectDirectory);
        frm.ShowDialog(this);

        WireUpDistribitionLists();
    }
    
    private void WireUpDistribitionLists()
    {
        //this.sfDataGridDirectory.DataSource = null;
        //this.sfDataGridDirectory.DataSource = _projectDirectory.OrderBy(x => x.Company.CompanyName).ThenBy(x => x.Person.FullNameReversed).ToList<ProjectDirectoryModel>();

        //this.sfDataGridDistribution.DataSource = null;
        //this.sfDataGridDistribution.DataSource = _distribution.OrderBy(x => x.Company.CompanyName).ThenBy(x => x.Person.FullNameReversed).ToList<TransmittalDistributionModel>();

        this.wizardControlPageDistribution.NextEnabled = DistributionNextEnabled();
    }

    private bool DistributionNextEnabled()
    {
        if (_distribution.Count > 0)
        {
            return true;
        }
        
        if (this.checkBoxRecordIssue.Checked == false)
        {
            return true;
        }
        
        return false;
    }

    #endregion


}

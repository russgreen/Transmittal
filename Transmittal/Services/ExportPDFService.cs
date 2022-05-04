using Autodesk.Revit.DB;
using System.IO;
using Transmittal.Extensions;
using Transmittal.Library.Services;

namespace Transmittal.Services;
internal class ExportPDFService : IExportPDFService
{
    private readonly ISettingsService _settingsService;

    public ExportPDFService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }
    
    public void ExportPDF(string ExportFileName, Document exportDocument, ViewSheet sheet)
    {
        throw new NotImplementedException();
    }

    public void ExportPDF(string exportFileName, Document exportDocument, ViewSet views, PDFExportOptions pdfExportOptions, bool RecordError = true)
    {
#if REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021
        //we've not using this method 
#else
        Transaction trans = null;
        try
        {
            trans = new Transaction(App.RevitDocument, "Export PDF");
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);
            trans.Start();
            
            // configure filename path for final PDF save location
            string fullPath = Path.Combine(_settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.PDF.ToString()), 
                exportFileName);
            
            if (File.Exists(fullPath) == true)
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception)
                {
                    fullPath = Path.Combine(_settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.PDF.ToString()), 
                        exportFileName.Replace(".pdf", $"({DateTime.Now.ToLongTimeString().Replace(":", "")}).pdf"));
                }
            }

            // get the titleblock instances
            var titleBlocks = new FilteredElementCollector(exportDocument);
            titleBlocks.OfCategory(BuiltInCategory.OST_TitleBlocks);
            titleBlocks.OfClass(typeof(FamilyInstance));
            
            // get the sizes with from the titleblock instances
            var Width = default(double);
            var Height = default(double);
            var viewIDs = new List<ElementId>();
            foreach (ViewSheet sheet in views)
            {
                viewIDs.Add(sheet.Id);
                foreach (FamilyInstance FI in titleBlocks)
                {
                    var _p = FI.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                    if ((_p.AsString() ?? "") == (sheet.SheetNumber ?? ""))
                    {
                        // we have the tb instance
                        _p = FI.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                        Width = _p.AsDouble().FootToMm();
                        _p = FI.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                        Height = _p.AsDouble().FootToMm();
                    }
                }
            }

            pdfExportOptions.FileName = exportFileName;

            if (Width > Height)
            {
                pdfExportOptions.PaperOrientation = PageOrientationType.Landscape;
            }
            else
            {
                pdfExportOptions.PaperOrientation = PageOrientationType.Portrait;
            }

            App.RevitDocument.Export(_settingsService.GlobalSettings.DrawingIssueStore, viewIDs, pdfExportOptions);
        }
        
        catch (Exception ex)
        {
            //TODO - report crashes
        }
        finally
        {
            trans.RollBack();
        }
#endif
    }
}

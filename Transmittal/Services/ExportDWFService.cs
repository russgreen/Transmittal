using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Extensions;
using Transmittal.Library.Services;

namespace Transmittal.Services;
internal class ExportDWFService : IExportDWFService
{
    private readonly ISettingsService _settingsService;

    public ExportDWFService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }    
    
    public void ExportDWF(string exportFileName, ExportPaperFormat sheetsize, PrintSetup printSetup, DWFExportOptions dwfExportOptions, Document exportDocument, ViewSet views)
    {
        Transaction trans = null;
        try
        {
            trans = new Transaction(exportDocument, "Export DWF");
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);

            string folderPath = _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(Enums.ExportFormatType.DWF.ToString());
            string fullPath = Path.Combine(folderPath, exportFileName);
            
            if (File.Exists(fullPath) == true)
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception)
                {
                    exportFileName.Replace(".dwf", $"({DateTime.Now.ToLongTimeString().Replace(":", "")}).dwf");
                }
            }

            trans.Start();

            dwfExportOptions.PaperFormat = sheetsize;

            exportDocument.Export(folderPath, exportFileName, views, dwfExportOptions);

        }
        catch (Exception ex)
        {
            //TODO - report crashes
        }
        finally
        {
            trans.RollBack();
        }
    }
}

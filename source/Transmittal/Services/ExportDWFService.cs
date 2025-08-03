using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Extensions;
using Transmittal.Library.Services;
using Transmittal.Library.Extensions;
using Microsoft.Extensions.Logging;

namespace Transmittal.Services;
internal class ExportDWFService : IExportDWFService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ExportDWFService> _logger;

    public ExportDWFService(ISettingsService settingsService,
        ILogger<ExportDWFService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }    
    
    public string ExportDWF(string exportFileName, string folderPath, ExportPaperFormat sheetsize, PrintSetup printSetup, DWFExportOptions dwfExportOptions, Document exportDocument, ViewSet views)
    {
        var fullPath = string.Empty;

        Transaction trans = null;
        try
        {
            trans = new Transaction(exportDocument, "Export DWF");
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(new WarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);

            fullPath = Path.Combine(folderPath, exportFileName);

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(fullPath) == true)
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting existing DWF");
                    exportFileName.Replace(".dwf", $"({DateTime.Now.ToLongTimeString().Replace(":", "")}).dwf");
                    fullPath = Path.Combine(folderPath, exportFileName);
                }
            }

            trans.Start();

            dwfExportOptions.PaperFormat = sheetsize;

            exportDocument.Export(folderPath, exportFileName, views, dwfExportOptions);

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error exporting DWF");
        }
        finally
        {
            trans.RollBack();
        }

        return fullPath;
    }
}

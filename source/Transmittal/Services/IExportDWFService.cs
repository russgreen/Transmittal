using Autodesk.Revit.DB;

namespace Transmittal.Services;
internal interface IExportDWFService
{
    /// <summary>
    /// DWF export
    /// </summary>
    /// <param name="exportFileName"></param>
    /// <param name="folderPath"></param>
    /// <param name="sheetsize"></param>
    /// <param name="printSetup"></param>
    /// <param name="dwfExportOptions"></param>
    /// <param name="exportDocument"></param>
    /// <param name="views"></param>
    /// <returns>full path to the exported file</returns>
    string ExportDWF(string exportFileName, string folderPath, ExportPaperFormat sheetsize, PrintSetup printSetup, DWFExportOptions dwfExportOptions, Document exportDocument, ViewSet views);
}

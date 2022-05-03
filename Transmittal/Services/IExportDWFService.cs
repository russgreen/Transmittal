using Autodesk.Revit.DB;

namespace Transmittal.Services;
internal interface IExportDWFService
{
    void ExportDWF(string exportFileName, ExportPaperFormat sheetsize, PrintSetup printSetup, DWFExportOptions dwfExportOptions, Document exportDocument, ViewSet views);
}

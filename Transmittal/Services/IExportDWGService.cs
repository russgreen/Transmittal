using Autodesk.Revit.DB;

namespace Transmittal.Services;
internal interface IExportDWGService
{
    void ExportDWG(string exportFileName, DWGExportOptions dwgExportOptions, ViewSet views, Document exportDocument);
}

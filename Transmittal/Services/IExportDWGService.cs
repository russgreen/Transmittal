using Autodesk.Revit.DB;
using Transmittal.Models;

namespace Transmittal.Services;
internal interface IExportDWGService
{
    void ExportDWG(string exportFileName, DWGExportOptions dwgExportOptions, ViewSet views, Document exportDocument);

    List<DWGLayerMappingModel> GetDWGLayerMappings();
}

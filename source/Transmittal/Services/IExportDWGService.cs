using Autodesk.Revit.DB;
using Transmittal.Models;

namespace Transmittal.Services;
internal interface IExportDWGService
{
    /// <summary>
    /// DWG export
    /// </summary>
    /// <param name="exportFileName"></param>
    /// <param name="folderPath"></param>
    /// <param name="dwgExportOptions"></param>
    /// <param name="views"></param>
    /// <param name="exportDocument"></param>
    /// <returns>full path to the exported file</returns>
    string ExportDWG(string exportFileName, string folderPath, DWGExportOptions dwgExportOptions, ViewSet views, Document exportDocument);

    DWGExportOptions GetDocumentDWGExportOptions(Document exportDocument);

    void SaveDocumentDWGExportOptions(Document exportDocument, DWGExportOptions dwgExportOptions);

    List<DWGLayerMappingModel> GetDWGLayerMappings();
}

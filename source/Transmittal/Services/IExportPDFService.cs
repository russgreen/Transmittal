using Autodesk.Revit.DB;

namespace Transmittal.Services;
internal interface IExportPDFService
{
#if REVIT2022_OR_GREATER
    /// <summary>
    /// PDF exported using the Revit 2022+ method
    /// </summary>
    /// <param name="exportFileName"></param>
    /// <param name="folderPath"></param>
    /// <param name="exportDocument"></param>
    /// <param name="views"></param>
    /// <param name="pdfExportOptions"></param>
    /// <param name="RecordError"></param>
    /// <returns>full path to the exported file</returns>
    string ExportPDF(string exportFileName, string folderPath, Document exportDocument, ViewSet views, PDFExportOptions pdfExportOptions, bool RecordError = true);
#endif

    /// <summary>
    /// PDF printed using PDF Printer   
    /// </summary>
    /// <param name="exportFileName"></param>
    /// <param name="folderPath"></param>
    /// <param name="exportDocument"></param>
    /// <param name="views"></param>
    /// <param name="pdfExportOptions"></param>
    /// <param name="RecordError"></param>
    /// <returns>full path to the printed file</returns>
    string PrintPDF(string exportFileName, string folderPath, Document exportDocument, ViewSet views, PDFExportOptions pdfExportOptions, bool RecordError = true);
}

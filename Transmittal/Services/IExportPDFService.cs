﻿using Autodesk.Revit.DB;

namespace Transmittal.Services;
internal interface IExportPDFService
{

    /// <summary>
    /// PDF exported using the Revit 2022+ method
    /// </summary>
    /// <param name="exportFileName"></param>
    /// <param name="exportDocument"></param>
    /// <param name="views"></param>
    /// <param name="pdfExportOptions"></param>
    /// <param name="RecordError"></param>
    /// <returns>full path to the exported file</returns>
    string ExportPDF(string exportFileName, Document exportDocument, ViewSet views, PDFExportOptions pdfExportOptions, bool RecordError = true);

}

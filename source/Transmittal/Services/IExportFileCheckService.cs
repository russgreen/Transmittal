using Transmittal.Models;

namespace Transmittal.Services;

internal interface IExportFileCheckService
{
    Task<IReadOnlyList<ExportFileCheckResult>> CheckExportFilesAsync(
        IReadOnlyCollection<DrawingSheetModel> sheets,
        bool enablePerSheetExportFormats,
        bool exportPDF,
        bool exportDWG,
        bool exportDWF,
        CancellationToken cancellationToken = default);
}

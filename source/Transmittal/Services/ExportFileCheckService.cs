using Transmittal.Enums;
using Transmittal.Library.Extensions;
using Transmittal.Library.Services;
using Transmittal.Models;
using System.IO;

namespace Transmittal.Services;

internal class ExportFileCheckService : IExportFileCheckService
{
    private readonly ISettingsService _settingsService;

    public ExportFileCheckService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public Task<IReadOnlyList<ExportFileCheckResult>> CheckExportFilesAsync(
        IReadOnlyCollection<DrawingSheetModel> sheets,
        bool enablePerSheetExportFormats,
        bool exportPDF,
        bool exportDWG,
        bool exportDWF,
        CancellationToken cancellationToken = default)
    {
        if (sheets.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<ExportFileCheckResult>>(Array.Empty<ExportFileCheckResult>());
        }

        return Task.Run<IReadOnlyList<ExportFileCheckResult>>(() =>
        {
            var results = new List<ExportFileCheckResult>();

            foreach (var sheet in sheets)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return results;
                }

                var exportPdf = enablePerSheetExportFormats ? sheet.ExportPDF == true : exportPDF;
                var exportDwg = enablePerSheetExportFormats ? sheet.ExportDWG == true : exportDWG;
                var exportDwf = enablePerSheetExportFormats ? sheet.ExportDWF == true : exportDWF;

                if (!exportPdf && !exportDwg && !exportDwf)
                {
                    continue;
                }

                var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(
                    _settingsService.GlobalSettings.ProjectNumber,
                    _settingsService.GlobalSettings.ProjectIdentifier,
                    _settingsService.GlobalSettings.ProjectName,
                    sheet.DrgOriginator,
                    sheet.DrgVolume,
                    sheet.DrgLevel,
                    sheet.DrgType,
                    sheet.DrgRole,
                    sheet.DrgNumber,
                    sheet.DrgName,
                    sheet.DrgRev,
                    sheet.DrgStatus,
                    sheet.DrgStatusDescription);

                if (exportPdf)
                {
                    results.Add(CreateResult(sheet, ExportFormatType.PDF, fileName, ".pdf"));
                }

                if (exportDwg)
                {
                    results.Add(CreateResult(sheet, ExportFormatType.DWG, fileName, ".dwg"));
                }

                if (exportDwf)
                {
                    results.Add(CreateResult(sheet, ExportFormatType.DWF, fileName, ".dwf"));
                }
            }

            return results;
        });
    }

    private ExportFileCheckResult CreateResult(DrawingSheetModel sheet, ExportFormatType exportFormatType, string fileName, string extension)
    {
        var folderPath = GetFolderPath(exportFormatType, sheet);
        var outputPath = Path.Combine(folderPath, $"{fileName}{extension}");

        return new ExportFileCheckResult
        {
            SheetId = sheet.ID,
            SheetNumber = sheet.DrgNumber ?? string.Empty,
            ExportFormat = exportFormatType,
            OutputPath = outputPath,
            FileExists = File.Exists(outputPath)
        };
    }

    private string GetFolderPath(ExportFormatType exportFormatType, DrawingSheetModel sheet)
    {
#if REVIT2025_OR_GREATER
        return _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(exportFormatType.ToString(), sheet.DrgPackage, sheet.DrgSheetCollection);
#else
        return _settingsService.GlobalSettings.DrawingIssueStore.ParseFolderName(exportFormatType.ToString(), sheet.DrgPackage);
#endif
    }
}

using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Reports.OpenXML.Models;

namespace Transmittal.Reports.OpenXML;

public class Reports
{
    private static readonly Regex TokenRegex = new("{{\\s*(?<name>[^{}]+?)\\s*}}", RegexOptions.Compiled);

    private readonly ISettingsService _settingsService;
    private readonly IContactDirectoryService _contactDirectoryService;
    private readonly ITransmittalService _transmittalService;

    public Reports(IServiceProvider serviceProvider, string reportStore)
    {
        _settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        _contactDirectoryService = serviceProvider.GetRequiredService<IContactDirectoryService>();
        _transmittalService = serviceProvider.GetRequiredService<ITransmittalService>();
    }

    public Reports(ISettingsService settingsService,
        IContactDirectoryService contactDirectoryService,
        ITransmittalService transmittalService)
    {
        _settingsService = settingsService;
        _contactDirectoryService = contactDirectoryService;
        _transmittalService = transmittalService;
    }

    public void ShowProjectDirectoryReport(List<ProjectDirectoryModel> projectDirectory)
    {
        var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
            _settingsService.GlobalSettings.ProjectIdentifier,
            _settingsService.GlobalSettings.ProjectName,
            _settingsService.GlobalSettings.Originator,
            "ZZ",
            "XX",
            "DY",
            _settingsService.GlobalSettings.Role,
            "0001",
            "ProjectDirectory",
            null, null, null);

        var folderPath = _settingsService.GlobalSettings.DirectoryStore.ParsePathWithEnvironmentVariables();
        var workbook = TryLoadTemplate("ProjectDirectory.xlsx") ?? new XLWorkbook();
        var worksheet = GetOrCreateWorksheet(workbook, "Project Directory");

        var commonContext = BuildCommonTokenContext("Project Directory", null);
        ApplyTemplateTokens(workbook, commonContext);
        ApplyCommonHeaderHeuristics(worksheet, "Project Directory", null);

        var templateRange = TryGetNamedRange(workbook, "ProjectDirectoryData", "ProjectDirectory", "ProjectDirectoryRange");

        var filtered = projectDirectory
            .Where(x => x.Person?.ShowInReport == true)
            .OrderBy(x => x.Person.LastName)
            .ThenBy(x => x.Person.FirstName)
            .ToList();

        if (templateRange != null)
        {
            PopulateRowsFromNamedRange(worksheet,
                templateRange,
                filtered,
                model => MergeContexts(commonContext, BuildProjectDirectoryContext(model)));
        }


        //worksheet.Columns().AdjustToContents();
        SaveAndOpen(workbook, folderPath, fileName);
    }

    public void ShowTransmittalReport(int transmittalID)
    {
        var transmittal = _transmittalService.GetTransmittal(transmittalID);

        var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
            _settingsService.GlobalSettings.ProjectIdentifier,
            _settingsService.GlobalSettings.ProjectName,
            _settingsService.GlobalSettings.Originator,
            "ZZ",
            "XX",
            "TL",
            _settingsService.GlobalSettings.Role,
            transmittal.ID.ToString().PadLeft(4, '0'),
            "TransmittalRecord",
            null, null, null);

        var folderPath = _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables();
        var workbook = TryLoadTemplate("TransmittalSheet.xlsx") ?? new XLWorkbook();
        var worksheet = GetOrCreateWorksheet(workbook, "Transmittal");

        var commonContext = BuildCommonTokenContext("Transmittal Record", transmittal.TransDate);
        ApplyTemplateTokens(workbook, commonContext);
        ApplyCommonHeaderHeuristics(worksheet, "Transmittal Record", transmittal.TransDate);

        var sheetsRange = TryGetNamedRange(workbook, "SheetListData");
        var distributionRange = TryGetNamedRange(workbook, "DistributionListData");

        if (sheetsRange != null && distributionRange != null)
        {
            var templateOrderedItems = transmittal.Items.OrderBy(x => x.DrgNumber).ToList();
            PopulateRowsFromNamedRange(worksheet,
                sheetsRange,
                templateOrderedItems,
                item => MergeContexts(commonContext, BuildTransmittalItemContext(item, transmittal)));

            var distributionRows = transmittal.Distribution
                .Select(dist =>
                {
                    var person = _contactDirectoryService.GetPerson(dist.PersonID);
                    var company = person == null ? null : _contactDirectoryService.GetCompany(person.CompanyID);
                    return new DistributionTemplateRow { Distribution = dist, Person = person, Company = company };
                })
                .ToList();

            PopulateRowsFromNamedRange(worksheet,
                distributionRange,
                distributionRows,
                row => MergeContexts(commonContext, BuildDistributionContext(row.Distribution, row.Person, row.Company, transmittal.ID)));
     }


        //worksheet.Columns().AdjustToContents();
        SaveAndOpen(workbook, folderPath, fileName);
    }

    public void ShowTransmittalSummaryReport(List<TransmittalModel> transmittals = null)
    {
        var orderedTransmittals = (transmittals ?? _transmittalService.GetTransmittals())
            .OrderBy(x => x.TransDate)
            .ThenBy(x => x.ID)
            .ToList();

        var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
            _settingsService.GlobalSettings.ProjectIdentifier,
            _settingsService.GlobalSettings.ProjectName,
            _settingsService.GlobalSettings.Originator,
            "ZZ",
            "XX",
            "MX",
            _settingsService.GlobalSettings.Role,
            "0001",
            "TransmittalSummary",
            null, null, null);

        var folderPath = _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables();
        var workbook = TryLoadTemplate("TransmittalSummary.xlsx") ?? new XLWorkbook();
        var worksheet = GetOrCreateWorksheet(workbook, "Summary");

        var commonContext = BuildCommonTokenContext("Transmittal Summary", null);
        ApplyTemplateTokens(workbook, commonContext, new[] { "DateYear", "DateMonth", "DateDay" });
        ApplyCommonHeaderHeuristics(worksheet, "Transmittal Summary", null);

        var sheetListAnchor = TryGetNamedRange(workbook, "SheetListData");
        var distributionListAnchor = TryGetNamedRange(workbook, "DistributionListData");
        var summaryColumnAnchor = TryGetNamedRange(workbook, "SummaryColumnData");
        var transmittalFormatAnchor = TryGetNamedRange(workbook, "TransmittalFormatData");

        if (sheetListAnchor == null || distributionListAnchor == null || summaryColumnAnchor == null)
        {
            SaveAndOpen(workbook, folderPath, fileName);
            return;
        }

        var dateRows = FindDateRowsByTokens(worksheet);
        if (dateRows.DayRow == 0)
        {
            SaveAndOpen(workbook, folderPath, fileName);
            return;
        }

        var orderedColumns = orderedTransmittals
            .Select((t, idx) => new SummaryColumn
            {
                Index = idx,
                TransmittalId = t.ID,
                Date = t.TransDate
            })
            .ToList();

        var summaryColumnNumbers = EnsureSummaryColumns(worksheet, summaryColumnAnchor, orderedColumns.Count);

        ApplySummaryDateRows(worksheet, dateRows, orderedColumns, summaryColumnNumbers);

        if (transmittalFormatAnchor != null)
        {
            ApplyTransmittalFormatRow(worksheet, transmittalFormatAnchor, orderedTransmittals, summaryColumnNumbers);
        }

        var docRows = BuildSummaryItemRows(orderedTransmittals, commonContext);
        WriteSummaryDocumentMatrix(worksheet, sheetListAnchor, docRows, orderedColumns, summaryColumnNumbers);

        var recipientRows = BuildSummaryDistributionRows(orderedTransmittals, commonContext);
        WriteSummaryDistributionMatrix(worksheet, distributionListAnchor, recipientRows, orderedColumns, summaryColumnNumbers);

        SaveAndOpen(workbook, folderPath, fileName);
    }

    public void ShowMasterDocumentsListReport()
    {
        var transmittals = _transmittalService.GetTransmittals();

        // Group all items by DrgVolume and DrgNumber to capture every unique document
        var allItems = transmittals
            .SelectMany(t => t.Items.Select(item => new { Transmittal = t, Item = item }))
            .GroupBy(x => new { x.Item.DrgVolume, x.Item.DrgNumber })
            .Select(g => new
            {
                Item = g.OrderByDescending(x => x.Transmittal.TransDate)
                         .ThenByDescending(x => x.Transmittal.ID)
                         .First().Item,
                Transmittal = g.OrderByDescending(x => x.Transmittal.TransDate)
                               .ThenByDescending(x => x.Transmittal.ID)
                               .First().Transmittal,
                LatestRevision = g.OrderByDescending(x => x.Transmittal.TransDate)
                                  .ThenByDescending(x => x.Transmittal.ID)
                                  .First().Item.DrgRev
            })
            .OrderBy(x => x.Item.DrgVolume)
            .ThenBy(x => x.Item.DrgNumber)
            .ToList();

        var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(
            _settingsService.GlobalSettings.ProjectNumber,
            _settingsService.GlobalSettings.ProjectIdentifier,
            _settingsService.GlobalSettings.ProjectName,
            _settingsService.GlobalSettings.Originator,
            "ZZ",
            "XX",
            "MX",
            _settingsService.GlobalSettings.Role,
            "0002",
            "MasterDocumentsList",
            null, null, null);

        var folderPath = _settingsService.GlobalSettings.IssueSheetStore.ParsePathWithEnvironmentVariables();
        var workbook = TryLoadTemplate("MasterDocumentsList.xlsx") ?? new XLWorkbook();
        var worksheet = GetOrCreateWorksheet(workbook, "Master Documents");

        var commonContext = BuildCommonTokenContext("Master Documents List", null);
        ApplyTemplateTokens(workbook, commonContext);
        ApplyCommonHeaderHeuristics(worksheet, "Master Documents List", null);

        var templateRange = TryGetNamedRange(workbook, "MasterDocumentListData", "MasterDocumentsList", "DocumentListRange");

        if (templateRange != null)
        {
            PopulateRowsFromNamedRange(worksheet,
                templateRange,
                allItems,
                item => MergeContexts(commonContext, BuildMasterDocumentContext(item.Item, item.Transmittal)));
        }

        SaveAndOpen(workbook, folderPath, fileName);
    }



    private void ApplyTransmittalFormatRow(IXLWorksheet worksheet, IXLRange formatRange, List<TransmittalModel> transmittals, List<int> dateColumns)
    {
        if (formatRange == null || dateColumns.Count == 0)
        {
            return;
        }

        var formatRow = formatRange.RangeAddress.FirstAddress.RowNumber;
        var ordered = transmittals.OrderBy(t => t.TransDate).ThenBy(t => t.ID).ToList();
        var count = Math.Min(dateColumns.Count, ordered.Count);

        for (var i = 0; i < count; i++)
        {
            var format = ordered[i].Distribution
                .GroupBy(d => d.TransFormat)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(format))
            {
                worksheet.Cell(formatRow, dateColumns[i]).Value = format;
            }
        }
    }

    private void ApplySummaryDateRows(IXLWorksheet worksheet, DateRows dateRows, List<SummaryColumn> columns, List<int> dateColumns)
    {
        if (dateRows.DayRow == 0 || dateColumns.Count == 0)
        {
            return;
        }

        var count = Math.Min(dateColumns.Count, columns.Count);

        for (var i = 0; i < count; i++)
        {
            var date = columns[i].Date;
            worksheet.Cell(dateRows.YearRow, dateColumns[i]).Value = date.Year % 100;
            worksheet.Cell(dateRows.MonthRow, dateColumns[i]).Value = date.Month;
            worksheet.Cell(dateRows.DayRow, dateColumns[i]).Value = date.Day;
        }
    }

    private int WriteSummaryDocumentMatrix(IXLWorksheet worksheet, IXLRange dataRange, List<SummaryItemRow> rows, List<SummaryColumn> columns, List<int> dateColumns)
    {
        if (dataRange == null || dateColumns.Count == 0 || rows.Count == 0)
        {
            return 0;
        }

        var template = CaptureTemplateRow(worksheet, dataRange);
        var targetRow = template.RowNumber;
        var insertedRows = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            if (i > 0)
            {
                worksheet.Row(targetRow + 1).InsertRowsAbove(1);
                targetRow++;
                insertedRows++;
                ResetTemplateRow(worksheet, targetRow, template);
            }

            var item = rows[i];
            RenderTemplateRow(worksheet, targetRow, template, item.RowContext ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            var columnCount = Math.Min(dateColumns.Count, columns.Count);
            for (var c = 0; c < columnCount; c++)
            {
                var targetCell = worksheet.Cell(targetRow, dateColumns[c]);
                targetCell.Style = worksheet.Cell(template.RowNumber, dateColumns[c]).Style;
                targetCell.Clear(XLClearOptions.Contents);

                if (item.RevisionsByTransmittal.TryGetValue(columns[c].TransmittalId, out var rev))
                {
                    targetCell.Value = rev;
                }
            }
        }

        return insertedRows;
    }

    private void WriteSummaryDistributionMatrix(IXLWorksheet worksheet, IXLRange dataRange, List<SummaryDistributionRow> rows, List<SummaryColumn> columns, List<int> dateColumns)
    {
        if (dataRange == null || dateColumns.Count == 0 || rows.Count == 0)
        {
            return;
        }

        var template = CaptureTemplateRow(worksheet, dataRange);
        var targetRow = template.RowNumber;

        for (var i = 0; i < rows.Count; i++)
        {
            if (i > 0)
            {
                worksheet.Row(targetRow + 1).InsertRowsAbove(1);
                targetRow++;
                ResetTemplateRow(worksheet, targetRow, template);
            }

            var row = rows[i];
            RenderTemplateRow(worksheet, targetRow, template, row.RowContext ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            var columnCount = Math.Min(dateColumns.Count, columns.Count);
            for (var c = 0; c < columnCount; c++)
            {
                var targetCell = worksheet.Cell(targetRow, dateColumns[c]);
                targetCell.Style = worksheet.Cell(template.RowNumber, dateColumns[c]).Style;
                targetCell.Clear(XLClearOptions.Contents);

                if (!row.FormatByTransmittal.TryGetValue(columns[c].TransmittalId, out var cell))
                {
                    continue;
                }

                if (targetCell.DataType == XLDataType.Number || targetCell.Style.NumberFormat.Format == "0" || targetCell.Style.NumberFormat.NumberFormatId > 0)
                {
                    targetCell.Value = cell.Copies;
                }
                else
                {
                    targetCell.Value = cell.Copies > 0 ? $"{cell.Copies}" : string.Empty;
                }
            }
        }
    }

    private XLWorkbook TryLoadTemplate(string templateName)
    {
        var reportStore = _settingsService.GlobalSettings.ReportStore;

        if (string.IsNullOrWhiteSpace(reportStore))
        {
            return null;
        }

        var reportStorePath = reportStore.ParsePathWithEnvironmentVariables();
        var templatePath = Path.Combine(reportStorePath, templateName);

        if (!File.Exists(templatePath))
        {
            return null;
        }

        return new XLWorkbook(templatePath);
    }

    private void ApplyTemplateTokens(
        XLWorkbook workbook,
        Dictionary<string, string> context,
        IEnumerable<string> excludedTokens = null)
    {
        var excluded = excludedTokens == null
            ? null
            : new HashSet<string>(excludedTokens, StringComparer.OrdinalIgnoreCase);

        foreach (var worksheet in workbook.Worksheets)
        {
            var used = worksheet.RangeUsed();
            if (used == null)
            {
                continue;
            }

            foreach (var cell in used.Cells())
            {
                if (cell.DataType != XLDataType.Text)
                {
                    continue;
                }

                var text = cell.GetString();
                if (string.IsNullOrWhiteSpace(text) || !text.Contains("{{", StringComparison.Ordinal))
                {
                    continue;
                }

                cell.Value = ReplaceTokens(text, context, excluded);
            }
        }
    }

    private Dictionary<string, string> BuildCommonTokenContext(string reportTitle, DateTime? transmittalDate)
    {
        var projectDisplay = BuildProjectDisplay();
        var now = DateTime.Now;
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Project"] = projectDisplay,
            ["ProjectDisplay"] = projectDisplay,
            ["ProjectName"] = _settingsService.GlobalSettings.ProjectName ?? string.Empty,
            ["ProjectNumber"] = _settingsService.GlobalSettings.ProjectNumber ?? string.Empty,
            ["ProjectIdentifier"] = _settingsService.GlobalSettings.ProjectIdentifier ?? string.Empty,
            ["ProjectID"] = _settingsService.GlobalSettings.ProjectIdentifier ?? string.Empty,
            ["ClientName"] = _settingsService.GlobalSettings.ClientName ?? string.Empty,
            ["ReportTitle"] = reportTitle,
            ["ReportDate"] = now.ToString("D"),
            ["TransmittalDate"] = transmittalDate?.ToString("D") ?? string.Empty,
            ["DateYear"] = (transmittalDate ?? now).Year.ToString(),
            ["DateMonth"] = (transmittalDate ?? now).Month.ToString(),
            ["DateDay"] = (transmittalDate ?? now).Day.ToString(),
        };
    }

    private static Dictionary<string, string> MergeContexts(
        Dictionary<string, string> commonContext,
        Dictionary<string, string> rowContext)
    {
        var merged = new Dictionary<string, string>(commonContext, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in rowContext)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }

    private Dictionary<string, string> BuildProjectDirectoryContext(ProjectDirectoryModel model)
    {
        var person = model.Person;
        var company = model.Company;

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CompanyName"] = company?.CompanyName ?? string.Empty,
            ["Role"] = person?.Position ?? company?.Role ?? string.Empty,
            ["Address"] = company?.Address ?? string.Empty,
            ["LastName"] = person?.LastName ?? string.Empty,
            ["FirstName"] = person?.FirstName ?? string.Empty,
            ["Tel"] = person?.Tel ?? company?.Tel ?? string.Empty,
            ["Fax"] = company?.Fax ?? string.Empty,
            ["Website"] = company?.Website ?? string.Empty,
            ["DDI"] = person?.Tel ?? string.Empty,
            ["Mobile"] = person?.Mobile ?? string.Empty,
            ["Email"] = person?.Email ?? string.Empty,
            ["Position"] = person?.Position ?? string.Empty,
            ["ContactName"] = person?.FullName ?? string.Empty,
            ["PersonName"] = person?.FullName ?? string.Empty,
        };
    }

    private Dictionary<string, string> BuildTransmittalItemContext(TransmittalItemModel item, TransmittalModel transmittal)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TransItemID"] = item.TransItemID.ToString(),
            ["DrgNumber"] = item.DrgNumber ?? string.Empty,
            ["DrgRev"] = item.DrgRev ?? string.Empty,
            ["DrgName"] = item.DrgName ?? string.Empty,
            ["DrgPaper"] = item.DrgPaper ?? string.Empty,
            ["DrgScale"] = item.DrgScale ?? string.Empty,
            ["DrgDrawn"] = item.DrgDrawn ?? string.Empty,
            ["DrgChecked"] = item.DrgChecked ?? string.Empty,
            ["TransID"] = transmittal.ID.ToString(),
            ["ProjectID"] = _settingsService.GlobalSettings.ProjectIdentifier ?? string.Empty,
            ["TransDate"] = transmittal.TransDate.ToShortDateString(),
            ["DateYear"] = transmittal.TransDate.Year.ToString(),
            ["DateMonth"] = transmittal.TransDate.Month.ToString(),
            ["DateDay"] = transmittal.TransDate.Day.ToString(),
            ["DrgProj"] = item.DrgProj ?? string.Empty,
            ["DrgOriginator"] = item.DrgOriginator ?? string.Empty,
            ["DrgVolume"] = item.DrgVolume ?? string.Empty,
            ["DrgLevel"] = item.DrgLevel ?? string.Empty,
            ["DrgType"] = item.DrgType ?? string.Empty,
            ["DrgRole"] = item.DrgRole ?? string.Empty,
            ["DrgStatus"] = item.DrgStatus ?? string.Empty,
            ["DrgPackage" ]= item.DrgPackage ?? string.Empty,
        };
    }

    private Dictionary<string, string> BuildDistributionContext(
        TransmittalDistributionModel distribution,
        PersonModel person,
        CompanyModel company,
        int transmittalId)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ApprovedListContactID"] = distribution.PersonID.ToString(),
            ["ContactName"] = person?.FullName ?? string.Empty,
            ["TransFormat"] = distribution.TransFormat ?? string.Empty,
            ["TransCopies"] = distribution.TransCopies.ToString(),
            ["TransID"] = transmittalId.ToString(),
            ["CompanyName"] = company?.CompanyName ?? string.Empty,
            ["PersonName"] = person?.FullName ?? string.Empty,
            ["FirstName"] = person?.FirstName ?? string.Empty,
            ["LastName"] = person?.LastName ?? string.Empty,
            ["Email"] = person?.Email ?? string.Empty,
            ["Tel"] = person?.Tel ?? string.Empty,
            ["Mobile"] = person?.Mobile ?? string.Empty,
            ["Role"] = person?.Position ?? company?.Role ?? string.Empty,
        };
    }

    private Dictionary<string, string> BuildMasterDocumentContext(TransmittalItemModel item, TransmittalModel transmittal)
    {
        return BuildTransmittalItemContext(item, transmittal);
    }

    private void PopulateRowsFromNamedRange<T>(
        IXLWorksheet worksheet,
        IXLRange templateRange,
        IReadOnlyList<T> rows,
        Func<T, Dictionary<string, string>> contextFactory)
    {
        var template = CaptureTemplateRow(worksheet, templateRange);

        if (rows.Count == 0)
        {
            ResetTemplateRow(worksheet, template.RowNumber, template);
            ClearTokenCells(worksheet, template.RowNumber, template);
            return;
        }

        var targetRow = template.RowNumber;

        for (var i = 0; i < rows.Count; i++)
        {
            if (i > 0)
            {
                worksheet.Row(targetRow + 1).InsertRowsAbove(1);
                targetRow++;
                ResetTemplateRow(worksheet, targetRow, template);
            }

            RenderTemplateRow(worksheet, targetRow, template, contextFactory(rows[i]));
        }
    }

    private static TemplateRow CaptureTemplateRow(IXLWorksheet worksheet, IXLRange templateRange)
    {
        var startRow = templateRange.RangeAddress.FirstAddress.RowNumber;
        var endRow = templateRange.RangeAddress.LastAddress.RowNumber;
        var rowNumber = startRow;
        var firstColumn = templateRange.RangeAddress.FirstAddress.ColumnNumber;
        var lastColumn = templateRange.RangeAddress.LastAddress.ColumnNumber;

        if (endRow > startRow)
        {
            var bestRow = startRow;
            var bestScore = -1;
            var worksheetLastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? lastColumn;

            for (var row = startRow; row <= endRow; row++)
            {
                var tokenCount = 0;
                var scanStartColumn = firstColumn;
                var scanEndColumn = lastColumn;

                if (firstColumn == lastColumn)
                {
                    scanStartColumn = 1;
                    scanEndColumn = worksheetLastColumn;
                }

                for (var col = scanStartColumn; col <= scanEndColumn; col++)
                {
                    var cellText = worksheet.Cell(row, col).GetString();
                    if (!string.IsNullOrWhiteSpace(cellText) && cellText.Contains("{{", StringComparison.Ordinal))
                    {
                        tokenCount++;
                    }
                }

                if (tokenCount > bestScore)
                {
                    bestScore = tokenCount;
                    bestRow = row;
                }
            }

            rowNumber = bestRow;
        }

        if (firstColumn == lastColumn)
        {
            var worksheetLastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? lastColumn;
            var tokenColumns = new List<int>();

            for (var col = 1; col <= worksheetLastColumn; col++)
            {
                var cellText = worksheet.Cell(rowNumber, col).GetString();
                if (!string.IsNullOrWhiteSpace(cellText) && cellText.Contains("{{", StringComparison.Ordinal))
                {
                    tokenColumns.Add(col);
                }
            }

            if (tokenColumns.Count > 0)
            {
                firstColumn = tokenColumns.Min();
                lastColumn = tokenColumns.Max();
            }
            else
            {
                var rowCells = worksheet.Row(rowNumber)
                    .CellsUsed(XLCellsUsedOptions.All)
                    .ToList();

                if (rowCells.Count > 0)
                {
                    firstColumn = rowCells.Min(c => c.Address.ColumnNumber);
                    lastColumn = rowCells.Max(c => c.Address.ColumnNumber);
                }
            }
        }

        var cells = new List<TemplateCell>();
        for (var col = firstColumn; col <= lastColumn; col++)
        {
            var cell = worksheet.Cell(rowNumber, col);
            cells.Add(new TemplateCell
            {
                Column = col,
                Style = cell.Style,
                HasFormula = !string.IsNullOrWhiteSpace(cell.FormulaA1),
                FormulaA1 = cell.FormulaA1,
                TextValue = cell.GetString(),
                Value = cell.Value,
                DataType = cell.DataType,
            });
        }

        return new TemplateRow
        {
            RowNumber = rowNumber,
            FirstColumn = firstColumn,
            LastColumn = lastColumn,
            RowStyle = worksheet.Row(rowNumber).Style,
            Cells = cells,
        };
    }

    private static void ResetTemplateRow(IXLWorksheet worksheet, int rowNumber, TemplateRow template)
    {
        worksheet.Row(rowNumber).Style = template.RowStyle;

        foreach (var templateCell in template.Cells)
        {
            var targetCell = worksheet.Cell(rowNumber, templateCell.Column);
            targetCell.Style = templateCell.Style;

            if (templateCell.HasFormula)
            {
                targetCell.FormulaA1 = templateCell.FormulaA1;
                continue;
            }

            targetCell.Value = templateCell.Value;
        }
    }

    private static void RenderTemplateRow(IXLWorksheet worksheet, int rowNumber, TemplateRow template, Dictionary<string, string> context)
    {
        foreach (var templateCell in template.Cells)
        {
            var targetCell = worksheet.Cell(rowNumber, templateCell.Column);

            if (templateCell.HasFormula)
            {
                targetCell.FormulaA1 = templateCell.FormulaA1;
                continue;
            }

            if (templateCell.DataType == XLDataType.Text &&
                !string.IsNullOrWhiteSpace(templateCell.TextValue) &&
                templateCell.TextValue.Contains("{{", StringComparison.Ordinal))
            {
                targetCell.Value = ReplaceTokens(templateCell.TextValue, context);
                continue;
            }

            targetCell.Value = templateCell.Value;
        }
    }

    private static void ClearTokenCells(IXLWorksheet worksheet, int rowNumber, TemplateRow template)
    {
        foreach (var templateCell in template.Cells)
        {
            if (templateCell.HasFormula)
            {
                continue;
            }

            if (templateCell.DataType == XLDataType.Text &&
                !string.IsNullOrWhiteSpace(templateCell.TextValue) &&
                templateCell.TextValue.Contains("{{", StringComparison.Ordinal))
            {
                worksheet.Cell(rowNumber, templateCell.Column).Value = string.Empty;
            }
        }
    }

    private static string ReplaceTokens(string input, Dictionary<string, string> context, HashSet<string> excludedTokens = null)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.Contains("{{", StringComparison.Ordinal))
        {
            return input;
        }

        return TokenRegex.Replace(input, match =>
        {
            var tokenName = match.Groups["name"].Value.Trim();

            if (excludedTokens?.Contains(tokenName) == true)
            {
                return match.Value;
            }

            if (context.TryGetValue(tokenName, out var value))
            {
                return value ?? string.Empty;
            }

            return match.Value;
        });
    }

    private void ApplyCommonHeaderHeuristics(IXLWorksheet worksheet, string reportTitle, DateTime? transmittalDate)
    {
        var used = worksheet.RangeUsed();
        if (used == null)
        {
            return;
        }

        var projectDisplay = BuildProjectDisplay();
        var reportDateText = $"Report date: {DateTime.Now:D}";
        var transmittalDateText = $"Transmittal date: {(transmittalDate?.ToString("D") ?? string.Empty)}";

        foreach (var cell in used.Cells())
        {
            if (cell.DataType != XLDataType.Text)
            {
                continue;
            }

            var text = cell.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (text.Contains("Report date:", StringComparison.OrdinalIgnoreCase))
            {
                text = Regex.Replace(text, @"Report date:[^\r\n]*", reportDateText, RegexOptions.IgnoreCase);
            }

            if (transmittalDate.HasValue && text.Contains("Transmittal date:", StringComparison.OrdinalIgnoreCase))
            {
                text = Regex.Replace(text, @"Transmittal date:[^\r\n]*", transmittalDateText, RegexOptions.IgnoreCase);
            }

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
            if (lines.Count >= 2)
            {
                if (Regex.IsMatch(lines[0], "Project Directory|Transmittal|Summary|TRANSMITTAL|PROJECT DIRECTORY", RegexOptions.IgnoreCase))
                {
                    lines[0] = reportTitle;
                    lines[1] = projectDisplay;
                    text = string.Join(Environment.NewLine, lines);
                }
            }

            cell.Value = text;
        }

        foreach (var labelCell in used.Cells().Where(c => c.DataType == XLDataType.Text))
        {
            var text = labelCell.GetString();
            if (!text.Contains("Project:", StringComparison.OrdinalIgnoreCase) ||
                !text.Contains("Project No", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var target = worksheet.Cell(labelCell.Address.RowNumber, labelCell.Address.ColumnNumber + 3);
            if (target != null)
            {
                target.Value = $"{_settingsService.GlobalSettings.ProjectName}{Environment.NewLine}{_settingsService.GlobalSettings.ProjectNumber}{Environment.NewLine}{_settingsService.GlobalSettings.ClientName}";
            }
        }
    }

    private List<SummaryItemRow> BuildSummaryItemRows(List<TransmittalModel> transmittals, Dictionary<string, string> commonContext)
    {
        var orderedTransmittals = transmittals
            .OrderBy(t => t.TransDate)
            .ThenBy(t => t.ID)
            .ToList();

        var transmittalById = orderedTransmittals.ToDictionary(t => t.ID);

        var dedupedItems = orderedTransmittals
            .OrderBy(t => t.TransDate)
            .ThenBy(t => t.ID)
            .SelectMany(t => t.Items.Select(item => new { t.ID, Item = item }))
            .GroupBy(x => new { x.ID, DrawingNumber = x.Item.DrgNumber })
            .Select(g => g.Last())
            .ToList();

        var rows = dedupedItems
            .GroupBy(x => new { x.Item.DrgVolume, x.Item.DrgNumber })
            .OrderBy(g => g.Key.DrgVolume)
            .ThenBy(g => g.Key.DrgNumber)
            .Select(g => new SummaryItemRow
            {
                RevisionsByTransmittal = g
                    .GroupBy(x => x.ID)
                    .ToDictionary(
                        tg => tg.Key,
                        tg => tg.Select(v => v.Item.DrgRev).LastOrDefault() ?? string.Empty),
                TemplateItem = g
                    .OrderByDescending(x => transmittalById[x.ID].TransDate)
                    .ThenByDescending(x => x.ID)
                    .Select(x => x.Item)
                    .FirstOrDefault(),
                TemplateTransmittal = g
                    .OrderByDescending(x => transmittalById[x.ID].TransDate)
                    .ThenByDescending(x => x.ID)
                    .Select(x => transmittalById[x.ID])
                    .FirstOrDefault(),
                RowContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            })
            .ToList();

        foreach (var row in rows)
        {
            if (row.TemplateItem != null && row.TemplateTransmittal != null)
            {
                row.RowContext = MergeContexts(commonContext, BuildTransmittalItemContext(row.TemplateItem, row.TemplateTransmittal));
            }
        }

        return rows;
    }

    private List<SummaryDistributionRow> BuildSummaryDistributionRows(List<TransmittalModel> transmittals, Dictionary<string, string> commonContext)
    {
        var orderedTransmittals = transmittals
            .OrderBy(t => t.TransDate)
            .ThenBy(t => t.ID)
            .ToList();

        var transmittalById = orderedTransmittals.ToDictionary(t => t.ID);

        var rows = transmittals
            .OrderBy(t => t.TransDate)
            .ThenBy(t => t.ID)
            .SelectMany(t => t.Distribution.Select(d => new { t.ID, Dist = d }))
            .GroupBy(x => x.Dist.PersonID)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var person = _contactDirectoryService.GetPerson(g.Key);
                var company = _contactDirectoryService.GetCompany(person.CompanyID);

                return new SummaryDistributionRow
                {
                    FormatByTransmittal = g.GroupBy(x => x.ID)
                        .ToDictionary(
                            x => x.Key,
                            x => new DistCell
                            {
                                Format = x.Select(v => v.Dist.TransFormat).FirstOrDefault() ?? string.Empty,
                                Copies = x.Sum(v => v.Dist.TransCopies)
                            }),
                    TemplateDistribution = g
                        .OrderByDescending(x => transmittalById[x.ID].TransDate)
                        .ThenByDescending(x => x.ID)
                        .Select(x => x.Dist)
                        .FirstOrDefault(),
                    TemplateTransmittal = g
                        .OrderByDescending(x => transmittalById[x.ID].TransDate)
                        .ThenByDescending(x => x.ID)
                        .Select(x => transmittalById[x.ID])
                        .FirstOrDefault(),
                    Person = person,
                    CompanyModel = company,
                    RowContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            })
            .ToList();

        foreach (var row in rows)
        {
            if (row.TemplateDistribution != null)
            {
                row.RowContext = MergeContexts(
                    commonContext,
                    BuildDistributionContext(
                        row.TemplateDistribution,
                        row.Person,
                        row.CompanyModel,
                        row.TemplateTransmittal?.ID ?? 0));
            }
            //else
            //{
            //    row.RowContext = new Dictionary<string, string>(commonContext, StringComparer.OrdinalIgnoreCase);
            //}
        }

        return rows;
    }

    private DateRows FindNearestDateRows(IXLWorksheet worksheet, int anchorRow)
    {
        var tokenRows = FindDateRowsByTokens(worksheet);
        if (tokenRows.DayRow > 0)
        {
            return tokenRows;
        }

        for (var row = anchorRow; row >= Math.Max(1, anchorRow - 40); row--)
        {
            var text = worksheet.Cell(row, 1).GetFormattedString().Trim();
            if (!text.Equals("Year", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var monthRow = row + 1;
            var dayRow = row + 2;
            if (worksheet.Cell(monthRow, 1).GetFormattedString().Equals("Month", StringComparison.OrdinalIgnoreCase) &&
                worksheet.Cell(dayRow, 1).GetFormattedString().Equals("Day", StringComparison.OrdinalIgnoreCase))
            {
                return new DateRows { YearRow = row, MonthRow = monthRow, DayRow = dayRow };
            }
        }

        return new DateRows();
    }

    private static DateRows FindDateRowsByTokens(IXLWorksheet worksheet)
    {
        var used = worksheet.RangeUsed();
        if (used == null)
        {
            return new DateRows();
        }

        var yearRow = 0;
        var monthRow = 0;
        var dayRow = 0;

        foreach (var cell in used.Cells())
        {
            if (cell.DataType != XLDataType.Text)
            {
                continue;
            }

            var text = cell.GetString();
            if (string.IsNullOrWhiteSpace(text) || !text.Contains("{{", StringComparison.Ordinal))
            {
                continue;
            }

            if (yearRow == 0 && ContainsToken(text, "DateYear"))
            {
                yearRow = cell.Address.RowNumber;
            }

            if (monthRow == 0 && ContainsToken(text, "DateMonth"))
            {
                monthRow = cell.Address.RowNumber;
            }

            if (dayRow == 0 && ContainsToken(text, "DateDay"))
            {
                dayRow = cell.Address.RowNumber;
            }

            if (yearRow > 0 && monthRow > 0 && dayRow > 0)
            {
                return new DateRows
                {
                    YearRow = yearRow,
                    MonthRow = monthRow,
                    DayRow = dayRow
                };
            }
        }

        return new DateRows();
    }

    private static bool ContainsToken(string input, string tokenName)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(tokenName))
        {
            return false;
        }

        foreach (Match match in TokenRegex.Matches(input))
        {
            var name = match.Groups["name"].Value.Trim();
            if (name.Equals(tokenName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static List<int> EnsureSummaryColumns(IXLWorksheet worksheet, IXLRange summaryColumnAnchor, int requiredColumnCount)
    {
        if (requiredColumnCount <= 0)
        {
            return new List<int>();
        }

        var firstColumn = summaryColumnAnchor.RangeAddress.FirstAddress.ColumnNumber;
        var anchorColumnCount = Math.Max(1, summaryColumnAnchor.ColumnCount());

        if (requiredColumnCount > anchorColumnCount)
        {
            for (var offset = anchorColumnCount; offset < requiredColumnCount; offset++)
            {
                var insertAfterColumn = firstColumn + offset - 1;
                worksheet.Column(insertAfterColumn).InsertColumnsAfter(1);
                ApplySummaryColumnFormatting(worksheet, summaryColumnAnchor, firstColumn, insertAfterColumn + 1);
            }
        }

        return Enumerable.Range(firstColumn, requiredColumnCount).ToList();
    }

    private static void ApplySummaryColumnFormatting(
        IXLWorksheet worksheet,
        IXLRange summaryColumnAnchor,
        int sourceColumn,
        int targetColumn)
    {
        var sourceTemplateColumn = worksheet.Column(sourceColumn);
        var targetTemplateColumn = worksheet.Column(targetColumn);

        targetTemplateColumn.Width = sourceTemplateColumn.Width;
        targetTemplateColumn.Style = sourceTemplateColumn.Style;

        var firstRow = 1;
        var lastRow = worksheet.LastRowUsed(XLCellsUsedOptions.All)?.RowNumber()
            ?? summaryColumnAnchor.RangeAddress.LastAddress.RowNumber;

        for (var row = firstRow; row <= lastRow; row++)
        {
            worksheet.Cell(row, targetColumn).Style = worksheet.Cell(row, sourceColumn).Style;
        }
    }

    private IXLRange TryGetNamedRange(XLWorkbook workbook, params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            return null;
        }

        foreach (var name in names)
        {
            var range = TryGetNamedRange(workbook, name);
            if (range != null)
            {
                return range;
            }
        }

        return null;
    }

    private IXLRange TryGetNamedRange(XLWorkbook workbook, string name)
    {
        foreach (var definedName in workbook.DefinedNames)
        {
            if (!definedName.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return definedName.Ranges.FirstOrDefault();
        }

        return null;
    }

    private string BuildProjectDisplay()
    {
        return $"{_settingsService.GlobalSettings.ProjectNumber} {_settingsService.GlobalSettings.ProjectName}".Trim();
    }

    private static IXLWorksheet GetOrCreateWorksheet(XLWorkbook workbook, string name)
    {
        if (workbook.TryGetWorksheet(name, out var existing))
        {
            return existing;
        }

        return workbook.Worksheets.FirstOrDefault() ?? workbook.AddWorksheet(name);
    }

    private void SaveAndOpen(XLWorkbook workbook, string folderPath, string fileName)
    {
        Directory.CreateDirectory(folderPath);

        var outputFileName = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.xlsx";

        var outputPath = Path.Combine(folderPath, outputFileName);
        try
        {
            workbook.SaveAs(outputPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }
        catch 
        {

        }

    }

}



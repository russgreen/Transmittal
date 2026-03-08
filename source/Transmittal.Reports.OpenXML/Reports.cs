using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;

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
        ApplyTemplateTokens(workbook, commonContext);
        ApplyCommonHeaderHeuristics(worksheet, "Transmittal Summary", null);

        var sheetListAnchor = TryGetNamedRange(workbook, "SheetListData");
        var distributionListAnchor = TryGetNamedRange(workbook, "DistributionListData");
        var summaryColumnAnchor = TryGetNamedRange(workbook, "SummaryColumnData");
        var transmittalFormatAnchor = TryGetNamedRange(workbook, "TransmittalFormatData");

        var sheetRange = FindSummarySheetSection(worksheet);
        var distributionRange = FindSummaryDistributionSection(worksheet, sheetRange);

        if (sheetListAnchor != null)
        {
            sheetRange.StartRow = sheetListAnchor.RangeAddress.FirstAddress.RowNumber;
            sheetRange.TemplateRow = CaptureTemplateRow(worksheet, sheetListAnchor);
            if (sheetRange.HeaderRow >= sheetRange.StartRow)
            {
                sheetRange.HeaderRow = Math.Max(1, sheetRange.StartRow - 1);
            }

            if (sheetRange.EndRow < sheetRange.StartRow)
            {
                sheetRange.EndRow = sheetRange.StartRow + 30;
            }
        }

        if (distributionListAnchor != null)
        {
            distributionRange.StartRow = distributionListAnchor.RangeAddress.FirstAddress.RowNumber;
            distributionRange.TemplateRow = CaptureTemplateRow(worksheet, distributionListAnchor);
            if (distributionRange.HeaderRow >= distributionRange.StartRow)
            {
                distributionRange.HeaderRow = Math.Max(1, distributionRange.StartRow - 1);
            }

            if (distributionRange.EndRow < distributionRange.StartRow)
            {
                distributionRange.EndRow = distributionRange.StartRow + 30;
            }
        }

        if (summaryColumnAnchor != null)
        {
            var firstColumn = summaryColumnAnchor.RangeAddress.FirstAddress.ColumnNumber;
            sheetRange.FirstDataColumn = firstColumn;
            distributionRange.FirstDataColumn = firstColumn;
        }

        if (transmittalFormatAnchor != null)
        {
            distributionRange.FormatRow = transmittalFormatAnchor.RangeAddress.FirstAddress.RowNumber;
        }

        if (distributionRange.HeaderRow > 0 && sheetRange.StartRow > 0 && distributionRange.HeaderRow > sheetRange.StartRow)
        {
            sheetRange.EndRow = Math.Min(sheetRange.EndRow, distributionRange.HeaderRow - 1);
        }

        var orderedColumns = orderedTransmittals
            .Select((t, idx) => new SummaryColumn
            {
                Index = idx,
                TransmittalId = t.ID,
                Date = t.TransDate
            })
            .ToList();

        var summaryColumnNumbers = GetDateColumns(worksheet, sheetRange.DateRows, sheetRange.FirstDataColumn)
            .Take(orderedColumns.Count)
            .ToList();

        ApplySummaryDateRows(worksheet, sheetRange, orderedColumns, summaryColumnNumbers);
        ApplySummaryDateRows(worksheet, distributionRange, orderedColumns, summaryColumnNumbers);
        ApplySummaryFormatRow(worksheet, distributionRange, orderedTransmittals, summaryColumnNumbers, transmittalFormatAnchor != null);

        var docRows = BuildSummaryItemRows(orderedTransmittals, commonContext);
        var shiftedRows = WriteSummaryDocumentMatrix(worksheet, sheetRange, orderedColumns, docRows, summaryColumnNumbers);
        if (shiftedRows > 0)
        {
            distributionRange.ShiftRows(shiftedRows);
        }

        var recipientRows = BuildSummaryDistributionRows(orderedTransmittals, commonContext);
        WriteSummaryDistributionMatrix(worksheet, distributionRange, orderedColumns, recipientRows, summaryColumnNumbers);

        //worksheet.Columns().AdjustToContents();
        SaveAndOpen(workbook, folderPath, fileName);
    }

    public void ShowMasterDocumentsListReport()
    {
        var transmittals = _transmittalService.GetTransmittals();

        var fileName = _settingsService.GlobalSettings.FileNameFilter.ParseFilename(_settingsService.GlobalSettings.ProjectNumber,
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

        var latestByDocument = transmittals
            .SelectMany(t => t.Items.Select(i => new MasterDocumentTemplateRow { Transmittal = t, Item = i }))
            .Where(x => !string.IsNullOrWhiteSpace(x.Item.DrgNumber))
            .GroupBy(x => x.Item.DrgNumber.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(x => x.Transmittal.TransDate)
                .ThenByDescending(x => x.Transmittal.ID)
                .First())
            .OrderBy(x => x.Item.DrgNumber, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Item.DrgName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var dataRange = TryGetNamedRange(workbook, "MasterDocumentListData");
        if (dataRange != null)
        {
            PopulateRowsFromNamedRange(worksheet,
                dataRange,
                latestByDocument,
                row => MergeContexts(commonContext, BuildMasterDocumentContext(row.Item, row.Transmittal)));

        }

        //worksheet.Columns().AdjustToContents();
        SaveAndOpen(workbook, folderPath, fileName);
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

    private void ApplyTemplateTokens(XLWorkbook workbook, Dictionary<string, string> context)
    {
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

                cell.Value = ReplaceTokens(text, context);
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
            ["DrgPackage"] = item.DrgPackage ?? string.Empty,
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

    private static string ReplaceTokens(string input, Dictionary<string, string> context)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.Contains("{{", StringComparison.Ordinal))
        {
            return input;
        }

        return TokenRegex.Replace(input, match =>
        {
            var tokenName = match.Groups["name"].Value.Trim();
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

    private int FindHeaderRow(IXLWorksheet worksheet, IEnumerable<string> keywords, int startRow, int endRow)
    {
        var normalizedKeywords = keywords
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!normalizedKeywords.Any())
        {
            return 0;
        }

        var bestRow = 0;
        var bestScore = 0;

        for (var row = startRow; row <= endRow; row++)
        {
            var cells = worksheet.Row(row).CellsUsed().Select(c => c.GetFormattedString()).ToList();
            if (!cells.Any())
            {
                continue;
            }

            var score = normalizedKeywords.Count(k => cells.Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase)));
            if (score > bestScore)
            {
                bestScore = score;
                bestRow = row;
            }
        }

        return bestRow;
    }

    private int[] ResolveDirectoryColumns(IXLWorksheet worksheet, int headerRow)
    {
        var map = worksheet.Row(headerRow).CellsUsed().ToDictionary(c => c.GetFormattedString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

        var nameCol = FindColumnByKeyword(map, "Name") ?? 1;
        var companyCol = FindColumnByKeyword(map, "Company") ?? nameCol + 1;
        var emailCol = FindColumnByKeyword(map, "Email") ?? companyCol + 1;
        var telCol = FindColumnByKeyword(map, "Tel") ?? (FindColumnByKeyword(map, "Telephone") ?? emailCol + 1);
        var roleCol = FindColumnByKeyword(map, "Role") ?? (FindColumnByKeyword(map, "Position") ?? telCol + 1);

        return new[] { nameCol, companyCol, emailCol, telCol, roleCol };
    }

    private int[] ResolveItemColumns(IXLWorksheet worksheet, int headerRow)
    {
        var map = worksheet.Row(headerRow).CellsUsed().ToDictionary(c => c.GetFormattedString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
        var numberCol = FindColumnByKeyword(map, "Drawing") ?? (FindColumnByKeyword(map, "Document") ?? 1);
        var nameCol = FindColumnByKeyword(map, "Name") ?? numberCol + 1;
        var revCol = FindColumnByKeyword(map, "Rev") ?? nameCol + 1;
        var statusCol = FindColumnByKeyword(map, "Status") ?? revCol + 1;
        var packageCol = FindColumnByKeyword(map, "Package") ?? statusCol + 1;
        return new[] { numberCol, nameCol, revCol, statusCol, packageCol };
    }

    private int FindDistributionHeaderRow(IXLWorksheet worksheet, int searchStart)
    {
        for (var row = searchStart; row <= Math.Max(searchStart + 200, worksheet.LastRowUsed()?.RowNumber() ?? searchStart); row++)
        {
            var text = string.Join("|", worksheet.Row(row).CellsUsed().Select(c => c.GetFormattedString()));
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (text.Contains("Format", StringComparison.OrdinalIgnoreCase) && text.Contains("Copies", StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }

            if (text.Contains("Company", StringComparison.OrdinalIgnoreCase) && text.Contains("Person", StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        return 0;
    }

    private int[] ResolveDistributionColumns(IXLWorksheet worksheet, int headerRow)
    {
        var map = worksheet.Row(headerRow).CellsUsed().ToDictionary(c => c.GetFormattedString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
        var nameCol = FindColumnByKeyword(map, "Person") ?? (FindColumnByKeyword(map, "Name") ?? 1);
        var companyCol = FindColumnByKeyword(map, "Company") ?? nameCol + 1;
        var formatCol = FindColumnByKeyword(map, "Format") ?? companyCol + 1;
        var copiesCol = FindColumnByKeyword(map, "Copies") ?? formatCol + 1;
        return new[] { nameCol, companyCol, formatCol, copiesCol };
    }

    private SummarySection FindSummarySheetSection(IXLWorksheet worksheet)
    {
        var section = new SummarySection();

        section.HeaderRow = FindHeaderRow(worksheet, new[] { "Drawing No", "Document ID", "Number", "Document Number" }, 1, 220);
        if (section.HeaderRow == 0)
        {
            section.HeaderRow = FindHeaderRow(worksheet, new[] { "Rev" }, 1, 220);
        }

        var revColumns = worksheet.Row(section.HeaderRow).CellsUsed()
            .Where(c => c.GetFormattedString().Equals("Rev", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Address.ColumnNumber)
            .ToList();

        section.FirstDataColumn = revColumns.Count > 0 ? revColumns.Min() : 5;
        section.DateRows = FindNearestDateRows(worksheet, section.HeaderRow);

        section.StartRow = section.HeaderRow + 1;
        section.EndRow = FindSectionEndRow(worksheet, section.StartRow, section.HeaderRow, new[] { "Year", "Recipient", "Distribution", "DISTRIBUTION", "DOCUMENTATION LIST" });

        section.NumberColumn = FindHeaderColumn(worksheet, section.HeaderRow, new[] { "Document ID", "Drawing", "Number" }) ?? 1;
        section.NameColumn = FindHeaderColumn(worksheet, section.HeaderRow, new[] { "Name" }) ?? (section.NumberColumn + 1);
        section.PaperColumn = FindHeaderColumn(worksheet, section.HeaderRow, new[] { "Paper" }) ?? (section.NameColumn + 1);
        section.LatestRevColumn = FindHeaderColumn(worksheet, section.HeaderRow, new[] { "Rev" }) ?? (section.NameColumn + 1);

        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? section.LatestRevColumn;
        section.TemplateRow = CaptureTemplateRow(worksheet, worksheet.Range(section.StartRow, 1, section.StartRow, lastColumn));

        return section;
    }

    private SummarySection FindSummaryDistributionSection(IXLWorksheet worksheet, SummarySection sheetSection)
    {
        var section = new SummarySection();

        var recipientRow = FindHeaderRow(worksheet, new[] { "Recipient" }, 1, 260);
        if (recipientRow > 0)
        {
            section.HeaderRow = recipientRow;
            section.StartRow = recipientRow + 1;
            section.DateRows = FindNearestDateRows(worksheet, recipientRow);
            section.FirstDataColumn = FindFirstNonEmptyDataColumn(worksheet, recipientRow, 2);
            section.NumberColumn = 1;
            section.NameColumn = 1;
            section.PaperColumn = 0;
            section.LatestRevColumn = 0;
            section.EndRow = FindSectionEndRow(worksheet, section.StartRow, recipientRow, new[] { "Year", "END" });
            section.FormatRow = recipientRow;
            section.CompanyColumn = 0;
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 20;
            section.TemplateRow = CaptureTemplateRow(worksheet, worksheet.Range(section.StartRow, 1, section.StartRow, lastColumn));
            return section;
        }

        var companyPersonHeader = FindHeaderRow(worksheet, new[] { "Company", "Person" }, 1, 260);
        if (companyPersonHeader > 0)
        {
            section.HeaderRow = companyPersonHeader;
            section.StartRow = companyPersonHeader + 1;
            section.DateRows = FindNearestDateRows(worksheet, companyPersonHeader);
            section.FirstDataColumn = FindFirstNonEmptyDataColumn(worksheet, section.DateRows.DayRow, 2);
            section.NameColumn = FindHeaderColumn(worksheet, companyPersonHeader, new[] { "Person", "Name" }) ?? 2;
            section.CompanyColumn = FindHeaderColumn(worksheet, companyPersonHeader, new[] { "Company" }) ?? 1;
            section.EndRow = FindSectionEndRow(worksheet, section.StartRow, companyPersonHeader, new[] { "DOCUMENTATION", "Document", "Drawing" });
            section.FormatRow = FindHeaderRow(worksheet, new[] { "METHOD OF ISSUE", "E=mail", "Format" }, 1, companyPersonHeader);
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 20;
            section.TemplateRow = CaptureTemplateRow(worksheet, worksheet.Range(section.StartRow, 1, section.StartRow, lastColumn));
            return section;
        }

        section.HeaderRow = sheetSection.EndRow + 2;
        section.StartRow = section.HeaderRow + 1;
        section.EndRow = section.StartRow + 30;
        section.FirstDataColumn = sheetSection.FirstDataColumn;
        section.DateRows = sheetSection.DateRows;
        section.NameColumn = 1;
        section.CompanyColumn = 2;
        section.FormatRow = section.HeaderRow;
        section.TemplateRow = sheetSection.TemplateRow;
        return section;
    }

    private void ApplySummaryDateRows(IXLWorksheet worksheet, SummarySection section, List<SummaryColumn> columns, List<int> dateColumns)
    {
        if (section.DateRows.DayRow == 0 || section.FirstDataColumn == 0 || dateColumns.Count == 0)
        {
            return;
        }

        var count = Math.Min(dateColumns.Count, columns.Count);

        for (var i = 0; i < count; i++)
        {
            var date = columns[i].Date;
            worksheet.Cell(section.DateRows.YearRow, dateColumns[i]).Value = date.Year % 100;
            worksheet.Cell(section.DateRows.MonthRow, dateColumns[i]).Value = date.Month;
            worksheet.Cell(section.DateRows.DayRow, dateColumns[i]).Value = date.Day;
        }
    }

    private void ApplySummaryFormatRow(
        IXLWorksheet worksheet,
        SummarySection section,
        List<TransmittalModel> transmittals,
        List<int> dateColumns,
        bool forceWriteAllColumns)
    {
        if (section.FormatRow <= 0 || section.DateRows.DayRow <= 0 || section.FirstDataColumn <= 0 || dateColumns.Count == 0)
        {
            return;
        }

        var ordered = transmittals.OrderBy(t => t.TransDate).ThenBy(t => t.ID).ToList();
        var count = Math.Min(dateColumns.Count, ordered.Count);

        for (var i = 0; i < count; i++)
        {
            var format = ordered[i].Distribution
                .GroupBy(d => d.TransFormat)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "";

            if (string.IsNullOrWhiteSpace(format))
            {
                continue;
            }

            if (forceWriteAllColumns || worksheet.Cell(section.FormatRow, dateColumns[i]).DataType == XLDataType.Text || section.FormatRow != section.HeaderRow)
            {
                worksheet.Cell(section.FormatRow, dateColumns[i]).Value = format;
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
            .GroupBy(x => x.Item.DrgNumber)
            .OrderBy(g => g.Key)
            .Select(g => new SummaryItemRow
            {
                DrawingNumber = g.Key,
                DrawingName = g.First().Item.DrgName,
                Paper = g.First().Item.DrgPaper,
                LatestRevision = g.Select(x => x.Item.DrgRev).LastOrDefault() ?? string.Empty,
                Package = g.Select(x => x.Item.DrgPackage).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
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
            else
            {
                row.RowContext = new Dictionary<string, string>(commonContext, StringComparer.OrdinalIgnoreCase);
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
                    Name = person.FullName,
                    Company = company?.CompanyName ?? string.Empty,
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
            else
            {
                row.RowContext = new Dictionary<string, string>(commonContext, StringComparer.OrdinalIgnoreCase);
            }
        }

        return rows;
    }

    private int WriteSummaryDocumentMatrix(IXLWorksheet worksheet, SummarySection section, List<SummaryColumn> columns, List<SummaryItemRow> rows, List<int> dateColumns)
    {
        if (section.StartRow <= 0 || section.FirstDataColumn <= 0 || dateColumns.Count == 0)
        {
            return 0;
        }

        var insertedRows = 0;
        if (rows.Count > Math.Max(0, section.EndRow - section.StartRow + 1))
        {
            var extra = rows.Count - Math.Max(0, section.EndRow - section.StartRow + 1);
            worksheet.Row(section.EndRow + 1).InsertRowsAbove(extra);
            section.EndRow += extra;
            insertedRows = extra;
        }

        worksheet.Range(section.StartRow, 1, section.EndRow, Math.Max(section.FirstDataColumn + columns.Count + 5, section.NameColumn + 2)).Clear(XLClearOptions.Contents);

        var columnCount = Math.Min(dateColumns.Count, columns.Count);

        for (var i = 0; i < rows.Count; i++)
        {
            var row = section.StartRow + i;
            var item = rows[i];

            if (section.TemplateRow != null)
            {
                ResetTemplateRow(worksheet, row, section.TemplateRow);
                RenderTemplateRow(worksheet, row, section.TemplateRow, item.RowContext ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }

            for (var c = 0; c < columnCount; c++)
            {
                if (item.RevisionsByTransmittal.TryGetValue(columns[c].TransmittalId, out var rev))
                {
                    worksheet.Cell(row, dateColumns[c]).Value = rev;
                }
            }
        }

        return insertedRows;
    }

    private void WriteSummaryDistributionMatrix(IXLWorksheet worksheet, SummarySection section, List<SummaryColumn> columns, List<SummaryDistributionRow> rows, List<int> dateColumns)
    {
        if (section.StartRow <= 0 || section.FirstDataColumn <= 0 || dateColumns.Count == 0)
        {
            return;
        }

        var capacity = Math.Max(0, section.EndRow - section.StartRow + 1);
        if (rows.Count > capacity)
        {
            worksheet.Row(section.EndRow + 1).InsertRowsAbove(rows.Count - capacity);
            section.EndRow += rows.Count - capacity;
        }

        var distributionDataRowStyle = worksheet.Row(section.StartRow).Style;
        worksheet.Range(section.StartRow, 1, section.EndRow, section.FirstDataColumn + columns.Count + 5).Clear(XLClearOptions.Contents);
        var columnCount = Math.Min(dateColumns.Count, columns.Count);

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNumber = section.StartRow + i;
            var row = rows[i];

            if (section.TemplateRow != null)
            {
                ResetTemplateRow(worksheet, rowNumber, section.TemplateRow);
                RenderTemplateRow(worksheet, rowNumber, section.TemplateRow, row.RowContext ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }
            else
            {
                worksheet.Row(rowNumber).Style = distributionDataRowStyle;
            }

            for (var c = 0; c < columnCount; c++)
            {
                if (!row.FormatByTransmittal.TryGetValue(columns[c].TransmittalId, out var cell))
                {
                    continue;
                }

                var targetCell = worksheet.Cell(rowNumber, dateColumns[c]);
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

    private DateRows FindNearestDateRows(IXLWorksheet worksheet, int anchorRow)
    {
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

    private List<int> GetDateColumns(IXLWorksheet worksheet, DateRows rows, int fallbackStart)
    {
        var columns = new SortedSet<int>();

        if (rows.DayRow > 0)
        {
            foreach (var cell in worksheet.Row(rows.DayRow).CellsUsed())
            {
                if (cell.Address.ColumnNumber < fallbackStart)
                {
                    continue;
                }

                var text = cell.GetFormattedString().Trim();
                if (int.TryParse(text, out _))
                {
                    columns.Add(cell.Address.ColumnNumber);
                }
            }
        }

        if (columns.Count == 0)
        {
            for (var col = fallbackStart; col < fallbackStart + 60; col++)
            {
                columns.Add(col);
            }
        }

        return columns.ToList();
    }

    private int FindSectionEndRow(IXLWorksheet worksheet, int startRow, int headerRow, IEnumerable<string> stopKeywords)
    {
        var maxRow = worksheet.LastRowUsed()?.RowNumber() ?? (startRow + 100);
        for (var row = startRow; row <= maxRow; row++)
        {
            var text = worksheet.Cell(row, 1).GetFormattedString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (row > headerRow + 1 && stopKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return row - 1;
            }
        }

        return maxRow;
    }

    private int FindFirstNonEmptyDataColumn(IXLWorksheet worksheet, int row, int minColumn)
    {
        foreach (var cell in worksheet.Row(row).CellsUsed())
        {
            if (cell.Address.ColumnNumber >= minColumn)
            {
                return cell.Address.ColumnNumber;
            }
        }

        return minColumn;
    }

    private int? FindHeaderColumn(IXLWorksheet worksheet, int row, IEnumerable<string> keywords)
    {
        foreach (var cell in worksheet.Row(row).CellsUsed())
        {
            var text = cell.GetFormattedString();
            if (keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return cell.Address.ColumnNumber;
            }
        }

        return null;
    }

    private int? FindColumnByKeyword(Dictionary<string, int> headerMap, string keyword)
    {
        foreach (var kv in headerMap)
        {
            if (kv.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return kv.Value;
            }
        }

        return null;
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

    private static void SaveAndOpen(XLWorkbook workbook, string folderPath, string fileName)
    {
        Directory.CreateDirectory(folderPath);

        var outputFileName = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.xlsx";

        var outputPath = Path.Combine(folderPath, outputFileName);
        workbook.SaveAs(outputPath);

        Process.Start(new ProcessStartInfo
        {
            FileName = outputPath,
            UseShellExecute = true
        });
    }

    private sealed class SummaryColumn
    {
        public int Index { get; set; }
        public int TransmittalId { get; set; }
        public DateTime Date { get; set; }
    }

    private sealed class DateRows
    {
        public int YearRow { get; set; }
        public int MonthRow { get; set; }
        public int DayRow { get; set; }
    }

    private sealed class SummarySection
    {
        public int HeaderRow { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int FirstDataColumn { get; set; }
        public int NumberColumn { get; set; }
        public int NameColumn { get; set; }
        public int PaperColumn { get; set; }
        public int LatestRevColumn { get; set; }
        public int CompanyColumn { get; set; }
        public int FormatRow { get; set; }
        public DateRows DateRows { get; set; } = new DateRows();
        public TemplateRow TemplateRow { get; set; }

        public void ShiftRows(int delta)
        {
            if (delta == 0)
            {
                return;
            }

            HeaderRow += delta;
            StartRow += delta;
            EndRow += delta;

            if (FormatRow > 0)
            {
                FormatRow += delta;
            }

            if (DateRows == null)
            {
                return;
            }

            if (DateRows.YearRow > 0)
            {
                DateRows.YearRow += delta;
            }

            if (DateRows.MonthRow > 0)
            {
                DateRows.MonthRow += delta;
            }

            if (DateRows.DayRow > 0)
            {
                DateRows.DayRow += delta;
            }

            if (TemplateRow != null)
            {
                TemplateRow.RowNumber += delta;
            }
        }
    }

    private sealed class SummaryItemRow
    {
        public string DrawingNumber { get; set; } = string.Empty;
        public string DrawingName { get; set; } = string.Empty;
        public string Paper { get; set; } = string.Empty;
        public string LatestRevision { get; set; } = string.Empty;
        public string Package { get; set; } = string.Empty;
        public Dictionary<int, string> RevisionsByTransmittal { get; set; } = new Dictionary<int, string>();
        public TransmittalItemModel TemplateItem { get; set; }
        public TransmittalModel TemplateTransmittal { get; set; }
        public Dictionary<string, string> RowContext { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class SummaryDistributionRow
    {
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public Dictionary<int, DistCell> FormatByTransmittal { get; set; } = new Dictionary<int, DistCell>();
        public TransmittalDistributionModel TemplateDistribution { get; set; }
        public TransmittalModel TemplateTransmittal { get; set; }
        public PersonModel Person { get; set; }
        public CompanyModel CompanyModel { get; set; }
        public Dictionary<string, string> RowContext { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class DistributionTemplateRow
    {
        public TransmittalDistributionModel Distribution { get; set; }
        public PersonModel Person { get; set; }
        public CompanyModel Company { get; set; }
    }

    private sealed class MasterDocumentTemplateRow
    {
        public TransmittalModel Transmittal { get; set; }
        public TransmittalItemModel Item { get; set; }
    }

    private sealed class TemplateRow
    {
        public int RowNumber { get; set; }
        public int FirstColumn { get; set; }
        public int LastColumn { get; set; }
        public IXLStyle RowStyle { get; set; }
        public List<TemplateCell> Cells { get; set; } = new List<TemplateCell>();
    }

    private sealed class TemplateCell
    {
        public int Column { get; set; }
        public IXLStyle Style { get; set; }
        public bool HasFormula { get; set; }
        public string FormulaA1 { get; set; }
        public string TextValue { get; set; }
        public XLCellValue Value { get; set; }
        public XLDataType DataType { get; set; }
    }

    private sealed class DistCell
    {
        public string Format { get; set; } = string.Empty;
        public int Copies { get; set; }
    }
}

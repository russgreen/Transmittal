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

        ApplyTemplateTokens(workbook, "Project Directory", null);
        ApplyCommonHeaderHeuristics(worksheet, "Project Directory", null);

        var templateRange = TryGetNamedRange(workbook, "ProjectDirectory") ?? TryGetNamedRange(workbook, "ProjectDirectoryRange");

        var filtered = projectDirectory
            .Where(x => x.Person?.ShowInReport == true)
            .OrderBy(x => x.Person.LastName)
            .ThenBy(x => x.Person.FirstName)
            .ToList();

        if (templateRange != null)
        {
            var startRow = templateRange.RangeAddress.FirstAddress.RowNumber;
            var endRow = templateRange.RangeAddress.LastAddress.RowNumber;
            var firstCol = templateRange.RangeAddress.FirstAddress.ColumnNumber;
            var rowCount = endRow - startRow + 1;

            if (filtered.Count > rowCount)
            {
                worksheet.Row(endRow + 1).InsertRowsAbove(filtered.Count - rowCount);
                endRow += filtered.Count - rowCount;
            }

            worksheet.Range(startRow, firstCol, endRow, firstCol + 6).Clear(XLClearOptions.Contents);

            for (var i = 0; i < filtered.Count; i++)
            {
                var row = startRow + i;
                var entry = filtered[i];
                worksheet.Cell(row, firstCol + 0).Value = entry.Person.FullName;
                worksheet.Cell(row, firstCol + 1).Value = entry.Company?.CompanyName ?? string.Empty;
                worksheet.Cell(row, firstCol + 2).Value = entry.Person.Position ?? entry.Company?.Role ?? string.Empty;
                worksheet.Cell(row, firstCol + 3).Value = entry.Person.Email ?? string.Empty;
                worksheet.Cell(row, firstCol + 4).Value = entry.Person.Mobile ?? string.Empty;
                worksheet.Cell(row, firstCol + 5).Value = entry.Person.Tel ?? string.Empty;
                worksheet.Cell(row, firstCol + 6).Value = entry.Company?.Address ?? string.Empty;
            }
        }
        else
        {
            var headerRow = FindHeaderRow(worksheet, new[] { "Name", "Company" }, 1, 60);
            if (headerRow == 0)
            {
                headerRow = 5;
                worksheet.Cell(headerRow, 1).Value = "Name";
                worksheet.Cell(headerRow, 2).Value = "Company";
                worksheet.Cell(headerRow, 3).Value = "Email";
                worksheet.Cell(headerRow, 4).Value = "Telephone";
                worksheet.Cell(headerRow, 5).Value = "Role";
                worksheet.Range(headerRow, 1, headerRow, 5).Style.Font.Bold = true;
            }

            var columns = ResolveDirectoryColumns(worksheet, headerRow);
            var startRow = headerRow + 1;
            var endRow = Math.Max(startRow + filtered.Count + 50, worksheet.LastRowUsed()?.RowNumber() ?? startRow);
            worksheet.Range(startRow, 1, endRow, Math.Max(columns.Max(), 8)).Clear(XLClearOptions.Contents);

            for (var i = 0; i < filtered.Count; i++)
            {
                var row = startRow + i;
                var entry = filtered[i];
                worksheet.Cell(row, columns[0]).Value = entry.Person.FullName;
                worksheet.Cell(row, columns[1]).Value = entry.Company?.CompanyName ?? string.Empty;
                worksheet.Cell(row, columns[2]).Value = entry.Person.Email ?? string.Empty;
                worksheet.Cell(row, columns[3]).Value = entry.Person.Tel ?? string.Empty;
                worksheet.Cell(row, columns[4]).Value = entry.Person.Position ?? entry.Company?.Role ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();
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
        ApplyTemplateTokens(workbook, "Transmittal Record", transmittal.TransDate);
        ApplyCommonHeaderHeuristics(worksheet, "Transmittal Record", transmittal.TransDate);

        var itemsHeaderRow = FindHeaderRow(worksheet, new[] { "Drawing", "Document", "Rev" }, 1, 80);
        if (itemsHeaderRow == 0)
        {
            itemsHeaderRow = 10;
            worksheet.Cell(itemsHeaderRow, 1).Value = "Document Number";
            worksheet.Cell(itemsHeaderRow, 2).Value = "Name";
            worksheet.Cell(itemsHeaderRow, 3).Value = "Rev";
            worksheet.Cell(itemsHeaderRow, 4).Value = "Status";
            worksheet.Cell(itemsHeaderRow, 5).Value = "Package";
            worksheet.Range(itemsHeaderRow, 1, itemsHeaderRow, 5).Style.Font.Bold = true;
        }

        var itemCols = ResolveItemColumns(worksheet, itemsHeaderRow);
        var distHeaderRow = FindDistributionHeaderRow(worksheet, itemsHeaderRow + 1);
        if (distHeaderRow == 0)
        {
            distHeaderRow = itemsHeaderRow + Math.Max(transmittal.Items.Count, 10) + 2;
            worksheet.Cell(distHeaderRow, 1).Value = "Name";
            worksheet.Cell(distHeaderRow, 2).Value = "Company";
            worksheet.Cell(distHeaderRow, 3).Value = "Format";
            worksheet.Cell(distHeaderRow, 4).Value = "Copies";
            worksheet.Range(distHeaderRow, 1, distHeaderRow, 4).Style.Font.Bold = true;
        }

        var itemsStartRow = itemsHeaderRow + 1;
        var itemsEndRow = Math.Max(distHeaderRow - 1, itemsStartRow + transmittal.Items.Count + 5);
        worksheet.Range(itemsStartRow, 1, itemsEndRow, Math.Max(itemCols.Max(), 8)).Clear(XLClearOptions.Contents);

        var orderedItems = transmittal.Items.OrderBy(x => x.DrgNumber).ToList();
        for (var i = 0; i < orderedItems.Count; i++)
        {
            var row = itemsStartRow + i;
            var item = orderedItems[i];
            worksheet.Cell(row, itemCols[0]).Value = item.DrgNumber;
            worksheet.Cell(row, itemCols[1]).Value = item.DrgName;
            worksheet.Cell(row, itemCols[2]).Value = item.DrgRev;
            worksheet.Cell(row, itemCols[3]).Value = item.DrgStatus;
            worksheet.Cell(row, itemCols[4]).Value = item.DrgPackage ?? string.Empty;
        }

        var distCols = ResolveDistributionColumns(worksheet, distHeaderRow);
        var distStartRow = distHeaderRow + 1;
        var distEndRow = Math.Max(distStartRow + transmittal.Distribution.Count + 10, worksheet.LastRowUsed()?.RowNumber() ?? distStartRow);
        worksheet.Range(distStartRow, 1, distEndRow, Math.Max(distCols.Max(), 8)).Clear(XLClearOptions.Contents);

        foreach (var pair in transmittal.Distribution.Select((d, idx) => new { Dist = d, Index = idx }))
        {
            var row = distStartRow + pair.Index;
            var person = _contactDirectoryService.GetPerson(pair.Dist.PersonID);
            var company = _contactDirectoryService.GetCompany(person.CompanyID);

            worksheet.Cell(row, distCols[0]).Value = person.FullName;
            worksheet.Cell(row, distCols[1]).Value = company?.CompanyName ?? string.Empty;
            worksheet.Cell(row, distCols[2]).Value = pair.Dist.TransFormat;
            worksheet.Cell(row, distCols[3]).Value = pair.Dist.TransCopies;
        }

        worksheet.Columns().AdjustToContents();
        SaveAndOpen(workbook, folderPath, fileName);
    }

    public void ShowTransmittalSummaryReport(List<TransmittalModel> transmittals = null)
    {
        transmittals ??= _transmittalService.GetTransmittals();
        var orderedTransmittals = transmittals.OrderBy(x => x.TransDate).ThenBy(x => x.ID).ToList();

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
        ApplyTemplateTokens(workbook, "Transmittal Summary", null);
        ApplyCommonHeaderHeuristics(worksheet, "Transmittal Summary", null);

        var sheetSection = FindSummarySheetSection(worksheet);
        var distributionSection = FindSummaryDistributionSection(worksheet, sheetSection);

        var orderedColumns = orderedTransmittals
            .Select((t, idx) => new SummaryColumn
            {
                Index = idx,
                TransmittalId = t.ID,
                Date = t.TransDate
            })
            .ToList();

        ApplySummaryDateRows(worksheet, sheetSection, orderedColumns);
        ApplySummaryDateRows(worksheet, distributionSection, orderedColumns);
        ApplySummaryFormatRow(worksheet, distributionSection, orderedTransmittals);

        var docRows = BuildSummaryItemRows(orderedTransmittals);
        WriteSummaryDocumentMatrix(worksheet, sheetSection, orderedColumns, docRows);

        var recipientRows = BuildSummaryDistributionRows(orderedTransmittals);
        WriteSummaryDistributionMatrix(worksheet, distributionSection, orderedColumns, recipientRows);

        worksheet.Columns().AdjustToContents();
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
        ApplyTemplateTokens(workbook, "Master Documents List", null);
        ApplyCommonHeaderHeuristics(worksheet, "Master Documents List", null);

        var row = FindHeaderRow(worksheet, new[] { "Date", "Transmittal", "Document" }, 1, 60);
        if (row == 0)
        {
            row = 4;
            worksheet.Cell(row, 1).Value = "Date";
            worksheet.Cell(row, 2).Value = "Transmittal";
            worksheet.Cell(row, 3).Value = "Document Number";
            worksheet.Cell(row, 4).Value = "Name";
            worksheet.Cell(row, 5).Value = "Rev";
            worksheet.Cell(row, 6).Value = "Status";
            worksheet.Range(row, 1, row, 6).Style.Font.Bold = true;
        }

        foreach (var pair in transmittals.OrderBy(x => x.TransDate).ThenBy(x => x.ID)
                     .SelectMany(t => t.Items.Select(i => new { Transmittal = t, Item = i })))
        {
            row++;
            worksheet.Cell(row, 1).Value = pair.Transmittal.TransDate;
            worksheet.Cell(row, 1).Style.DateFormat.Format = _settingsService.GlobalSettings.DateFormatString;
            worksheet.Cell(row, 2).Value = pair.Transmittal.ID;
            worksheet.Cell(row, 3).Value = pair.Item.DrgNumber;
            worksheet.Cell(row, 4).Value = pair.Item.DrgName;
            worksheet.Cell(row, 5).Value = pair.Item.DrgRev;
            worksheet.Cell(row, 6).Value = pair.Item.DrgStatus;
        }

        worksheet.Columns().AdjustToContents();
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

    private void ApplyTemplateTokens(XLWorkbook workbook, string reportTitle, DateTime? transmittalDate)
    {
        var projectDisplay = BuildProjectDisplay();
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{{Project}}"] = projectDisplay,
            ["{{ProjectDisplay}}"] = projectDisplay,
            ["{{ProjectName}}"] = _settingsService.GlobalSettings.ProjectName ?? string.Empty,
            ["{{ProjectNumber}}"] = _settingsService.GlobalSettings.ProjectNumber ?? string.Empty,
            ["{{ProjectIdentifier}}"] = _settingsService.GlobalSettings.ProjectIdentifier ?? string.Empty,
            ["{{ClientName}}"] = _settingsService.GlobalSettings.ClientName ?? string.Empty,
            ["{{ReportTitle}}"] = reportTitle,
            ["{{ReportDate}}"] = DateTime.Now.ToString("D"),
            ["{{TransmittalDate}}"] = transmittalDate?.ToString("D") ?? string.Empty,
        };

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

                foreach (var replacement in replacements)
                {
                    text = text.Replace(replacement.Key, replacement.Value, StringComparison.OrdinalIgnoreCase);
                }

                cell.Value = text;
            }
        }
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
        for (var row = startRow; row <= endRow; row++)
        {
            var cells = worksheet.Row(row).CellsUsed().Select(c => c.GetFormattedString()).ToList();
            if (!cells.Any())
            {
                continue;
            }

            if (keywords.Any(k => cells.Any(c => c.Contains(k, StringComparison.OrdinalIgnoreCase))))
            {
                return row;
            }
        }

        return 0;
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
        return section;
    }

    private void ApplySummaryDateRows(IXLWorksheet worksheet, SummarySection section, List<SummaryColumn> columns)
    {
        if (section.DateRows.DayRow == 0 || section.FirstDataColumn == 0)
        {
            return;
        }

        var colNumbers = GetDateColumns(worksheet, section.DateRows, section.FirstDataColumn);
        var dateColumns = colNumbers.Take(columns.Count).ToList();

        for (var i = 0; i < dateColumns.Count; i++)
        {
            var date = columns[i].Date;
            worksheet.Cell(section.DateRows.YearRow, dateColumns[i]).Value = date.Year % 100;
            worksheet.Cell(section.DateRows.MonthRow, dateColumns[i]).Value = date.Month;
            worksheet.Cell(section.DateRows.DayRow, dateColumns[i]).Value = date.Day;
        }
    }

    private void ApplySummaryFormatRow(IXLWorksheet worksheet, SummarySection section, List<TransmittalModel> transmittals)
    {
        if (section.FormatRow <= 0 || section.DateRows.DayRow <= 0 || section.FirstDataColumn <= 0)
        {
            return;
        }

        var colNumbers = GetDateColumns(worksheet, section.DateRows, section.FirstDataColumn);
        var ordered = transmittals.OrderBy(t => t.TransDate).ThenBy(t => t.ID).ToList();
        var count = Math.Min(colNumbers.Count, ordered.Count);

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

            if (worksheet.Cell(section.FormatRow, colNumbers[i]).DataType == XLDataType.Text || section.FormatRow != section.HeaderRow)
            {
                worksheet.Cell(section.FormatRow, colNumbers[i]).Value = format;
            }
        }
    }

    private List<SummaryItemRow> BuildSummaryItemRows(List<TransmittalModel> transmittals)
    {
        var rows = transmittals
            .OrderBy(t => t.TransDate)
            .ThenBy(t => t.ID)
            .SelectMany(t => t.Items.Select(item => new { t.ID, Item = item }))
            .GroupBy(x => x.Item.DrgNumber)
            .OrderBy(g => g.Key)
            .Select(g => new SummaryItemRow
            {
                DrawingNumber = g.Key,
                DrawingName = g.First().Item.DrgName,
                Paper = g.First().Item.DrgPaper,
                LatestRevision = g.Select(x => x.Item.DrgRev).LastOrDefault() ?? string.Empty,
                Package = g.Select(x => x.Item.DrgPackage).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                RevisionsByTransmittal = g.ToDictionary(x => x.ID, x => x.Item.DrgRev)
            })
            .ToList();

        return rows;
    }

    private List<SummaryDistributionRow> BuildSummaryDistributionRows(List<TransmittalModel> transmittals)
    {
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
                            })
                };
            })
            .ToList();

        return rows;
    }

    private void WriteSummaryDocumentMatrix(IXLWorksheet worksheet, SummarySection section, List<SummaryColumn> columns, List<SummaryItemRow> rows)
    {
        if (section.StartRow <= 0 || section.FirstDataColumn <= 0)
        {
            return;
        }

        if (rows.Count > Math.Max(0, section.EndRow - section.StartRow + 1))
        {
            var extra = rows.Count - Math.Max(0, section.EndRow - section.StartRow + 1);
            worksheet.Row(section.EndRow + 1).InsertRowsAbove(extra);
            section.EndRow += extra;
        }

        worksheet.Range(section.StartRow, 1, section.EndRow, Math.Max(section.FirstDataColumn + columns.Count + 5, section.NameColumn + 2)).Clear(XLClearOptions.Contents);

        var dateColumns = GetDateColumns(worksheet, section.DateRows, section.FirstDataColumn).Take(columns.Count).ToList();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = section.StartRow + i;
            var item = rows[i];

            worksheet.Cell(row, section.NumberColumn).Value = item.DrawingNumber;
            worksheet.Cell(row, section.NameColumn).Value = item.DrawingName;

            if (section.PaperColumn > 0)
            {
                worksheet.Cell(row, section.PaperColumn).Value = item.Paper ?? string.Empty;
            }

            if (section.LatestRevColumn > 0 && section.LatestRevColumn < section.FirstDataColumn)
            {
                worksheet.Cell(row, section.LatestRevColumn).Value = item.LatestRevision;
            }

            for (var c = 0; c < dateColumns.Count; c++)
            {
                if (item.RevisionsByTransmittal.TryGetValue(columns[c].TransmittalId, out var rev))
                {
                    worksheet.Cell(row, dateColumns[c]).Value = rev;
                }
            }
        }
    }

    private void WriteSummaryDistributionMatrix(IXLWorksheet worksheet, SummarySection section, List<SummaryColumn> columns, List<SummaryDistributionRow> rows)
    {
        if (section.StartRow <= 0 || section.FirstDataColumn <= 0)
        {
            return;
        }

        var capacity = Math.Max(0, section.EndRow - section.StartRow + 1);
        if (rows.Count > capacity)
        {
            worksheet.Row(section.EndRow + 1).InsertRowsAbove(rows.Count - capacity);
            section.EndRow += rows.Count - capacity;
        }

        worksheet.Range(section.StartRow, 1, section.EndRow, section.FirstDataColumn + columns.Count + 5).Clear(XLClearOptions.Contents);
        var dateColumns = GetDateColumns(worksheet, section.DateRows, section.FirstDataColumn).Take(columns.Count).ToList();

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNumber = section.StartRow + i;
            var row = rows[i];

            if (section.CompanyColumn > 0)
            {
                worksheet.Cell(rowNumber, section.CompanyColumn).Value = row.Company;
            }

            worksheet.Cell(rowNumber, section.NameColumn).Value = row.Name;

            for (var c = 0; c < dateColumns.Count; c++)
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
    }

    private sealed class SummaryItemRow
    {
        public string DrawingNumber { get; set; } = string.Empty;
        public string DrawingName { get; set; } = string.Empty;
        public string Paper { get; set; } = string.Empty;
        public string LatestRevision { get; set; } = string.Empty;
        public string Package { get; set; } = string.Empty;
        public Dictionary<int, string> RevisionsByTransmittal { get; set; } = new Dictionary<int, string>();
    }

    private sealed class SummaryDistributionRow
    {
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public Dictionary<int, DistCell> FormatByTransmittal { get; set; } = new Dictionary<int, DistCell>();
    }

    private sealed class DistCell
    {
        public string Format { get; set; } = string.Empty;
        public int Copies { get; set; }
    }
}

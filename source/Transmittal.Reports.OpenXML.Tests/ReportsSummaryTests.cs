using ClosedXML.Excel;
using Transmittal.Library.Models;
using Transmittal.Reports.OpenXML;
using Transmittal.Reports.OpenXML.Models;

namespace Transmittal.Reports.OpenXML.Tests;

public class ReportsSummaryTests
{
    [Test]
    public async Task WriteSummaryDocumentMatrix_WritesRevisionValuesByTransmittal()
    {
        var sut = ReportsTestHelpers.CreateSut();
        var transmittals = new List<TransmittalModel>
        {
            new()
            {
                ID = 1,
                TransDate = new DateTime(2024, 1, 1),
                Items =
                [
                    new TransmittalItemModel { DrgVolume = "A", DrgNumber = "A-001", DrgRev = "P01", DrgName = "Doc A" },
                    new TransmittalItemModel { DrgVolume = "A", DrgNumber = "A-002", DrgRev = "P01", DrgName = "Doc B" }
                ]
            },
            new()
            {
                ID = 2,
                TransDate = new DateTime(2024, 2, 1),
                Items =
                [
                    new TransmittalItemModel { DrgVolume = "A", DrgNumber = "A-001", DrgRev = "P02", DrgName = "Doc A" }
                ]
            }
        };

        var commonContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rows = ReportsTestHelpers.InvokeInstancePrivate<List<SummaryItemRow>>(
            sut,
            "BuildSummaryItemRows",
            [typeof(List<TransmittalModel>), typeof(Dictionary<string, string>)],
            transmittals,
            commonContext);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Summary");
        worksheet.Cell(1, 1).Value = "{{DrgNumber}}";
        worksheet.Cell(1, 2).Value = "{{DrgName}}";
        var dataRange = worksheet.Range(1, 1, 1, 2);

        var columns = new List<SummaryColumn>
        {
            new() { Index = 0, TransmittalId = 1, Date = new DateTime(2024, 1, 1) },
            new() { Index = 1, TransmittalId = 2, Date = new DateTime(2024, 2, 1) }
        };

        var dateColumns = new List<int> { 3, 4 };

        _ = ReportsTestHelpers.InvokeInstancePrivate<int>(
            sut,
            "WriteSummaryDocumentMatrix",
            [typeof(IXLWorksheet), typeof(IXLRange), typeof(List<SummaryItemRow>), typeof(List<SummaryColumn>), typeof(List<int>)],
            worksheet,
            dataRange,
            rows,
            columns,
            dateColumns);

        var rowA001 = ReportsTestHelpers.FindRowByValue(worksheet, 1, "A-001");
        var rowA002 = ReportsTestHelpers.FindRowByValue(worksheet, 1, "A-002");

        await Assert.That(rowA001 > 0).IsTrue();
        await Assert.That(rowA002 > 0).IsTrue();

        await Assert.That(worksheet.Cell(rowA001, 3).GetString()).IsEqualTo("P01");
        await Assert.That(worksheet.Cell(rowA001, 4).GetString()).IsEqualTo("P02");
        await Assert.That(worksheet.Cell(rowA002, 3).GetString()).IsEqualTo("P01");
        await Assert.That(worksheet.Cell(rowA002, 4).GetString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task WriteSummaryDistributionMatrix_AggregatesCopiesPerRecipientAndTransmittal()
    {
        var contactService = new FakeContactDirectoryService();
        contactService.AddCompany(new CompanyModel { ID = 10, CompanyName = "Acme" });
        contactService.AddCompany(new CompanyModel { ID = 20, CompanyName = "Beta" });
        contactService.AddPerson(new PersonModel { ID = 1, FirstName = "Ann", LastName = "Smith", CompanyID = 10 });
        contactService.AddPerson(new PersonModel { ID = 2, FirstName = "Bob", LastName = "Jones", CompanyID = 20 });

        var sut = ReportsTestHelpers.CreateSut(contactService: contactService);
        var transmittals = new List<TransmittalModel>
        {
            new()
            {
                ID = 1,
                TransDate = new DateTime(2024, 1, 1),
                Distribution =
                [
                    new TransmittalDistributionModel { PersonID = 1, TransFormat = "E", TransCopies = 1 },
                    new TransmittalDistributionModel { PersonID = 1, TransFormat = "E", TransCopies = 2 }
                ]
            },
            new()
            {
                ID = 2,
                TransDate = new DateTime(2024, 2, 1),
                Distribution =
                [
                    new TransmittalDistributionModel { PersonID = 1, TransFormat = "P", TransCopies = 4 },
                    new TransmittalDistributionModel { PersonID = 2, TransFormat = "E", TransCopies = 1 }
                ]
            }
        };

        var rows = ReportsTestHelpers.InvokeInstancePrivate<List<SummaryDistributionRow>>(
            sut,
            "BuildSummaryDistributionRows",
            [typeof(List<TransmittalModel>), typeof(Dictionary<string, string>)],
            transmittals,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Summary");
        worksheet.Cell(1, 1).Value = "{{PersonName}}";
        worksheet.Cell(1, 2).Value = "{{CompanyName}}";
        worksheet.Cell(1, 4).Value = 0;
        worksheet.Cell(1, 5).Value = 0;
        var dataRange = worksheet.Range(1, 1, 1, 2);

        var columns = new List<SummaryColumn>
        {
            new() { Index = 0, TransmittalId = 1, Date = new DateTime(2024, 1, 1) },
            new() { Index = 1, TransmittalId = 2, Date = new DateTime(2024, 2, 1) }
        };

        var dateColumns = new List<int> { 4, 5 };

        ReportsTestHelpers.InvokeInstancePrivate<object>(
            sut,
            "WriteSummaryDistributionMatrix",
            [typeof(IXLWorksheet), typeof(IXLRange), typeof(List<SummaryDistributionRow>), typeof(List<SummaryColumn>), typeof(List<int>)],
            worksheet,
            dataRange,
            rows,
            columns,
            dateColumns);

        var annRow = ReportsTestHelpers.FindRowByValue(worksheet, 1, "Ann Smith");
        var bobRow = ReportsTestHelpers.FindRowByValue(worksheet, 1, "Bob Jones");

        await Assert.That(worksheet.Cell(annRow, 4).GetValue<int>()).IsEqualTo(3);
        await Assert.That(worksheet.Cell(annRow, 5).GetValue<int>()).IsEqualTo(4);
        await Assert.That(worksheet.Cell(bobRow, 4).GetString()).IsEqualTo(string.Empty);
        await Assert.That(worksheet.Cell(bobRow, 5).GetValue<int>()).IsEqualTo(1);
    }

    [Test]
    public async Task EnsureSummaryColumns_ExtendsColumnListFromAnchor()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Summary");
        worksheet.Cell(1, 3).Value = "C";
        worksheet.Cell(1, 4).Value = "D";
        var anchor = worksheet.Range(1, 3, 1, 4);

        var columns = ReportsTestHelpers.InvokeStaticPrivate<List<int>>(
            typeof(Reports),
            "EnsureSummaryColumns",
            [typeof(IXLWorksheet), typeof(IXLRange), typeof(int)],
            worksheet,
            anchor,
            4);

        await Assert.That(columns.Count).IsEqualTo(4);
        await Assert.That(columns[0]).IsEqualTo(3);
        await Assert.That(columns[3]).IsEqualTo(6);
    }

    [Test]
    public async Task ApplySummaryDateRows_WritesYearMonthDayValuesToDateColumns()
    {
        var sut = ReportsTestHelpers.CreateSut();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Summary");

        var dateRows = new DateRows { YearRow = 1, MonthRow = 2, DayRow = 3 };
        var columns = new List<SummaryColumn>
        {
            new() { Index = 0, TransmittalId = 10, Date = new DateTime(2024, 3, 21) },
            new() { Index = 1, TransmittalId = 11, Date = new DateTime(2024, 4, 22) }
        };

        var dateColumns = new List<int> { 5, 6 };

        ReportsTestHelpers.InvokeInstancePrivate<object>(
            sut,
            "ApplySummaryDateRows",
            [typeof(IXLWorksheet), typeof(DateRows), typeof(List<SummaryColumn>), typeof(List<int>)],
            worksheet,
            dateRows,
            columns,
            dateColumns);

        await Assert.That(worksheet.Cell(1, 5).GetValue<int>()).IsEqualTo(24);
        await Assert.That(worksheet.Cell(2, 5).GetValue<int>()).IsEqualTo(3);
        await Assert.That(worksheet.Cell(3, 5).GetValue<int>()).IsEqualTo(21);

        await Assert.That(worksheet.Cell(1, 6).GetValue<int>()).IsEqualTo(24);
        await Assert.That(worksheet.Cell(2, 6).GetValue<int>()).IsEqualTo(4);
        await Assert.That(worksheet.Cell(3, 6).GetValue<int>()).IsEqualTo(22);
    }

    [Test]
    public async Task PopulateRowsFromNamedRange_SortsRowsByTaggedColumn()
    {
        var sut = ReportsTestHelpers.CreateSut();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Summary");
        worksheet.Cell(1, 1).Value = "{{Name}}";
        worksheet.Cell(1, 2).Value = "{{Code}}";
        worksheet.Cell(2, 1).Value = "<<sort>>";

        var dataRange = worksheet.Range(1, 1, 1, 2);

        var rows = new List<SortRow>
        {
            new() { Name = "Zulu", Code = "03" },
            new() { Name = "alpha", Code = "01" },
            new() { Name = "Mike", Code = "02" }
        };

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            dataRange,
            rows,
            row => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Name"] = row.Name,
                ["Code"] = row.Code
            });

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("alpha");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("01");

        await Assert.That(worksheet.Cell(2, 1).GetString()).IsEqualTo("Mike");
        await Assert.That(worksheet.Cell(2, 2).GetString()).IsEqualTo("02");

        await Assert.That(worksheet.Cell(3, 1).GetString()).IsEqualTo("Zulu");
        await Assert.That(worksheet.Cell(3, 2).GetString()).IsEqualTo("03");

        await Assert.That(worksheet.Cell(2, 1).GetString()).IsNotEqualTo("<<sort>>");
    }

    [Test]
    public async Task PopulateRowsFromNamedRange_SortsRowsByMultipleTaggedColumns()
    {
        var sut = ReportsTestHelpers.CreateSut();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Summary");
        worksheet.Cell(1, 1).Value = "{{Group}}";
        worksheet.Cell(1, 2).Value = "{{Name}}";
        worksheet.Cell(2, 1).Value = "<<sort>>";
        worksheet.Cell(2, 2).Value = "<<sort>>";

        var dataRange = worksheet.Range(1, 1, 1, 2);

        var rows = new List<SortRow>
        {
            new() { Group = "B", Name = "Beta" },
            new() { Group = "A", Name = "Zulu" },
            new() { Group = "A", Name = "Alpha" }
        };

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            dataRange,
            rows,
            row => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Group"] = row.Group,
                ["Name"] = row.Name
            });

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("A");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("Alpha");

        await Assert.That(worksheet.Cell(2, 1).GetString()).IsEqualTo("A");
        await Assert.That(worksheet.Cell(2, 2).GetString()).IsEqualTo("Zulu");

        await Assert.That(worksheet.Cell(3, 1).GetString()).IsEqualTo("B");
        await Assert.That(worksheet.Cell(3, 2).GetString()).IsEqualTo("Beta");
    }

    private sealed class SortRow
    {
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}

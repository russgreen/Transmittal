using ClosedXML.Excel;
using Transmittal.Library.Models;
using Transmittal.Reports.OpenXML.Models;

namespace Transmittal.Reports.OpenXML.Tests;

public class ReportsTransmittalTests
{
    [Test]
    public async Task TransmittalReport_ItemRows_RenderExpectedTokenValues()
    {
        var sut = ReportsTestHelpers.CreateSut();
        var transmittal = new TransmittalModel
        {
            ID = 12,
            TransDate = new DateTime(2024, 5, 15),
            Items =
            [
                new TransmittalItemModel { DrgNumber = "B-200", DrgRev = "P03", DrgName = "Doc B", DrgVolume = "A" },
                new TransmittalItemModel { DrgNumber = "A-100", DrgRev = "P02", DrgName = "Doc A", DrgVolume = "A" }
            ]
        };

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Transmittal");
        worksheet.Cell(1, 1).Value = "{{DrgNumber}}";
        worksheet.Cell(1, 2).Value = "{{DrgRev}}";
        worksheet.Cell(1, 3).Value = "{{TransID}}";
        var templateRange = worksheet.Range(1, 1, 1, 3);

        var orderedItems = transmittal.Items.OrderBy(x => x.DrgNumber).ToList();

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            templateRange,
            orderedItems,
            item => ReportsTestHelpers.InvokeInstancePrivate<Dictionary<string, string>>(
                sut,
                "BuildTransmittalItemContext",
                [typeof(TransmittalItemModel), typeof(TransmittalModel)],
                item,
                transmittal));

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("A-100");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("P02");
        await Assert.That(worksheet.Cell(1, 3).GetString()).IsEqualTo("12");

        await Assert.That(worksheet.Cell(2, 1).GetString()).IsEqualTo("B-200");
        await Assert.That(worksheet.Cell(2, 2).GetString()).IsEqualTo("P03");
        await Assert.That(worksheet.Cell(2, 3).GetString()).IsEqualTo("12");
    }

    [Test]
    public async Task TransmittalReport_DistributionRows_RenderExpectedTokenValues()
    {
        var contactService = new FakeContactDirectoryService();
        contactService.AddCompany(new CompanyModel { ID = 10, CompanyName = "Acme" });
        contactService.AddPerson(new PersonModel { ID = 1, FirstName = "Ann", LastName = "Smith", CompanyID = 10, Email = "ann@acme.test" });

        var sut = ReportsTestHelpers.CreateSut(contactService: contactService);

        var rows = new List<DistributionTemplateRow>
        {
            new()
            {
                Distribution = new TransmittalDistributionModel { PersonID = 1, TransFormat = "E", TransCopies = 2 },
                Person = contactService.GetPerson(1),
                Company = contactService.GetCompany(10)
            }
        };

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Transmittal");
        worksheet.Cell(1, 1).Value = "{{PersonName}}";
        worksheet.Cell(1, 2).Value = "{{CompanyName}}";
        worksheet.Cell(1, 3).Value = "{{TransFormat}}";
        worksheet.Cell(1, 4).Value = "{{TransCopies}}";
        var templateRange = worksheet.Range(1, 1, 1, 4);

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            templateRange,
            rows,
            row => ReportsTestHelpers.InvokeInstancePrivate<Dictionary<string, string>>(
                sut,
                "BuildDistributionContext",
                [typeof(TransmittalDistributionModel), typeof(PersonModel), typeof(CompanyModel), typeof(int)],
                row.Distribution,
                row.Person,
                row.Company,
                77));

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("Ann Smith");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("Acme");
        await Assert.That(worksheet.Cell(1, 3).GetString()).IsEqualTo("E");
        await Assert.That(worksheet.Cell(1, 4).GetString()).IsEqualTo("2");
    }

    [Test]
    public async Task TransmittalReport_ItemRows_SortUsingTemplateSortTag()
    {
        var sut = ReportsTestHelpers.CreateSut();
        var transmittal = new TransmittalModel
        {
            ID = 12,
            TransDate = new DateTime(2024, 5, 15),
            Items =
            [
                new TransmittalItemModel { DrgVolume = "B", DrgNumber = "200", DrgRev = "P03", DrgName = "Doc B" },
                new TransmittalItemModel { DrgVolume = "A", DrgNumber = "300", DrgRev = "P04", DrgName = "Doc C" },
                new TransmittalItemModel { DrgVolume = "A", DrgNumber = "100", DrgRev = "P02", DrgName = "Doc A" }
            ]
        };

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Transmittal");
        worksheet.Cell(1, 1).Value = "{{DrgVolume}}";
        worksheet.Cell(1, 2).Value = "{{DrgNumber}}";
        worksheet.Cell(1, 3).Value = "{{DrgRev}}";
        worksheet.Cell(2, 1).Value = "<<sort>>";
        worksheet.Cell(2, 2).Value = "<<sort>>";
        var templateRange = worksheet.Range(1, 1, 1, 3);

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            templateRange,
            transmittal.Items,
            item => ReportsTestHelpers.InvokeInstancePrivate<Dictionary<string, string>>(
                sut,
                "BuildTransmittalItemContext",
                [typeof(TransmittalItemModel), typeof(TransmittalModel)],
                item,
                transmittal));

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("A");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("100");

        await Assert.That(worksheet.Cell(2, 1).GetString()).IsEqualTo("A");
        await Assert.That(worksheet.Cell(2, 2).GetString()).IsEqualTo("300");

        await Assert.That(worksheet.Cell(3, 1).GetString()).IsEqualTo("B");
        await Assert.That(worksheet.Cell(3, 2).GetString()).IsEqualTo("200");
    }
}

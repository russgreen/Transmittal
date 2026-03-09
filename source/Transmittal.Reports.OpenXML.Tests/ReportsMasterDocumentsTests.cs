using ClosedXML.Excel;
using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML.Tests;

public class ReportsMasterDocumentsTests
{
    [Test]
    public async Task MasterDocumentsReport_Rows_RenderExpectedTokenValues()
    {
        var sut = ReportsTestHelpers.CreateSut();

        var transmittal = new TransmittalModel
        {
            ID = 22,
            TransDate = new DateTime(2024, 6, 10)
        };

        var items = new List<TransmittalItemModel>
        {
            new() { DrgNumber = "M-001", DrgRev = "P04", DrgName = "Master Doc", DrgVolume = "M" }
        };

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Master Documents");
        worksheet.Cell(1, 1).Value = "{{DrgNumber}}";
        worksheet.Cell(1, 2).Value = "{{DrgRev}}";
        worksheet.Cell(1, 3).Value = "{{DrgName}}";
        worksheet.Cell(1, 4).Value = "{{TransID}}";
        var templateRange = worksheet.Range(1, 1, 1, 4);

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            templateRange,
            items,
            item => ReportsTestHelpers.InvokeInstancePrivate<Dictionary<string, string>>(
                sut,
                "BuildMasterDocumentContext",
                [typeof(TransmittalItemModel), typeof(TransmittalModel)],
                item,
                transmittal));

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("M-001");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("P04");
        await Assert.That(worksheet.Cell(1, 3).GetString()).IsEqualTo("Master Doc");
        await Assert.That(worksheet.Cell(1, 4).GetString()).IsEqualTo("22");
    }
}

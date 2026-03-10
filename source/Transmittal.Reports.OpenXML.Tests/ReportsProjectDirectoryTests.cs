using ClosedXML.Excel;
using Transmittal.Library.Models;

namespace Transmittal.Reports.OpenXML.Tests;

public class ReportsProjectDirectoryTests
{
    [Test]
    public async Task ProjectDirectoryReport_Rows_RenderExpectedTokenValues()
    {
        var sut = ReportsTestHelpers.CreateSut();

        var projectDirectory = new List<ProjectDirectoryModel>
        {
            new()
            {
                Company = new CompanyModel { ID = 1, CompanyName = "Beta Co", Role = "Architect" },
                Person = new PersonModel { ID = 10, FirstName = "Zoe", LastName = "Taylor", Email = "zoe@beta.test", ShowInReport = true }
            },
            new()
            {
                Company = new CompanyModel { ID = 2, CompanyName = "Acme Co", Role = "Engineer" },
                Person = new PersonModel { ID = 11, FirstName = "Ann", LastName = "Brown", Email = "ann@acme.test", ShowInReport = true }
            }
        };

        var ordered = projectDirectory
            .Where(x => x.Person?.ShowInReport == true)
            .OrderBy(x => x.Person!.LastName)
            .ThenBy(x => x.Person!.FirstName)
            .ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Project Directory");
        worksheet.Cell(1, 1).Value = "{{LastName}}";
        worksheet.Cell(1, 2).Value = "{{FirstName}}";
        worksheet.Cell(1, 3).Value = "{{CompanyName}}";
        worksheet.Cell(1, 4).Value = "{{Email}}";
        var templateRange = worksheet.Range(1, 1, 1, 4);

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            templateRange,
            ordered,
            model => ReportsTestHelpers.InvokeInstancePrivate<Dictionary<string, string>>(
                sut,
                "BuildProjectDirectoryContext",
                [typeof(ProjectDirectoryModel)],
                model));

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("Brown");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("Ann");
        await Assert.That(worksheet.Cell(1, 3).GetString()).IsEqualTo("Acme Co");

        await Assert.That(worksheet.Cell(2, 1).GetString()).IsEqualTo("Taylor");
        await Assert.That(worksheet.Cell(2, 2).GetString()).IsEqualTo("Zoe");
        await Assert.That(worksheet.Cell(2, 3).GetString()).IsEqualTo("Beta Co");
    }

    [Test]
    public async Task ProjectDirectoryReport_Rows_SortUsingTemplateSortTag()
    {
        var sut = ReportsTestHelpers.CreateSut();

        var projectDirectory = new List<ProjectDirectoryModel>
        {
            new()
            {
                Company = new CompanyModel { ID = 1, CompanyName = "Beta Co", Role = "Architect" },
                Person = new PersonModel { ID = 10, FirstName = "Zoe", LastName = "Taylor", Email = "zoe@beta.test", ShowInReport = true }
            },
            new()
            {
                Company = new CompanyModel { ID = 2, CompanyName = "Acme Co", Role = "Engineer" },
                Person = new PersonModel { ID = 11, FirstName = "Ann", LastName = "Brown", Email = "ann@acme.test", ShowInReport = true }
            }
        };

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Project Directory");
        worksheet.Cell(1, 1).Value = "{{LastName}}";
        worksheet.Cell(1, 2).Value = "{{FirstName}}";
        worksheet.Cell(1, 3).Value = "{{CompanyName}}";
        worksheet.Cell(2, 1).Value = "<<sort>>";
        var templateRange = worksheet.Range(1, 1, 1, 3);

        ReportsTestHelpers.InvokePopulateRowsFromNamedRange(
            sut,
            worksheet,
            templateRange,
            projectDirectory,
            model => ReportsTestHelpers.InvokeInstancePrivate<Dictionary<string, string>>(
                sut,
                "BuildProjectDirectoryContext",
                [typeof(ProjectDirectoryModel)],
                model));

        await Assert.That(worksheet.Cell(1, 1).GetString()).IsEqualTo("Brown");
        await Assert.That(worksheet.Cell(1, 2).GetString()).IsEqualTo("Ann");
        await Assert.That(worksheet.Cell(2, 1).GetString()).IsEqualTo("Taylor");
        await Assert.That(worksheet.Cell(2, 2).GetString()).IsEqualTo("Zoe");
    }
}

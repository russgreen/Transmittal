using Transmittal.Enums;
using Transmittal.Library.Extensions;
using Transmittal.Library.Models;
using Transmittal.Library.Services;
using Transmittal.Models;
using Transmittal.Services;
using TUnit.Core;

namespace Transmittal.Revit.Tests;

public class ExportFileCheckServiceTests
{
    private class TestSettingsService : ISettingsService
    {
        public SettingsModel GlobalSettings { get; set; }

        public TestSettingsService(
            string projectNumber = "0001",
            string projectIdentifier = "TEST",
            string projectName = "TestProject",
            string? drawingIssueStore = null,
            string? fileNameFilter = null)
        {
            GlobalSettings = new SettingsModel
            {
                ProjectNumber = projectNumber,
                ProjectIdentifier = projectIdentifier,
                ProjectName = projectName,
                DrawingIssueStore = drawingIssueStore ?? Path.Combine(Path.GetTempPath(), "ExportTests"),
                FileNameFilter = fileNameFilter ?? "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Rev>"
            };
        }

        public void GetSettings() { }
        public void UpdateSettings() { }
    }

    private ExportFileCheckService CreateExportFileCheckService(ISettingsService settingsService)
    {
        return new ExportFileCheckService(settingsService);
    }

    private ISettingsService CreateMockSettingsService(
        string projectNumber = "0001",
        string projectIdentifier = "TEST",
        string projectName = "TestProject",
        string? drawingIssueStore = null,
        string? fileNameFilter = null)
    {
        return new TestSettingsService(projectNumber, projectIdentifier, projectName, drawingIssueStore, fileNameFilter);
    }

    private DrawingSheetModel CreateDrawingSheet(
        string sheetNumber = "0001",
        string sheetName = "TestSheet",
        string volume = "AA",
        string level = "00",
        string type = "DR",
        string role = "A",
        string revision = "P01",
        string originator = "XXX",
        string package = "PKG1",
        string status = "S0",
        string statusDescription = "")
    {
        return new DrawingSheetModel
        {
            ID = new Autodesk.Revit.DB.ElementId(1000),
            DrgNumber = sheetNumber,
            DrgName = sheetName,
            DrgVolume = volume,
            DrgLevel = level,
            DrgType = type,
            DrgRole = role,
            DrgRev = revision,
            DrgOriginator = originator,
            DrgPackage = package,
            DrgStatus = status,
            DrgStatusDescription = statusDescription,
            ExportPDF = true,
            ExportDWG = false,
            ExportDWF = false
        };
    }

    private string BuildExpectedOutputPath(ISettingsService settingsService, DrawingSheetModel sheet, ExportFormatType exportFormatType, string extension)
    {
        var settings = settingsService.GlobalSettings;
        var fileName = settings.FileNameFilter.ParseFilename(
            settings.ProjectNumber,
            settings.ProjectIdentifier,
            settings.ProjectName,
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

        var folderPath = settings.DrawingIssueStore.ParseFolderName(exportFormatType.ToString(), sheet.DrgPackage, sheet.DrgSheetCollection);
        return Path.Combine(folderPath, $"{fileName}{extension}");
    }

    [Test]
    public async Task CheckExportFilesAsync_WithEmptySheetCollection_ReturnsEmptyList()
    {
        var settingsService = CreateMockSettingsService();
        var sut = CreateExportFileCheckService(settingsService);

        var sheets = new List<DrawingSheetModel>();

        var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false, 
            exportPDF: true, exportDWG: false, exportDWF: false);

        await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task CheckExportFilesAsync_WithNonExistentFile_ReportsFalse()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet = CreateDrawingSheet(sheetNumber: "0001", volume: "AA");
        var sheets = new List<DrawingSheetModel> { sheet };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: false, exportDWF: false);

            await Assert.That(results).Count().IsEqualTo(1);
            await Assert.That(results[0].FileExists).IsFalse();
            await Assert.That(results[0].ExportFormat).IsEqualTo(ExportFormatType.PDF);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_WithExistingFile_ReportsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet = CreateDrawingSheet(sheetNumber: "0001", volume: "AA");
        
        // Create the expected file at the exact path the service will check
        var expectedFilePath = BuildExpectedOutputPath(settingsService, sheet, ExportFormatType.PDF, ".pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(expectedFilePath)!);
        File.WriteAllText(expectedFilePath, "test");

        var sheets = new List<DrawingSheetModel> { sheet };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: false, exportDWF: false);

            await Assert.That(results).Count().IsEqualTo(1);
            await Assert.That(results[0].FileExists).IsTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_WithMultipleVolumes_CorrectlyIdentifiesPerVolumeFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        // Create two sheets with different volumes but same package
        var sheetAA = CreateDrawingSheet(
            sheetNumber: "0001",
            sheetName: "TestSheet",
            volume: "AA",
            package: "PKG1");
        
        var sheetZZ = CreateDrawingSheet(
            sheetNumber: "0001",
            sheetName: "TestSheet",
            volume: "ZZ",
            package: "PKG1");

        // Create file only for volume ZZ at the exact path the service will check
        var filePathZZ = BuildExpectedOutputPath(settingsService, sheetZZ, ExportFormatType.PDF, ".pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(filePathZZ)!);
        File.WriteAllText(filePathZZ, "test");

        var sheets = new List<DrawingSheetModel> { sheetAA, sheetZZ };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: false, exportDWF: false);

            await Assert.That(results).Count().IsEqualTo(2);
            
            // Sheet AA should report file does not exist
            var resultAA = results.First(r => r.SheetNumber == "0001" && r.OutputPath.Contains("-AA-"));
            await Assert.That(resultAA.FileExists).IsFalse();
            
            // Sheet ZZ should report file exists
            var resultZZ = results.First(r => r.SheetNumber == "0001" && r.OutputPath.Contains("-ZZ-"));
            await Assert.That(resultZZ.FileExists).IsTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_WithMultipleFormats_ChecksAllFormats()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet = CreateDrawingSheet(sheetNumber: "0001", volume: "AA");
        
        // Create only the PDF file at the exact path the service will check
        var pdfFilePath = BuildExpectedOutputPath(settingsService, sheet, ExportFormatType.PDF, ".pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(pdfFilePath)!);
        File.WriteAllText(pdfFilePath, "test");

        var sheets = new List<DrawingSheetModel> { sheet };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: true, exportDWF: true);

            await Assert.That(results).Count().IsEqualTo(3);
            
            var pdfResult = results.First(r => r.ExportFormat == ExportFormatType.PDF);
            var dwgResult = results.First(r => r.ExportFormat == ExportFormatType.DWG);
            var dwfResult = results.First(r => r.ExportFormat == ExportFormatType.DWF);

            await Assert.That(pdfResult.FileExists).IsTrue();
            await Assert.That(dwgResult.FileExists).IsFalse();
            await Assert.That(dwfResult.FileExists).IsFalse();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_WithPerSheetExportFormats_RespectsSheetsettings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet = CreateDrawingSheet(sheetNumber: "0001", volume: "AA");
        sheet.ExportPDF = true;
        sheet.ExportDWG = false;
        sheet.ExportDWF = false;

        var sheets = new List<DrawingSheetModel> { sheet };

        try
        {
            // When enablePerSheetExportFormats is true, it should use sheet's settings
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: true,
                exportPDF: false, exportDWG: true, exportDWF: true);

            // Should only check PDF because sheet.ExportPDF is true
            await Assert.That(results).Count().IsEqualTo(1);
            await Assert.That(results[0].ExportFormat).IsEqualTo(ExportFormatType.PDF);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_SkipsSheetWhenNoFormatsEnabled()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet = CreateDrawingSheet(sheetNumber: "0001", volume: "AA");
        sheet.ExportPDF = false;
        sheet.ExportDWG = false;
        sheet.ExportDWF = false;

        var sheets = new List<DrawingSheetModel> { sheet };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: true,
                exportPDF: false, exportDWG: false, exportDWF: false);

            // Should return empty because no formats are enabled
            await Assert.That(results).IsEmpty();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_MultipleSheetsWithSameVolumeButDifferentNumbers()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet1 = CreateDrawingSheet(sheetNumber: "0001", sheetName: "Sheet1", volume: "AA");
        var sheet2 = CreateDrawingSheet(sheetNumber: "0002", sheetName: "Sheet2", volume: "AA");

        // Create file for sheet1 only at the exact path the service will check
        var file1Path = BuildExpectedOutputPath(settingsService, sheet1, ExportFormatType.PDF, ".pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(file1Path)!);
        File.WriteAllText(file1Path, "test");

        var sheets = new List<DrawingSheetModel> { sheet1, sheet2 };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: false, exportDWF: false);

            await Assert.That(results).Count().IsEqualTo(2);
            
            var result1 = results.First(r => r.SheetNumber == "0001");
            var result2 = results.First(r => r.SheetNumber == "0002");

            await Assert.That(result1.FileExists).IsTrue();
            await Assert.That(result2.FileExists).IsFalse();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_VerifiesCorrectSheetIdInResults()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet1 = CreateDrawingSheet(sheetNumber: "0001", volume: "AA");
        var sheet2 = CreateDrawingSheet(sheetNumber: "0002", volume: "BB");

        var sheets = new List<DrawingSheetModel> { sheet1, sheet2 };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: false, exportDWF: false);

            await Assert.That(results).Count().IsEqualTo(2);
            
            var result1 = results.First(r => r.SheetNumber == "0001");
            var result2 = results.First(r => r.SheetNumber == "0002");

            // Verify the sheet IDs are correctly mapped
            await Assert.That(result1.SheetId).IsEqualTo(sheet1.ID);
            await Assert.That(result2.SheetId).IsEqualTo(sheet2.ID);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public async Task CheckExportFilesAsync_CorrectlyBuildsOutputPaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        
        var settingsService = CreateMockSettingsService(
            projectNumber: "2024",
            drawingIssueStore: tempDir);
        var sut = CreateExportFileCheckService(settingsService);

        var sheet = CreateDrawingSheet(
            sheetNumber: "0001",
            sheetName: "TestSheet",
            volume: "AA",
            level: "01",
            revision: "P02");

        var sheets = new List<DrawingSheetModel> { sheet };

        try
        {
            var results = await sut.CheckExportFilesAsync(sheets, enablePerSheetExportFormats: false,
                exportPDF: true, exportDWG: false, exportDWF: false);

            await Assert.That(results).Count().IsEqualTo(1);
            
            var result = results[0];
            
            // Verify the output path contains the correct elements
            await Assert.That(result.OutputPath).Contains("2024");  // ProjectNo
            await Assert.That(result.OutputPath).Contains("AA");    // Volume
            await Assert.That(result.OutputPath).Contains("01");    // Level
            await Assert.That(result.OutputPath).Contains("P02");   // Revision
            await Assert.That(result.OutputPath).EndsWith(".pdf");  // Format
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}



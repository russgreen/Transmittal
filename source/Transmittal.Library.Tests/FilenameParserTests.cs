using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmittal.Library.Models;
using Transmittal.Library.Helpers;


namespace Transmittal.Library.Tests;
public class FilenameParserTests
{
    [Test]
    [Arguments("0001-ORI-XX-D01-GA-A-0003-SiteAnalysisPlan-S2-P01.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [Arguments("0001-ORI-XX-D01-GA-A-0003 P01 SiteAnalysisPlan.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev> <SheetName>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [Arguments("0001-ORI-XX-D01-GA-A-0003 P01 Site Analysis Plan.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev> <SheetName2>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [Arguments("PROJ-ORI-XX-00-GA-A-0003", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>", "PROJ", "ORI", "A", "XX", "00", "GA", "0003", "S2", "", "", "")]
    [Arguments("PROJ-ORI-XX-00-GA-A-0003-C02", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<Rev>", "PROJ", "ORI", "A", "XX", "00", "GA", "0003", "S2", "", "C02", "")]
    //[InlineData("0001-ORI-XX-D01-GA-A-0003.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "", "", "", "")]
    [Arguments("PROJ-ORI-XX-00-GA-A-0003-SheetName-ProjectName-S1-C02", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<ProjName>-<Status>-<Rev>", "PROJ", "ORI", "A", "XX", "00", "GA", "0003", "S1", "", "C02", "Sheet Name")]
    [Arguments("0001-ORI-XX-D01-GA-A-0003-SiteAnalysisPlan-S2-P01.pdf", "<ProjNo>-ORI-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [Arguments("P00 000-OR-ZZ-ZZ-DR-A-0601-P02_Planning Detail 01 - Edge of Slab, Brick Facade.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<Rev>_<SheetName2>", "P00 000", "OR", "A", "ZZ", "ZZ", "DR", "0601", "", "", "P02", "Planning Detail 01 - Edge of Slab, Brick Facade")]
    [Arguments("P00 000-OR-ZZ-ZZ-DR-A-06-601-P02_Planning Detail 01 - Edge of Slab, Brick Facade.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<Rev>_<SheetName2>", "P00 000", "OR", "A", "ZZ", "ZZ", "DR", "06-601", "", "", "P02", "Planning Detail 01 - Edge of Slab, Brick Facade")]
    [Arguments("P00 000-OR-ZZ-ZZ-DR-A-06-601 P02_Planning Detail 01 - Edge of Slab, Brick Facade.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev>_<SheetName2>", "P00 000", "OR", "A", "ZZ", "ZZ", "DR", "06-601", "", "", "P02", "Planning Detail 01 - Edge of Slab, Brick Facade")]
    [Arguments("PROJ-OR-ZZ-ZZ-DR-A-06-601 P02-S1_PlanningDetail01.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev>-<Status>_<SheetName>", "PROJ", "OR", "A", "ZZ", "ZZ", "DR", "06-601", "S1", "", "P02", "Planning Detail 01")]
    public async Task DocumentModel_ShouldReturnExpectedResult(string filePath, 
        string exportRule, 
        string projectNumber,
        string originator,
        string role,
        string expectedVolume, 
        string expectedLevel, 
        string expectedType,
        string expectedNumber,
        string expectedStatus, 
        string expectedStatusDescription,
        string expectedRev,
        string expectedName)
    {
        // Act
        DocumentModel document = FilenameParser.GetDocumentModel(filePath, projectNumber, originator, role, exportRule);

        // Assert
        if(exportRule.Contains("<Volume>"))
        {
            await Assert.That(document.DrgVolume).IsEqualTo(expectedVolume);
        }

        if (exportRule.Contains("<Level>"))
        {
            await Assert.That(document.DrgLevel).IsEqualTo(expectedLevel);
        }

        if (exportRule.Contains("<Type>"))
        {
            await Assert.That(document.DrgType).IsEqualTo(expectedType);
        }

        if (exportRule.Contains("<SheetNo>"))
        {
            await Assert.That(document.DrgNumber).IsEqualTo(expectedNumber);
        }

        if (exportRule.Contains("<Status>"))
        {
            await Assert.That(document.DrgStatus).IsEqualTo(expectedStatus);
        }

        if (exportRule.Contains("<StatusDescription>"))
        {
            await Assert.That(document.DrgStatusDescription).IsEqualTo(expectedStatusDescription);
        }

        if (exportRule.Contains("<Rev>"))
        {
            await Assert.That(document.DrgRev).IsEqualTo(expectedRev);
        }

        if (exportRule.Contains("<SheetName>"))
        {
            await Assert.That(document.DrgName.ToLower()).IsEqualTo(expectedName.ToLower());
        }

        if (exportRule.Contains("<SheetName2>"))
        {
            await Assert.That(document.DrgName.ToLower()).IsEqualTo(expectedName.ToLower());
        }
    }
}

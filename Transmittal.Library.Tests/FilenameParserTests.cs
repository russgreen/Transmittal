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
    [Theory]
    [InlineData("0001-ORI-XX-D01-GA-A-0003-SiteAnalysisPlan-S2-P01.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [InlineData("0001-ORI-XX-D01-GA-A-0003 P01 SiteAnalysisPlan.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev> <SheetName>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [InlineData("0001-ORI-XX-D01-GA-A-0003 P01 Site Analysis Plan.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev> <SheetName2>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [InlineData("PROJ-ORI-XX-00-GA-A-0003", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>", "PROJ", "ORI", "A", "XX", "00", "GA", "0003", "S2", "", "", "")]
    [InlineData("PROJ-ORI-XX-00-GA-A-0003-C02", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<Rev>", "PROJ", "ORI", "A", "XX", "00", "GA", "0003", "S2", "", "C02", "")]
    //[InlineData("0001-ORI-XX-D01-GA-A-0003.pdf", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "ORI", "A", "XX", "D01", "GA", "0003", "S2", "", "P01", "Site Analysis Plan")]
    [InlineData("PROJ-ORI-XX-00-GA-A-0003-SheetName-ProjectName-S1-C02", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<ProjName>-<Status>-<Rev>", "PROJ", "ORI", "A", "XX", "00", "GA", "0003", "S1", "", "C02", "Sheet Name")]
    public void DocumentModel_ShouldReturnExpectedResult(string filePath, 
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
            Assert.Equal(expectedVolume, document.DrgVolume);

        if(exportRule.Contains("<Level>"))
            Assert.Equal(expectedLevel, document.DrgLevel);

        if (exportRule.Contains("<Type>")) 
            Assert.Equal(expectedType, document.DrgType);

        if (exportRule.Contains("<SheetNo>")) 
            Assert.Equal(expectedNumber, document.DrgNumber);

        if (exportRule.Contains("<Status>")) 
            Assert.Equal(expectedStatus, document.DrgStatus);

        if (exportRule.Contains("<StatusDescription>"))
            Assert.Equal(expectedStatusDescription, document.DrgStatusDescription);

        if (exportRule.Contains("<Rev>")) 
            Assert.Equal(expectedRev, document.DrgRev);

        if (exportRule.Contains("<SheetName>")) 
            Assert.Equal(expectedName.ToLower(), document.DrgName.ToLower());

        if (exportRule.Contains("<SheetName2>")) 
            Assert.Equal(expectedName.ToLower(), document.DrgName.ToLower());  
   
    }
}

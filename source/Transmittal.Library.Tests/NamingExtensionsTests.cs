using Humanizer;
using System;
using Transmittal.Library.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Transmittal.Library.Tests;
public class NamingExtensionsTests
{
    //[Theory]
    //[InlineData(@"C:\Users\PDF\2023\02\01", @"C:\Users\<Format>\<DateYYYY>\<DateMM>\<DateDD>", "PDF", 2023, 02, 01)]
    [Test]
    public async Task ParseFolderName_ShouldReplaceTagsInFolderPath() //string expected, string path, string format, int year, int month, int day)
    {
        // Arrange
        var path = @"C:\Users\<Format>\<SheetCollection>\<Package>\<DateYYYY>\<DateMM>\<DateDD>";
        var format = "PDF";
        var package = "package name";
        var sheetCollection = "sheet collection";

        // Act
        var result = path.ParseFolderName(format, package, sheetCollection);

        // Assert
        await Assert.That(result).Contains(format);

        await Assert.That(result).Contains(package);
        await Assert.That(result).Contains(sheetCollection);

        await Assert.That(result).Contains(DateTime.Now.Year.ToString());
        await Assert.That(result).Contains(DateTime.Now.ToString("MM"));
        await Assert.That(result).Contains(DateTime.Now.ToString("dd"));
    }


    [Test]
    [Arguments(@"C:\Users\PDF\package name", @"C:\Users\<Format>\<Package>", "PDF", "package name", "sheet collection")]
    [Arguments(@"C:\Users\package name\PDF", @"C:\Users\<Package>\<Format>", "PDF", "package name", "")]
    [Arguments(@"C:\Users\PDF\sheet collection", @"C:\Users\<Format>\<SheetCollection>", "PDF", "package name", "sheet collection")]
    [Arguments(@"C:\Users\CDE", @"C:\Users\<Package>\<Format>", "CDE", "", "")]
    [Arguments(@"C:\Users\PDF\<DateYYYY>\<DateYY><DateMM><DateDD>", @"C:\Users\<Format>\<DateYYYY>\<DateYY><DateMM><DateDD>", "PDF", "package name", "sheet collection")]
    public async Task ParseFolderName_ShouldReplaceTagsInFolderPath2(string expected, string path, string format, string package, string sheetCollection) //string expected, string path, string format, int year, int month, int day)
    {
        // Arrange
        expected = expected.Replace("<DateYYYY>", DateTime.Now.Year.ToString());
        expected = expected.Replace("<DateYY>", DateTime.Now.ToString("YY"));
        expected = expected.Replace("<DateMM>", DateTime.Now.ToString("MM"));
        expected = expected.Replace("<DateDD>", DateTime.Now.ToString("dd"));


        path = path.Replace("<DateYYYY>", DateTime.Now.Year.ToString());
        path = path.Replace("<DateYY>", DateTime.Now.ToString("YY"));
        path = path.Replace("<DateMM>", DateTime.Now.ToString("MM"));
        path = path.Replace("<DateDD>", DateTime.Now.ToString("dd"));

        // Act
        var result = path.ParseFolderName(format, package, sheetCollection);

        // Assert
        await Assert.That(result).IsEqualTo(expected); 
    }

    [Test]
    public async Task ParsePathWithEnvironmentVariables_ShouldReplaceEnvironmentVariableTagsInPath()
    {
        // Arrange
        var path = @"%UserProfile%\Documents\<Format>";
        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "<Format>");

        // Act
        var result = path.ParsePathWithEnvironmentVariables();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("12345_L1_Dr_A_A101_FloorPlan_P01", "<ProjNo>_<Level>_<Type>_<Role>_<SheetNo>_<SheetName>_<Rev>", "12345", "", "", "", "", "L1", "Dr", "A", "A101", "FloorPlan", "P01", "", "") ]
    [Arguments("0001-ORI-XX-XX-GA-A-0001 P01 FloorPlan", "<ProjNo>-ORI-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev> <SheetName>", "0001", "", "", "ORI", "XX", "XX", "GA", "A", "0001", "Floor Plan", "P01", "", "")]
    [Arguments("12345-L1-Dr-A-A101-FloorPlan-P01", "<ProjNo>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Rev>", "12345", "", "", "", "", "L1", "Dr", "A", "A101", "FloorPlan", "P01", "", "")]
    [Arguments("0001-ORI-ZZ-XX-TL-A-0001-TransmittalRecord", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "", "", "ORI", "ZZ", "XX", "TL", "A", "0001", "Transmittal Record", "", "", "")]
    [Arguments("0001-ORI-ZZ-XX-DR-A-0001-DrawingSheet-S3-P01", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName>-<Status>-<Rev>", "0001", "", "", "ORI", "ZZ", "XX", "DR", "A", "0001", "Drawing Sheet", "P01", "S3", "")]
    [Arguments("0001-ORI-ZZ-XX-DR-A-0001-Drawing Sheet-S3-P01", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo>-<SheetName2>-<Status>-<Rev>", "0001", "", "", "ORI", "ZZ", "XX", "DR", "A", "0001", "Drawing Sheet", "P01", "S3", "")]
    [Arguments("0001-ORI-ZZ-XX-DR-A-0001 P01_Drawing Sheet", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev>_<SheetName2>", "0001", "", "", "ORI", "ZZ", "XX", "DR", "A", "0001", "Drawing Sheet", "P01", "S3", "")]
    [Arguments("0001-ORI-ZZ-XX-TL-A-0001 TransmittalRecord", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev>_<SheetName>", "0001", "", "", "ORI", "ZZ", "XX", "TL", "A", "0001", "Transmittal Record", "", "", "")]
    [Arguments("0001-ORI-ZZ-XX-DR-A-0001 Drawing Sheet", "<ProjNo>-<Originator>-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev>_<SheetName2>", "0001", "", "", "ORI", "ZZ", "XX", "DR", "A", "0001", "Drawing Sheet", "", "", "")]
    public async Task ParseFilename_ShouldReplaceTagsInFilename(string expected, string filenameFilter, string projNo, string projID, string projName, string originator, 
        string volume, string level, string type, string role, string sheetNo, string sheetName, string rev, string status, string statusDescription)
    {
        // Arrange

        // Act
        var result = filenameFilter.ParseFilename(projNo, projID, projName, originator, volume, level, type, role, sheetNo, sheetName, rev, status, statusDescription);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task RemoveIllegalCharacters_ShouldRemoveIllegalCharactersFromInputString()
    {
        // Arrange
        var illegalString = @"\/:*?<>|";

        // Act
        var result = illegalString.RemoveIllegalCharacters();

        // Assert
        await Assert.That(result).IsEqualTo("  -     ");
    }

    [Test]
    [Arguments("12345_L1_Dr_A_A101_FloorPlan", "12345_L1_Dr_A_A101_FloorPlan___")]
    [Arguments("12345_L1_Dr_A_A101_FloorPlan", "12345_L1_Dr_A_A101_FloorPlan--")]
    [Arguments("12345_L1_Dr_A_A101_FloorPlan", "12345_L1_Dr_A_A101_FloorPlan *")]
    [Arguments("12345_L1_Dr_A_A101_FloorPlan", "12345_L1_Dr_A_A101_FloorPlan >")]
    [Arguments("12345_L1_Dr_A_A101_FloorPlan", "12345_L1_Dr_A_A101_FloorPlan | ")]
    public async Task RemoveTrailingSymbols_ShouldRemoveTrailingSymbolsFromInputString(string expected, string input)
    {
        // Arrange

        // Act
        var result = input.RemoveTrailingSymbols();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }
}

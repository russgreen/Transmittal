﻿using Humanizer;
using System;
using Transmittal.Library.Extensions;


namespace Transmittal.Library.Tests;
public class NamingExtensionsTests
{
    //[Theory]
    //[InlineData(@"C:\Users\PDF\2023\02\01", @"C:\Users\<Format>\<DateYYYY>\<DateMM>\<DateDD>", "PDF", 2023, 02, 01)]
    [Fact]
    public void ParseFolderName_ShouldReplaceTagsInFolderPath() //string expected, string path, string format, int year, int month, int day)
    {
        // Arrange
        var path = @"C:\Users\<Format>\<DateYYYY>\<DateMM>\<DateDD>";
        var format = "PDF";

        // Act
        var result = path.ParseFolderName(format);

        // Assert
        Assert.Contains(format, result);
        Assert.Contains(DateTime.Now.Year.ToString(), result);
        Assert.Contains(DateTime.Now.ToString("MM"), result);
        Assert.Contains(DateTime.Now.ToString("dd"), result);
    }

    [Fact]
    public void ParsePathWithEnvironmentVariables_ShouldReplaceEnvironmentVariableTagsInPath()
    {
        // Arrange
        var path = @"%UserProfile%\Documents\<Format>";
        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "<Format>");

        // Act
        var result = path.ParsePathWithEnvironmentVariables();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("12345_L1_Dr_A_A101_FloorPlan_P01", "<ProjNo>_<Level>_<Type>_<Role>_<SheetNo>_<SheetName>_<Rev>", "12345", "", "", "", "", "L1", "Dr", "A", "A101", "FloorPlan", "P01", "", "") ]
    [InlineData("0001-ORI-XX-XX-GA-A-0001 P01 FloorPlan", "<ProjNo>-ORI-<Volume>-<Level>-<Type>-<Role>-<SheetNo> <Rev> <SheetName>", "0001", "", "", "ORI", "XX", "XX", "GA", "A", "0001", "FloorPlan", "P01", "", "")]
    public void ParseFilename_ShouldReplaceTagsInFilename(string expected, string filenameFilter, string projNo, string projID, string projName, string originator, 
        string volume, string level, string type, string role, string sheetNo, string sheetName, string rev, string status, string statusDescription)
    {
        // Arrange

        // Act
        var result = filenameFilter.ParseFilename(projNo, projID, projName, originator, volume, level, type, role, sheetNo, sheetName, rev, status, statusDescription);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveIllegalCharacters_ShouldRemoveIllegalCharactersFromInputString()
    {
        // Arrange
        var illegalString = @"\/:*?<>|";

        // Act
        var result = illegalString.RemoveIllegalCharacters();

        // Assert
        Assert.Equal("  -     ", result);
    }
}

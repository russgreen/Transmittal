using Transmittal.Library.Extensions;

namespace Transmittal.Library.Tests;
public class DateExtensionsTests
{
    [Theory]
    [InlineData("23.03.18", 2023, 3, 18)]
    [InlineData("10.10.10", 2010, 10, 10)]
    public void ToStringYYMMDDEx_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringYYMMDDEx();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("230318", 2023, 3, 18)]
    [InlineData("101010", 2010, 10, 10)]
    public void ToStringYYMMDD_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringYYMMDD();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("23", 2023, 3, 18)]
    [InlineData("10", 2010, 10, 10)]
    public void ToStringYY_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringYY();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("03", 2023, 3, 18)]
    [InlineData("10", 2010, 10, 10)]
    public void ToStringMM_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringMM();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("18", 2023, 3, 18)]
    [InlineData("01", 2010, 10, 1)]
    public void ToStringDD_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringDD();

        // Assert
        Assert.Equal(expected, result);
    }

}

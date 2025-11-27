using Transmittal.Library.Extensions;

namespace Transmittal.Library.Tests;
public class DateExtensionsTests
{
    [Test]
    [Arguments("23.03.18", 2023, 3, 18)]
    [Arguments("10.10.10", 2010, 10, 10)]
    public async Task ToStringYYMMDDEx_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringYYMMDDEx();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("230318", 2023, 3, 18)]
    [Arguments("101010", 2010, 10, 10)]
    public async Task ToStringYYMMDD_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringYYMMDD();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("23", 2023, 3, 18)]
    [Arguments("10", 2010, 10, 10)]
    public async Task ToStringYY_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringYY();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("03", 2023, 3, 18)]
    [Arguments("10", 2010, 10, 10)]
    public async Task ToStringMM_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringMM();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("18", 2023, 3, 18)]
    [Arguments("01", 2010, 10, 1)]
    public async Task ToStringDD_ShouldReturnFormattedString(string expected, int year, int month, int day)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.ToStringDD();

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

}

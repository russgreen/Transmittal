namespace Transmittal.Library.Tests;

public class CollectionsExtensionsTests
{
    [Fact]
    public void GetValueOrDefault_ShouldReturnDefault_WhenKeyNotFound()
    {
        // Arrange
        var dictionary = new Dictionary<string, string>();

        // Act
        var result = dictionary.GetValueOrDefault("foo", "bar");

        // Assert
        Assert.Equal("bar", result);
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDictionaryValue_WhenKeyFound()
    {
        // Arrange
        var dictionary = new Dictionary<string, string> { { "foo", "bar" } };

        // Act
        var result = dictionary.GetValueOrDefault("foo", "default");

        // Assert
        Assert.Equal("bar", result);
    }
}
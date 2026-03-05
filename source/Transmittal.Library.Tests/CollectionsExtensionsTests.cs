namespace Transmittal.Library.Tests;

public class CollectionsExtensionsTests
{
    [Test]
    public async Task GetValueOrDefault_ShouldReturnDefault_WhenKeyNotFound()
    {
        // Arrange
        var dictionary = new Dictionary<string, string>();

        // Act
        var result = dictionary.GetValueOrDefault("foo", "bar");

        // Assert
        await Assert.That(result).IsEqualTo("bar");
    }

    [Test]
    public async Task GetValueOrDefault_ShouldReturnDictionaryValue_WhenKeyFound()
    {
        // Arrange
        var dictionary = new Dictionary<string, string> { { "foo", "bar" } };

        // Act
        var result = dictionary.GetValueOrDefault("foo", "default");

        // Assert
        await Assert.That(result).IsEqualTo("bar");
    }
}
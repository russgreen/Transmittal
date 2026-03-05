using Transmittal.Library.Extensions;


namespace Transmittal.Library.Tests;
public class ModelExtensionsTests
{
    [Test]
    public async Task CopyPropertiesTo_ShouldCopyPropertiesFromOneObjectToAnother()
    {
        // Arrange
        var fromObject = new { FirstName = "John", LastName = "Doe", Age = 30 };
        var toObject = new Person();

        // Act
        fromObject.CopyPropertiesTo(toObject);

        // Assert
        await Assert.That(toObject.FirstName).IsEqualTo(fromObject.FirstName);
        await Assert.That(toObject.LastName).IsEqualTo(fromObject.LastName);
        await Assert.That(toObject.Age).IsEqualTo(fromObject.Age);
    }


    private class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
    }
}



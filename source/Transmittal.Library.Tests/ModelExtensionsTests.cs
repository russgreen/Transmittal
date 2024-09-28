using Transmittal.Library.Extensions;


namespace Transmittal.Library.Tests;
public class ModelExtensionsTests
{
    [Fact]
    public void CopyPropertiesTo_ShouldCopyPropertiesFromOneObjectToAnother()
    {
        // Arrange
        var fromObject = new { FirstName = "John", LastName = "Doe", Age = 30 };
        var toObject = new Person();

        // Act
        fromObject.CopyPropertiesTo(toObject);

        // Assert
        Assert.Equal(fromObject.FirstName, toObject.FirstName);
        Assert.Equal(fromObject.LastName, toObject.LastName);
        Assert.Equal(fromObject.Age, toObject.Age);
    }


    private class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
    }
}



using MovieCategories.Domain;
using NUnit.Framework;

namespace MovieCategories.Tests;

[TestFixture]
public class CustomExceptionTests
{
    [Test]
    public void Constructor_WithMessage_ShouldSetMessageProperty()
    {
        // Arrange
        const string message = "Test exception";

        // Act
        var exception = new CustomException(message);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
    }

    [Test]
    public void Constructor_WithMessageAndCode_ShouldSetCodeProperty()
    {
        // Arrange
        const string message = "Test exception";
        const int code = 404;

        // Act
        var exception = new CustomException(message, code);

        // Assert
        Assert.That(exception.Code, Is.EqualTo(code));
    }

    [Test]
    public void Constructor_WithMessageAndDetails_ShouldSetDetailsProperty()
    {
        // Arrange
        const string message = "Test exception";
        const string details = "Test details";

        // Act
        var exception = new CustomException(message, details: details);

        // Assert
        Assert.That(exception.Details, Is.EqualTo(details));
    }

    [Test]
    public void Constructor_WithMessageCodeAndDetails_ShouldSetProperties()
    {
        // Arrange
        const string message = "Test exception";
        const int code = 404;
        const string details = "Test details";

        // Act
        var exception = new CustomException(message, code, details);

        // Assert
        Assert.That(exception.Code, Is.EqualTo(code));
        Assert.That(exception.Details, Is.EqualTo(details));
    }

    [Test]
    public void Constructor_WithMessageAndInnerException_ShouldSetInnerExceptionProperty()
    {
        // Arrange
        const string message = "Test exception";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new CustomException(message, innerException);

        // Assert
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void ToString_ShouldIncludeCodeAndDetails()
    {
        // Arrange
        var message = "Test exception";
        const int code = 404;
        const string details = "Test details";

        // Act
        var exception = new CustomException(message, code, details);
        var result = exception.ToString();

        // Assert
        Assert.That(result, Does.Contain($"Code: {code}"));
        Assert.That(result, Does.Contain($"Details: {details}"));
    }
}
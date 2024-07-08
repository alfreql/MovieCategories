using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using MovieCategories.Api.Middleware;
using MovieCategories.Domain;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

namespace MovieCategories.Tests;

[TestFixture]
public class ExceptionMiddlewareTests
{
    private Mock<RequestDelegate> _nextMock;
    private Mock<ILogger<ExceptionMiddleware>> _loggerMock;
    private Mock<IHostEnvironment> _envMock;
    private ExceptionMiddleware _middleware;

    [SetUp]
    public void SetUp()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        _envMock = new Mock<IHostEnvironment>();
        _middleware = new ExceptionMiddleware(_nextMock.Object, _loggerMock.Object, _envMock.Object);
    }

    [Test]
    public void Constructor_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ExceptionMiddleware(null!, _loggerMock.Object, _envMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("next"));
    }

    [Test]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ExceptionMiddleware(_nextMock.Object, null!, _envMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public void Constructor_WithNullEnvironment_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ExceptionMiddleware(_nextMock.Object, _loggerMock.Object, null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("env"));
    }

    [Test]
    public async Task InvokeAsync_WhenNoException_ShouldCallNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldHandleException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Test exception");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(exception);
        SetEnvironmentAsDevelopment(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        VerifyLogger();

        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/json"));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var expectedResponse = new
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = "An unexpected error occurred.",
            Detailed = "Test exception"
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        Assert.That(responseBody, Is.EqualTo(jsonResponse));
    }

    [Test]
    public async Task InvokeAsync_WhenCustomExceptionThrown_ShouldHandleCustomException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new CustomException("Custom exception", 400, "Custom details");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(exception);
        SetEnvironmentAsDevelopment(true);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        VerifyLogger();

        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/json"));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var expectedResponse = new
        {
            StatusCode = 400,
            Message = "Custom exception",
            Detailed = "Custom details"
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        Assert.That(responseBody, Is.EqualTo(jsonResponse));
    }

    [Test]
    public async Task InvokeAsync_WhenCustomExceptionThrownInProduction_ShouldHandleCustomExceptionWithoutDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new CustomException("Custom exception", 400, "Custom details");
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(exception);
        SetEnvironmentAsDevelopment(false);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        VerifyLogger();

        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/json"));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var expectedResponse = new
        {
            StatusCode = 400,
            Message = "Custom exception",
            Detailed = string.Empty
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        Assert.That(responseBody, Is.EqualTo(jsonResponse));
    }

    private void SetEnvironmentAsDevelopment(bool isDevelopment)
    {
        _envMock.Setup(env => env.EnvironmentName).Returns(isDevelopment ? "Development" : "Production");
    }

    private void VerifyLogger()
    {
        _loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception: ")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
    }
}
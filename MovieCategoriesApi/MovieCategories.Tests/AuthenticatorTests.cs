using Moq;
using Moq.Protected;
using MovieCategories.Domain;
using MovieCategories.Infrastructure.Auth;
using NUnit.Framework;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MovieCategories.Tests;

public class AuthenticatorTests
{
    private WireMockServer _server;
    private HttpClient _httpClient;
    private Authenticator _authenticator;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _server = WireMockServer.Start();
        _httpClient = new HttpClient { BaseAddress = new Uri(_server.Urls[0]) };
        _authenticator = new Authenticator(_httpClient);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _server.Stop();
        _server.Dispose();
    }

    [TearDown]
    public void TearDown()
    {
        _server.Reset();
    }

    [Test]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Authenticator(null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("httpClient"));
    }

    [Test]
    public async Task AuthenticateAsync_ShouldReturnTokenResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            Token = "dummy-token",
            ExpireTime = DateTime.UtcNow.AddHours(1)
        };
        _server.Given(Request.Create()
                .WithPath("/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithBody(JsonSerializer.Serialize(tokenResponse)));

        // Act
        var result = await _authenticator.AuthenticateAsync("user@example.com", "password123");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Token, Is.EqualTo("dummy-token"));
        Assert.That(result.ExpireTime, Is.EqualTo(tokenResponse.ExpireTime).Within(1).Seconds);
    }

    [Test]
    public void AuthenticateAsync_ShouldThrowCustomException_WhenResponseIsBadRequest()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/token")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.BadRequest));

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException>(async () => await _authenticator.AuthenticateAsync("user@example.com", "password123"))!;
        Assert.That(ex.Code, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }

    [Test]
    public void AuthenticateAsync_ShouldThrowCustomException_WhenResponseIsInternalServerError()
    {
        // Arrange
        _server.Given(Request.Create()
                .WithPath("/token")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.InternalServerError));

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException>(async () => await _authenticator.AuthenticateAsync("user@example.com", "password123"))!;
        Assert.That(ex.Message, Contains.Substring("Error consuming API"));
    }

    [Test]
    public void AuthenticateAsync_ShouldThrowCustomException_WhenHttpRequestExceptionOccurs()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Request failed"));

        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        var authenticator = new Authenticator(httpClient);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException>(async () => await authenticator.AuthenticateAsync("user@example.com", "password123"))!;
        Assert.That(ex.Message, Contains.Substring("Error consuming API"));
        Assert.That(ex.InnerException, Is.TypeOf<HttpRequestException>());
    }
}
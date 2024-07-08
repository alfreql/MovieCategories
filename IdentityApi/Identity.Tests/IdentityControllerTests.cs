using Identity.Api.Controllers;
using Identity.Api.Dto;
using Identity.Application.Interfaces;
using Identity.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Net.Http.Json;

namespace Identity.Tests;

public class IdentityControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private Mock<ITokenGenerator> _tokenGeneratorMock;
    private Mock<IApplicationUserService> _userServiceMock;

    [SetUp]
    public void Setup()
    {
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _userServiceMock = new Mock<IApplicationUserService>();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_tokenGeneratorMock.Object);
                    services.AddSingleton(_userServiceMock.Object);
                });
            });

        _client = _factory.CreateClient();
    }

    [Test]
    public void Constructor_WithNullTokenGenerator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new IdentityController(null!, _userServiceMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("tokenGenerator"));
    }

    [Test]
    public void Constructor_WithNullUserService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new IdentityController(_tokenGeneratorMock.Object, null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("userService"));
    }

    [Test]
    public async Task CreateUser_ShouldReturnUserId_WhenUserIsCreated()
    {
        // Arrange
        var userRequest = new CreateUserRequest { Email = "test@example.com", Password = "password" };
        _userServiceMock.Setup(service => service.CreateAsync(userRequest.Email, userRequest.Password))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/Users", userRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var userId = await response.Content.ReadFromJsonAsync<int>();
        Assert.That(userId, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateUser_ShouldReturnBadRequest_WhenEmailIsRequired()
    {
        // Arrange
        var userRequest = new CreateUserRequest { Email = "", Password = "password" };

        // Act
        var response = await _client.PostAsJsonAsync("/Users", userRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Email is required."));
    }

    [Test]
    public async Task CreateUser_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var userRequest = new CreateUserRequest { Email = "sssss", Password = "password" };

        // Act
        var response = await _client.PostAsJsonAsync("/Users", userRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Invalid email format."));
    }

    [Test]
    public async Task CreateUser_ShouldReturnBadRequest_WhenPasswordIsRequired()
    {
        // Arrange
        var userRequest = new CreateUserRequest { Email = "email@emai.com", Password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/Users", userRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Password is required."));
    }

    [Test]
    public async Task CreateUser_ThrowsException()
    {
        // Arrange
        var userRequest = new CreateUserRequest { Email = "email@emai.com", Password = "Password" };
        _userServiceMock.Setup(service => service.CreateAsync(userRequest.Email, userRequest.Password)).ThrowsAsync(new Exception("Error creating User."));

        // Act
        var response = await _client.PostAsJsonAsync("/Users", userRequest);
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        Assert.That(responseBody!.StatusCode, Is.EqualTo((int)System.Net.HttpStatusCode.InternalServerError));
        Assert.That(responseBody.Message, Contains.Substring("An unexpected error occurred."));
        Assert.That(responseBody.Detailed, Contains.Substring("Error creating User."));
    }

    [Test]
    public async Task CreateUser_ThrowsCustomException()
    {
        // Arrange
        var userRequest = new CreateUserRequest { Email = "email@emai.com", Password = "Password" };
        _userServiceMock.Setup(service => service.CreateAsync(userRequest.Email, userRequest.Password))
                            .ThrowsAsync(new CustomException("Error creating User.", 515, "Details Info."));


        // Act
        var response = await _client.PostAsJsonAsync("/Users", userRequest);
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        // Assert
        Assert.That((int)response.StatusCode, Is.EqualTo(515));
        Assert.That(responseBody!.StatusCode, Is.EqualTo(515));
        Assert.That(responseBody.Message, Contains.Substring("Error creating User."));
        Assert.That(responseBody.Detailed, Contains.Substring("Details Info."));
    }

    [Test]
    public async Task CreateToken_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var createTokenRequest = new CreateTokenRequest { Email = "test@example.com", Password = "password" };
        var token = ("testToken", DateTime.UtcNow.AddHours(1));

        _tokenGeneratorMock.Setup(generator => generator.GenerateTokenAsync(createTokenRequest.Email, createTokenRequest.Password))
            .ReturnsAsync(token);

        // Act
        var response = await _client.PostAsJsonAsync("/token", createTokenRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.That(result!.Token, Is.EqualTo("testToken"));
        Assert.That(result.ExpireTime, Is.EqualTo(token.Item2));
    }

    [Test]
    public async Task CreateToken_ThrowsCustomException_WhenCredentialsAreInValid()
    {
        // Arrange
        var createTokenRequest = new CreateTokenRequest { Email = "email@email.com", Password = "Password" };
        _tokenGeneratorMock.Setup(generator => generator.GenerateTokenAsync(createTokenRequest.Email, createTokenRequest.Password))
            .ThrowsAsync(new CustomException("Wrong User or Password", 515, "Details Info."));

        // Act
        var response = await _client.PostAsJsonAsync("/token", createTokenRequest);
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        // Assert
        Assert.That((int)response.StatusCode, Is.EqualTo(515));
        Assert.That(responseBody!.StatusCode, Is.EqualTo(515));
        Assert.That(responseBody.Message, Contains.Substring("Wrong User or Password"));
        Assert.That(responseBody.Detailed, Contains.Substring("Details Info."));
    }

    [Test]
    public async Task CreateToken_ShouldReturnBadRequest_WhenEmailIsRequired()
    {
        // Arrange
        var createTokenRequest = new CreateTokenRequest { Email = "", Password = "password" };

        // Act
        var response = await _client.PostAsJsonAsync("/token", createTokenRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Email is required."));
    }

    [Test]
    public async Task CreateToken_ShouldReturnBadRequest_WhenEmailInvalid()
    {
        // Arrange
        var createTokenRequest = new CreateTokenRequest { Email = "sss", Password = "password" };

        // Act
        var response = await _client.PostAsJsonAsync("/token", createTokenRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Invalid email format."));
    }
    [Test]
    public async Task CreateToken_ShouldReturnBadRequest_WhenPasswordIsRequired()
    {
        // Arrange
        var createTokenRequest = new CreateTokenRequest { Email = "email@email.com", Password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/token", createTokenRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Password is required."));
    }

    [Test]
    public async Task CreateToken_ThrowsException()
    {
        // Arrange
        var createTokenRequest = new CreateTokenRequest { Email = "email@email.com", Password = "Password" };

        _tokenGeneratorMock.Setup(generator => generator.GenerateTokenAsync(createTokenRequest.Email, createTokenRequest.Password))
            .ThrowsAsync(new Exception("Error creating Token."));

        // Act
        var response = await _client.PostAsJsonAsync("/token", createTokenRequest);
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        Assert.That(responseBody!.StatusCode, Is.EqualTo((int)System.Net.HttpStatusCode.InternalServerError));
        Assert.That(responseBody.Message, Contains.Substring("An unexpected error occurred."));
        Assert.That(responseBody.Detailed, Contains.Substring("Error creating Token."));
    }


    private record ErrorResponse(int StatusCode, string Message, string Detailed);
    private record TokenResponse(string Token, DateTime ExpireTime);
}
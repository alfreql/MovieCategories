using AutoMapper;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieCategories.Api.Controllers;
using MovieCategories.Api.Dto;
using MovieCategories.Application.Interfaces;
using MovieCategories.Domain;
using MovieCategories.Infrastructure.Auth;
using NUnit.Framework;
using System.Net.Http.Json;

namespace MovieCategories.Tests;

public class MovieCategoriesControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private Mock<ICategoryService> _serviceMock;
    private Mock<IAuthenticator> _authenticatorMock;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<ICategoryService>();
        _authenticatorMock = new Mock<IAuthenticator>();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IPolicyEvaluator, FakePolicyEvaluator>();
                    services.AddSingleton<ICategoryService>(_serviceMock.Object);
                    services.AddSingleton<IAuthenticator>(_authenticatorMock.Object);
                });
            });
    }

    [Test]
    public void Constructor_WithNullService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new MovieCategoriesController(null!, new Mock<IMapper>().Object, _authenticatorMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("service"));
    }

    [Test]
    public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new MovieCategoriesController(_serviceMock.Object, null!, _authenticatorMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("mapper"));
    }

    [Test]
    public void Constructor_WithNullAuthenticator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new MovieCategoriesController(_serviceMock.Object, new Mock<IMapper>().Object, null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("authenticator"));
    }

    [Test]
    public async Task Get_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<MovieCategory>
        {
            new() { Id = 1, Category = "Action" },
            new() { Id = 2, Category = "Comedy" }
        };

        _serviceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(categories);

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/MoviesCategories");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var responseCategories = await response.Content.ReadFromJsonAsync<IEnumerable<MovieCategory>>();
        Assert.That(responseCategories!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Get_ReturnsCategoryById()
    {
        // Arrange
        var category = new MovieCategory { Id = 1, Category = "Action" };

        _serviceMock.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(category);

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/MoviesCategories/1");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var responseCategory = await response.Content.ReadFromJsonAsync<MovieCategory>();
        Assert.That(responseCategory!.Id, Is.EqualTo(category.Id));
    }

    [Test]
    public async Task Get_ReturnsNotFound_WhenCategoryNotFound()
    {
        // Arrange
        _serviceMock.Setup(service => service.GetByIdAsync(1)).ReturnsAsync((MovieCategory)null!);

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/MoviesCategories/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_CreatesNewCategory()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = "Horror", Description = "Scary movies" };

        _serviceMock.Setup(service => service.CreateAsync(It.IsAny<MovieCategory>())).ReturnsAsync(3);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/MoviesCategories", categoryRequest);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var newId = await response.Content.ReadFromJsonAsync<int>();
        Assert.That(newId, Is.EqualTo(3));
        _serviceMock.Verify(service => service.CreateAsync(It.IsAny<MovieCategory>()), Times.Once);

    }

    [Test]
    public async Task Post_ReturnsBadRequest_WhenCategoryIsEmpty()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = "", Description = "No category" };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/MoviesCategories", categoryRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        Assert.That(responseBody, Contains.Substring("Category is required."));
    }

    [Test]
    public async Task Post_ThrowsException()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = "Horror", Description = "Scary movies" };
        _serviceMock.Setup(service => service.CreateAsync(It.IsAny<MovieCategory>())).ThrowsAsync(new Exception("Error creating category"));

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/MoviesCategories", categoryRequest);
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
        Assert.That(responseBody!.StatusCode, Is.EqualTo((int)System.Net.HttpStatusCode.InternalServerError));
        Assert.That(responseBody.Message, Contains.Substring("An unexpected error occurred."));
        Assert.That(responseBody.Detailed, Contains.Substring("Error creating category"));
    }

    [Test]
    public async Task Post_ThrowsCustomException()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = "Horror", Description = "Scary movies" };
        _serviceMock.Setup(service => service.CreateAsync(It.IsAny<MovieCategory>())).ThrowsAsync(new CustomException("Error creating category", 515, "Details Info."));

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/MoviesCategories", categoryRequest);
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.That((int)response.StatusCode, Is.EqualTo(515));
        Assert.That(responseBody!.StatusCode, Is.EqualTo(515));
        Assert.That(responseBody.Message, Contains.Substring("Error creating category"));
        Assert.That(responseBody.Detailed, Contains.Substring("Details Info."));
    }

    [Test]
    public async Task Put_UpdatesCategory()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = "Horror", Description = "Scary movies" };
        var category = new MovieCategory { Id = 1, Category = "Action", Description = "Action movies" };

        _serviceMock.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(category);
        _serviceMock.Setup(service => service.UpdateAsync(It.IsAny<MovieCategory>())).Returns(Task.FromResult(1));

        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/MoviesCategories/1", categoryRequest);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        _serviceMock.Verify(service => service.UpdateAsync(It.Is<MovieCategory>(c => c.Category == "Horror" && c.Description == "Scary movies")));
    }

    [Test]
    public async Task Put_ReturnsBadRequest_WhenCategoryIsNull()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = null!, Description = "No category" };

        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/MoviesCategories/1", categoryRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Put_ReturnsNotFound_WhenCategoryNotFound()
    {
        // Arrange
        var categoryRequest = new CreateMovieCategoryRequest { Category = "Horror", Description = "Scary movies" };

        _serviceMock.Setup(service => service.GetByIdAsync(1)).ReturnsAsync((MovieCategory)null!);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("/api/MoviesCategories/1", categoryRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_RemovesCategory()
    {
        // Arrange
        var category = new MovieCategory { Id = 1, Category = "Action" };

        _serviceMock.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(category);
        _serviceMock.Setup(service => service.DeleteAsync(1)).Returns(Task.CompletedTask);

        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/MoviesCategories/1");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        _serviceMock.Verify(service => service.DeleteAsync(1), Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsNotFound_WhenCategoryNotFound()
    {
        // Arrange
        _serviceMock.Setup(service => service.GetByIdAsync(1)).ReturnsAsync((MovieCategory)null!);

        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/api/MoviesCategories/1");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        _serviceMock.Verify(service => service.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetAllHttpAuth_ShouldReturnUnauthorized_WhenCredentialsHeaderAreMissing()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/MoviesCategories/GetAllHttpAuth");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetAllHttpAuth_ShouldReturnUnauthorized_WhenAuthenticationFails()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/MoviesCategories/GetAllHttpAuth");
        request.Headers.Add("email", "wrong@example.com");
        request.Headers.Add("password", "wrongpassword");
        var client = _factory.CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetAllHttpAuth_ShouldReturnOk_WhenAuthenticationSucceeds()
    {
        // Arrange
        _authenticatorMock.Setup(a => a.AuthenticateAsync("user@example.com", "password123"))
            .ReturnsAsync(new TokenResponse { Token = "dummy-token", ExpireTime = DateTime.UtcNow.AddHours(1) });

        var categories = new List<MovieCategory>
        {
            new() { Id = 1, Category = "Action" },
            new() { Id = 2, Category = "Comedy" }
        };

        _serviceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(categories);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/MoviesCategories/GetAllHttpAuth");
        request.Headers.Add("email", "user@example.com");
        request.Headers.Add("password", "password123");
        var client = _factory.CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        var responseCategories = await response.Content.ReadFromJsonAsync<IEnumerable<MovieCategory>>();
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(responseCategories!.Count(), Is.EqualTo(2));
    }
}

record ErrorResponse(int StatusCode, string Message, string Detailed);

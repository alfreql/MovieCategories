using Moq;
using MovieCategories.Application.Category;
using MovieCategories.Application.Interfaces;
using MovieCategories.Domain;
using NUnit.Framework;

namespace MovieCategories.Tests;

[TestFixture]
public class CategoryServiceTests
{
    private Mock<ICategoryRepo> _repositoryMock;
    private CategoryService _categoryService;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<ICategoryRepo>();
        _categoryService = new CategoryService(_repositoryMock.Object);
    }

    [Test]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new CategoryService(null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("repository"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<MovieCategory>
        {
            new () { Id = 1, Category = "Action", Description = "Action movie"},
            new () { Id = 2, Category = "Comedy" }
        };
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(categories.Count));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var category = new MovieCategory { Id = 1, Category = "Action", Description = "Action movie" };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.EqualTo(category));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MovieCategory)null!);

        // Act
        var result = await _categoryService.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void CreateAsync_ThrowsCustomException_WhenCategoryAlreadyExists()
    {
        // Arrange
        var category = new MovieCategory { Category = "Action" };
        _repositoryMock.Setup(repo => repo.GetByNameAsync("Action")).ReturnsAsync(category);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException>(() => _categoryService.CreateAsync(category))!;
        Assert.That(ex.Message, Is.EqualTo("Category 'Action' already exist"));
        Assert.That(ex.Code, Is.EqualTo(409));
    }

    [Test]
    public async Task CreateAsync_CreatesCategory_WhenCategoryDoesNotExist()
    {
        // Arrange
        var category = new MovieCategory { Category = "Action" };
        _repositoryMock.Setup(repo => repo.GetByNameAsync("Action")).ReturnsAsync((MovieCategory)null!);
        _repositoryMock.Setup(repo => repo.CreateAsync(category)).ReturnsAsync(1);

        // Act
        var result = await _categoryService.CreateAsync(category);

        // Assert
        Assert.That(result, Is.EqualTo(1));
        _repositoryMock.Verify(repo => repo.CreateAsync(category), Times.Once);
    }

    [Test]
    public void UpdateAsync_ThrowsCustomException_WhenCategoryAlreadyExistsWithDifferentId()
    {
        // Arrange
        var existingCategory = new MovieCategory { Id = 2, Category = "Action" };
        var category = new MovieCategory { Id = 1, Category = "Action" };
        _repositoryMock.Setup(repo => repo.GetByNameAsync("Action")).ReturnsAsync(existingCategory);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException>(() => _categoryService.UpdateAsync(category))!;
        Assert.That(ex.Message, Is.EqualTo("Category 'Action' already exist"));
        Assert.That(ex.Code, Is.EqualTo(409));
    }

    [Test]
    public async Task UpdateAsync_UpdatesCategory_WhenCategoryNameExists()
    {
        // Arrange
        var category = new MovieCategory { Id = 1, Category = "Action" };
        _repositoryMock.Setup(repo => repo.GetByNameAsync("Action")).ReturnsAsync(category);

        // Act
        await _categoryService.UpdateAsync(category);

        // Assert
        _repositoryMock.Verify(repo => repo.UpdateAsync(category), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_UpdatesCategory_WhenCategoryNameNonExists()
    {
        // Arrange
        var category = new MovieCategory { Id = 1, Category = "Action" };
        _repositoryMock.Setup(repo => repo.GetByNameAsync("Action")).ReturnsAsync((MovieCategory)null!);

        // Act
        await _categoryService.UpdateAsync(category);

        // Assert
        _repositoryMock.Verify(repo => repo.UpdateAsync(category), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ShouldCallDeleteOnRepository()
    {
        // Arrange
        var categoryId = 1;

        // Act
        await _categoryService.DeleteAsync(categoryId);

        // Assert
        _repositoryMock.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
    }
}
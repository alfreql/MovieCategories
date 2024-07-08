using Microsoft.Data.Sqlite;
using MovieCategories.Domain;
using MovieCategories.Infrastructure;
using NUnit.Framework;
using System.Data;
using System.Data.Common;

namespace MovieCategories.Tests;

public class CategoryRepoTests
{
    private DbConnection _connection;
    private CategoryRepo _repository;
    private const string DATABASE_FILE_PATH = "test_database.sqlite";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        if (File.Exists(DATABASE_FILE_PATH))
        {
            File.Delete(DATABASE_FILE_PATH);
        }
    }

    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection($"Data Source={DATABASE_FILE_PATH}");
        _connection.Open();

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MovieCategory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Category TEXT NOT NULL,
                        Description TEXT
                    );";
            cmd.ExecuteNonQuery();
        }
        _connection.Close();
        _repository = new CategoryRepo(_connection);
    }

    [TearDown]
    public void TearDown()
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "DROP TABLE MovieCategory;";
            cmd.ExecuteNonQuery();
        }
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Constructor_WithNullConnection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new CategoryRepo(null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("connection"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsListOfCategories()
    {
        // Arrange
        await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<IEnumerable<MovieCategory>>());
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Category, Is.EqualTo("Action"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsCategory()
    {
        // Arrange
        var newId = await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();

        // Act
        var result = await _repository.GetByIdAsync(newId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<MovieCategory>());
        Assert.That(result!.Id, Is.EqualTo(newId));
        Assert.That(result.Category, Is.EqualTo("Action"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull()
    {
        // Arrange
        await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();

        // Act
        var result = await _repository.GetByIdAsync(100);

        // Assert
        Assert.That(result, Is.Null);
    }


    [Test]
    public async Task GetByNameAsync_ReturnsCategory()
    {
        // Arrange
        var category = new MovieCategory { Category = "Action", Description = "Action movies" };
        await _repository.CreateAsync(category);
        await _connection.CloseAsync();

        // Act
        var result = await _repository.GetByNameAsync("Action");

        // Assert
        Assert.That(result!.Category, Is.EqualTo(category.Category));
        Assert.That(result.Description, Is.EqualTo(category.Description));
    }

    [Test]
    public async Task GetByNameAsync_ReturnsNull()
    {
        // Arrange
        await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();

        // Act
        var result = await _repository.GetByNameAsync("NonExistingCategory");

        // Assert
        Assert.That(result, Is.Null);
    }


    [Test]
    public async Task CreateAsync_InsertsNewCategory()
    {
        // Arrange
        var category = new MovieCategory { Category = "Action", Description = "Action movies" };

        // Act
        var newId = await _repository.CreateAsync(category);

        // Assert
        Assert.That(newId, Is.GreaterThan(0));

        // Verify insertion
        var insertedCategory = await _repository.GetByIdAsync(newId);
        Assert.That(insertedCategory, Is.Not.Null);
        Assert.That(insertedCategory!.Category, Is.EqualTo("Action"));
    }

    [Test]
    public async Task UpdateAsync_UpdatesExistingCategory()
    {
        // Arrange
        var newId = await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();
        var categoryToUpdate = new MovieCategory { Id = newId, Category = "Updated Action", Description = "Updated description" };

        // Act
        var result = await _repository.UpdateAsync(categoryToUpdate);
        Assert.That(result, Is.EqualTo(1));

        // Assert
        var updatedCategory = await _repository.GetByIdAsync(newId);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(updatedCategory, Is.Not.Null);
        Assert.That(updatedCategory!.Category, Is.EqualTo("Updated Action"));
        Assert.That(updatedCategory.Description, Is.EqualTo("Updated description"));
    }

    [Test]
    public async Task UpdateAsync_NonUpdateNonExistingCategory()
    {
        // Arrange
        await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();
        var categoryToUpdate = new MovieCategory { Id = 2, Category = "Updated Action", Description = "Updated description" };

        // Act
        var result = await _repository.UpdateAsync(categoryToUpdate);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteAsync_DeletesCategory()
    {
        // Arrange
        var newId = await _repository.CreateAsync(new MovieCategory { Category = "Action", Description = "Action movies" });
        await _connection.CloseAsync();

        // Act
        await _repository.DeleteAsync(newId);

        // Assert
        var deletedCategory = await _repository.GetByIdAsync(newId);
        Assert.That(deletedCategory, Is.Null);
    }
}
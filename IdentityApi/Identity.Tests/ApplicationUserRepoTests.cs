using Identity.Application.Interfaces;
using Identity.Domain;
using Identity.Infrastructure;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System.Data;

namespace Identity.Tests;

[TestFixture]
public class ApplicationUserRepoTests
{
    private IDbConnection _connection;
    private IApplicationUserRepo _repo;
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

        using var command = _connection.CreateCommand();
        command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ApplicationUser (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Salt TEXT NOT NULL);";
        command.ExecuteNonQuery();

        _connection.Close();
        _repo = new ApplicationUserRepo(_connection);
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
            cmd.CommandText = "DROP TABLE ApplicationUser;";
            cmd.ExecuteNonQuery();
        }
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Constructor_WithNullArg_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ApplicationUserRepo(null!));
        Assert.That(ex!.ParamName, Is.EqualTo("connection"));
    }

    [Test]
    public async Task SaveAsync_ShouldInsertUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            PasswordHash = "passwordHash",
            Salt = "salt"
        };

        // Act
        var userId = await _repo.SaveAsync(user);

        // Assert
        Assert.That(userId, Is.GreaterThan(0));
        var insertedUser = await _repo.FirstOrDefaultAsync(user.Email);
        Assert.That(insertedUser, Is.Not.Null);
        Assert.That(insertedUser!.Email, Is.EqualTo(user.Email));
        Assert.That(insertedUser.PasswordHash, Is.EqualTo(user.PasswordHash));
        Assert.That(insertedUser.Salt, Is.EqualTo(user.Salt));
    }

    [Test]
    public async Task FirstOrDefaultAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Act
        var user = await _repo.FirstOrDefaultAsync("nonexistent@example.com");

        // Assert
        Assert.That(user, Is.Null);
    }

    [Test]
    public async Task FirstOrDefaultAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Email = "test2@example.com",
            PasswordHash = "passwordHash2",
            Salt = "salt2"
        };

        await _repo.SaveAsync(user);

        // Act
        var fetchedUser = await _repo.FirstOrDefaultAsync(user.Email);

        // Assert
        Assert.That(fetchedUser, Is.Not.Null);
        Assert.That(fetchedUser!.Email, Is.EqualTo(user.Email));
        Assert.That(fetchedUser.PasswordHash, Is.EqualTo(user.PasswordHash));
        Assert.That(fetchedUser.Salt, Is.EqualTo(user.Salt));
    }
}
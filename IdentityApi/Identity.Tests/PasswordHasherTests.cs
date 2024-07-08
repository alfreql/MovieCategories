using NUnit.Framework;
using Identity.Infrastructure;
using Identity.Application.User;

namespace Identity.Tests;

public class PasswordHasherTests
{
    private PasswordHasher _passwordHasher;

    [SetUp]
    public void SetUp()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Test]
    public void HashPassword_ShouldReturnNonNullHashedPasswordAndSalt()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        var result = _passwordHasher.HashPassword(password);

        // Assert
        Assert.That(result.hashedPassword, Is.Not.Null.And.Not.Empty);
        Assert.That(result.salt, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void HashPassword_ShouldReturnSaltWithExpectedLength()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        var result = _passwordHasher.HashPassword(password);
        var saltBytes = Convert.FromBase64String(result.salt);

        // Assert
        Assert.That(saltBytes.Length, Is.EqualTo(128 / 8));
    }

    [Test]
    public void VerifyPassword_ShouldReturnTrueForMatchingPassword()
    {
        // Arrange
        string password = "TestPassword123!";
        var hashResult = _passwordHasher.HashPassword(password);

        // Act
        bool isMatch = _passwordHasher.VerifyPassword(hashResult.hashedPassword, hashResult.salt, password);

        // Assert
        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void VerifyPassword_ShouldReturnFalseForNonMatchingPassword()
    {
        // Arrange
        string password = "TestPassword123!";
        string wrongPassword = "WrongPassword!";
        var hashResult = _passwordHasher.HashPassword(password);

        // Act
        bool isMatch = _passwordHasher.VerifyPassword(hashResult.hashedPassword, hashResult.salt, wrongPassword);

        // Assert
        Assert.That(isMatch, Is.False);
    }
}
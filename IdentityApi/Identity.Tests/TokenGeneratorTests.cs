using Identity.Application.Interfaces;
using Identity.Domain;
using Identity.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Identity.Tests;

public class TokenGeneratorTests
{
    private Mock<IApplicationUserRepo> _userRepoMock;
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<IConfiguration> _configurationMock;
    private TokenGenerator _tokenGenerator;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IApplicationUserRepo>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.SetupGet(c => c["Jwt:Key"]).Returns("Do_Not_Store_Key_In_Here_123456789_0123456789");
        _configurationMock.SetupGet(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.SetupGet(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configurationMock.SetupGet(c => c["Jwt:TokenLifeTimeHours"]).Returns("1");

        _tokenGenerator = new TokenGenerator(_userRepoMock.Object, _passwordHasherMock.Object, _configurationMock.Object);
    }

    [Test]
    public void Constructor_WithNullUserRepo_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new TokenGenerator(null!, _passwordHasherMock.Object, _configurationMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("userRepo"));
    }

    [Test]
    public void Constructor_WithNullPasswordHasher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new TokenGenerator(_userRepoMock.Object, null!, _configurationMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("passwordHasher"));
    }

    [Test]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new TokenGenerator(_userRepoMock.Object, _passwordHasherMock.Object, null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("configuration"));
    }

    [Test]
    public void GenerateTokenAsync_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "password";

        _userRepoMock.Setup(repo => repo.FirstOrDefaultAsync(email)).ReturnsAsync((ApplicationUser)null!);

        // Act & Assert
        var exception = Assert.ThrowsAsync<CustomException>(async () => await _tokenGenerator.GenerateTokenAsync(email, password))!;
        Assert.That(exception.Code, Is.EqualTo((int)HttpStatusCode.Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("Wrong User or Password"));
    }

    [Test]
    public void GenerateTokenAsync_ShouldThrowException_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "wrongPassword";
        var user = new ApplicationUser { Email = email, PasswordHash = "hashedPassword", Salt = "salt" };

        _userRepoMock.Setup(repo => repo.FirstOrDefaultAsync(email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(user.PasswordHash, user.Salt, password)).Returns(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<CustomException>(async () => await _tokenGenerator.GenerateTokenAsync(email, password))!;
        Assert.That(exception.Code, Is.EqualTo((int)HttpStatusCode.Unauthorized));
        Assert.That(exception.Message, Is.EqualTo("Wrong User or Password"));
    }

    [Test]
    public async Task GenerateTokenAsync_ShouldReturnTokenAndExpiration_WhenCredentialsAreCorrect()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "password";
        var user = new ApplicationUser { Id = 5, Email = email, PasswordHash = "hashedPassword", Salt = "salt" };

        _userRepoMock.Setup(repo => repo.FirstOrDefaultAsync(email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(user.PasswordHash, user.Salt, password)).Returns(true);

        // Act
        var result = await _tokenGenerator.GenerateTokenAsync(email, password);

        // Assert
        Assert.That(result.token, Is.Not.Null.And.Not.Empty);
        Assert.That(result.expire, Is.GreaterThan(DateTime.Now.AddMinutes(50)));

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.token);

        Assert.That(token, Is.Not.Null);
        Assert.That(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(email));
        Assert.That(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value, Is.EqualTo(email));
        Assert.That(token.Claims.First(c => c.Type == "userId").Value, Is.EqualTo(user.Id.ToString()));
    }
}
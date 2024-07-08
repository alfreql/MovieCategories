using Identity.Application.Interfaces;
using Identity.Application.User;
using Identity.Domain;
using Moq;
using NUnit.Framework;

namespace Identity.Tests;

public class ApplicationUserServiceTests
{
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<IApplicationUserRepo> _userRepoMock;
    private IApplicationUserService _userService;

    [SetUp]
    public void Setup()
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _userRepoMock = new Mock<IApplicationUserRepo>();

        _userService = new ApplicationUserService(_passwordHasherMock.Object, _userRepoMock.Object);
    }

    [Test]
    public void Constructor_WithNullPasswordHasher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ApplicationUserService(null!, _userRepoMock.Object))!;
        Assert.That(ex.ParamName, Is.EqualTo("passwordHasher"));
    }
    
    [Test]
    public void Constructor_WithNullUserRepo_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ApplicationUserService(_passwordHasherMock.Object, null!))!;
        Assert.That(ex.ParamName, Is.EqualTo("userRepo"));
    }

    [Test]
    public async Task CreateAsync_ShouldReturnUserId_WhenUserIsCreated()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "password";
        const string hashedPassword = "hashedPassword";
        const string salt = "salt";
        const int userId = 1;

        _passwordHasherMock.Setup(ph => ph.HashPassword(password)).Returns((hashedPassword, salt));
        _userRepoMock.Setup(repo => repo.SaveAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(userId);

        // Act
        var result = await _userService.CreateAsync(email, password);

        // Assert
        Assert.That(result, Is.EqualTo(userId));
        _passwordHasherMock.Verify(ph => ph.HashPassword(password), Times.Once);
        _userRepoMock.Verify(repo => repo.SaveAsync(It.Is<ApplicationUser>(u =>
            u.Email == email &&
            u.PasswordHash == hashedPassword &&
            u.Salt == salt)), Times.Once);
    }

    [Test]
    public void CreateAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        const string email = "existing@example.com";
        const string password = "password";
        var existingUser = new ApplicationUser { Email = email };

        _userRepoMock.Setup(repo => repo.FirstOrDefaultAsync(email)).ReturnsAsync(existingUser);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException>(async () => await _userService.CreateAsync(email, password))!;
        Assert.That(ex.Message, Is.EqualTo("Email already in use."));
        Assert.That(ex.Code, Is.EqualTo(409));
        _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<string>()), Times.Never);
        _userRepoMock.Verify(repo => repo.SaveAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
}
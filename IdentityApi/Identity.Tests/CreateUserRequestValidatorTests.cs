using FluentValidation.TestHelper;
using Identity.Api.Dto;
using NUnit.Framework;

namespace Identity.Tests;

[TestFixture]
public class CreateUserRequestValidatorTests
{
    private CreateUserRequestValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateUserRequestValidator();
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        var model = new CreateUserRequest { Email = null!, Password = "password123" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required.");
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var model = new CreateUserRequest { Email = string.Empty, Password = "password123" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required.");
    }

    [Test]
    public void Should_Have_Error_When_Email_Is_Invalid_Format()
    {
        var model = new CreateUserRequest { Email = "invalid-email-format", Password = "password123" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format.");
    }

    [Test]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        var model = new CreateUserRequest { Email = "test@example.com", Password = "password123" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Should_Have_Error_When_Password_Is_Null()
    {
        var model = new CreateUserRequest { Email = "test@example.com", Password = null! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required.");
    }

    [Test]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var model = new CreateUserRequest { Email = "test@example.com", Password = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required.");
    }

    [Test]
    public void Should_Not_Have_Error_When_Password_Is_Provided()
    {
        var model = new CreateUserRequest { Email = "test@example.com", Password = "password123" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
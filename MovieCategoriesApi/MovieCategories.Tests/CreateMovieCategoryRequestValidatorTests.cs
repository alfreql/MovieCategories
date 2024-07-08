using FluentValidation.TestHelper;
using MovieCategories.Api.Dto;
using NUnit.Framework;

namespace MovieCategories.Tests;

public class CreateMovieCategoryRequestValidatorTests
{
    private CreateMovieCategoryRequestValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateMovieCategoryRequestValidator();
    }

    [Test]
    public void Should_Have_Error_When_Category_Is_Null()
    {
        var model = new CreateMovieCategoryRequest { Category = null! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Category).WithErrorMessage("Category is required.");
    }

    [Test]
    public void Should_Have_Error_When_Category_Is_Empty()
    {
        var model = new CreateMovieCategoryRequest { Category = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Category).WithErrorMessage("Category is required.");
    }

    [Test]
    public void Should_Not_Have_Error_When_Category_Is_Not_Empty()
    {
        var model = new CreateMovieCategoryRequest { Category = "Action" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }
}
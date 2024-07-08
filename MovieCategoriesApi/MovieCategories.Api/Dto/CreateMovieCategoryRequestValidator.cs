using FluentValidation;

namespace MovieCategories.Api.Dto;

public class CreateMovieCategoryRequestValidator : AbstractValidator<CreateMovieCategoryRequest>
{
    public CreateMovieCategoryRequestValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.");

    }
}
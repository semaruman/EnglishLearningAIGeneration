using FluentValidation;

namespace EnglishLearning.Application.Features.Words.Commands;

public class AddWordByTextCommandValidator : AbstractValidator<AddWordByTextCommand>
{
    public AddWordByTextCommandValidator()
    {
        RuleFor(x => x.WordText)
            .NotEmpty().WithMessage("Word text is required.")
            .MaximumLength(100).WithMessage("Word text must not exceed 100 characters.");
    }
}

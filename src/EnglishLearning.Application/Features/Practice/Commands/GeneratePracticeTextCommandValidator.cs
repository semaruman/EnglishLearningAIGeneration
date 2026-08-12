using FluentValidation;

namespace EnglishLearning.Application.Features.Practice.Commands;

public class GeneratePracticeTextCommandValidator : AbstractValidator<GeneratePracticeTextCommand>
{
    public GeneratePracticeTextCommandValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(200).WithMessage("Topic must not exceed 200 characters.");

        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty is required.")
            .Must(d => new[] { "Easy", "Medium", "Hard", "A1", "A2", "B1", "B2", "C1", "C2" }
                .Contains(d, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Difficulty must be Easy, Medium, Hard, or a CEFR level.");

        RuleFor(x => x.Length)
            .NotEmpty().WithMessage("Length is required.")
            .Must(l => new[] { "Short", "Medium", "Long" }.Contains(l, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Length must be Short, Medium, or Long.");
    }
}

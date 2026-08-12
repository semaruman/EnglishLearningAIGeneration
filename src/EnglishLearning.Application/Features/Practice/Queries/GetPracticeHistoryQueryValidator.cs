using FluentValidation;

namespace EnglishLearning.Application.Features.Practice.Queries;

public class GetPracticeHistoryQueryValidator : AbstractValidator<GetPracticeHistoryQuery>
{
    public GetPracticeHistoryQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

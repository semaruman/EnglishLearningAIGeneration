using FluentValidation;

namespace EnglishLearning.Application.Features.Words.Queries;

public class SearchWordsQueryValidator : AbstractValidator<SearchWordsQuery>
{
    public SearchWordsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

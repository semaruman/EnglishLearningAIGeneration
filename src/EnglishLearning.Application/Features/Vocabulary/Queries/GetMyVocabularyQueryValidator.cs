using FluentValidation;

namespace EnglishLearning.Application.Features.Vocabulary.Queries;

public class GetMyVocabularyQueryValidator : AbstractValidator<GetMyVocabularyQuery>
{
    public GetMyVocabularyQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

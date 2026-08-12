using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.WordSets.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.WordSets.Queries;

public class GetWordSetsQueryHandler : IRequestHandler<GetWordSetsQuery, IReadOnlyList<WordSetDto>>
{
    private readonly IWordSetRepository _wordSetRepository;

    public GetWordSetsQueryHandler(IWordSetRepository wordSetRepository)
    {
        _wordSetRepository = wordSetRepository;
    }

    public async Task<IReadOnlyList<WordSetDto>> Handle(GetWordSetsQuery request, CancellationToken cancellationToken)
    {
        var sets = await _wordSetRepository.GetAllAsync(cancellationToken);
        return sets.Select(s => s.ToDto()).ToList();
    }
}

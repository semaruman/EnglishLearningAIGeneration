using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Words.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Words.Queries;

public class SearchWordsQueryHandler : IRequestHandler<SearchWordsQuery, PagedResult<WordDto>>
{
    private readonly IWordRepository _wordRepository;

    public SearchWordsQueryHandler(IWordRepository wordRepository)
    {
        _wordRepository = wordRepository;
    }

    public async Task<PagedResult<WordDto>> Handle(SearchWordsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _wordRepository.SearchAsync(
            request.Search,
            request.PartOfSpeech,
            request.Difficulty,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<WordDto>
        {
            Items = items.Select(w => w.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

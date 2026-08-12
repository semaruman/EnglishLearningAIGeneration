using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Queries;

public class GetMyVocabularyQueryHandler : IRequestHandler<GetMyVocabularyQuery, PagedResult<VocabularyWordDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserWordRepository _userWordRepository;

    public GetMyVocabularyQueryHandler(ICurrentUserService currentUser, IUserWordRepository userWordRepository)
    {
        _currentUser = currentUser;
        _userWordRepository = userWordRepository;
    }

    public async Task<PagedResult<VocabularyWordDto>> Handle(
        GetMyVocabularyQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var (items, totalCount) = await _userWordRepository.GetByUserAsync(
            userId,
            request.Status,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<VocabularyWordDto>
        {
            Items = items.Select(uw => uw.ToVocabularyDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

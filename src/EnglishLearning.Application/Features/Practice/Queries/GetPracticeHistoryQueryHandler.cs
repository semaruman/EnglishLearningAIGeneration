using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Practice.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Practice.Queries;

public class GetPracticeHistoryQueryHandler
    : IRequestHandler<GetPracticeHistoryQuery, PagedResult<PracticeSessionDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPracticeSessionRepository _practiceSessionRepository;

    public GetPracticeHistoryQueryHandler(
        ICurrentUserService currentUser,
        IPracticeSessionRepository practiceSessionRepository)
    {
        _currentUser = currentUser;
        _practiceSessionRepository = practiceSessionRepository;
    }

    public async Task<PagedResult<PracticeSessionDto>> Handle(
        GetPracticeHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var (items, totalCount) = await _practiceSessionRepository.GetByUserAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new PagedResult<PracticeSessionDto>
        {
            Items = items.Select(s => s.ToDto()).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

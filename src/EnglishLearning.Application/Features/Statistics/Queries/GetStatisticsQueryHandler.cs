using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.Statistics.DTOs;
using EnglishLearning.Domain.Enums;
using MediatR;

namespace EnglishLearning.Application.Features.Statistics.Queries;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, StatisticsDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserWordRepository _userWordRepository;
    private readonly IPracticeSessionRepository _practiceSessionRepository;

    public GetStatisticsQueryHandler(
        ICurrentUserService currentUser,
        IUserWordRepository userWordRepository,
        IPracticeSessionRepository practiceSessionRepository)
    {
        _currentUser = currentUser;
        _userWordRepository = userWordRepository;
        _practiceSessionRepository = practiceSessionRepository;
    }

    public async Task<StatisticsDto> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var byStatus = await _userWordRepository.CountByStatusAsync(userId, cancellationToken);
        var total = byStatus.Values.Sum();
        var due = await _userWordRepository.GetDueForReviewAsync(userId, 10_000, cancellationToken);
        var practiceCount = await _practiceSessionRepository.CountByUserAsync(userId, cancellationToken);
        var dueCount = due.Count;

        return new StatisticsDto(
            TotalWords: total,
            NewWords: byStatus.GetValueOrDefault(WordStatus.New, 0),
            LearningWords: byStatus.GetValueOrDefault(WordStatus.Learning, 0),
            KnownWords: byStatus.GetValueOrDefault(WordStatus.Known, 0),
            MasteredWords: byStatus.GetValueOrDefault(WordStatus.Mastered, 0),
            WordsReviewedToday: dueCount,
            PracticeSessions: practiceCount,
            DueForReviewCount: dueCount);
    }
}

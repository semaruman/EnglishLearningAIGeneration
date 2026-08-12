using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.Learning.DTOs;
using EnglishLearning.Domain.Entities;
using MediatR;

namespace EnglishLearning.Application.Features.Learning.Queries;

public class GetNextLearningCardQueryHandler : IRequestHandler<GetNextLearningCardQuery, LearningCardDto?>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserWordRepository _userWordRepository;
    private readonly IWordRepository _wordRepository;
    private readonly ILearningSessionRepository _learningSessionRepository;

    public GetNextLearningCardQueryHandler(
        ICurrentUserService currentUser,
        IUserWordRepository userWordRepository,
        IWordRepository wordRepository,
        ILearningSessionRepository learningSessionRepository)
    {
        _currentUser = currentUser;
        _userWordRepository = userWordRepository;
        _wordRepository = wordRepository;
        _learningSessionRepository = learningSessionRepository;
    }

    public async Task<LearningCardDto?> Handle(GetNextLearningCardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var sessionId = await ResolveSessionIdAsync(userId, request.SessionId, cancellationToken);

        var due = await _userWordRepository.GetDueForReviewAsync(userId, 1, cancellationToken);
        if (due.Count > 0)
        {
            return MapUserWordCard(due[0], sessionId);
        }

        var weak = await _userWordRepository.GetWeakWordsAsync(userId, 1, cancellationToken);
        if (weak.Count > 0)
        {
            return MapUserWordCard(weak[0], sessionId);
        }

        var discover = await _wordRepository.GetRandomWordsNotInUserVocabularyAsync(userId, 1, cancellationToken);
        if (discover.Count == 0)
        {
            return null;
        }

        var word = discover[0];
        return new LearningCardDto(
            word.Id,
            word.WordText,
            word.PartOfSpeech,
            word.Definition,
            word.Translation,
            word.Pronunciation,
            word.Phonetic,
            word.ExampleSentence,
            word.DifficultyLevel,
            IsInVocabulary: false,
            Status: null,
            KnowledgeLevel: null,
            SessionId: sessionId);
    }

    private async Task<Guid?> ResolveSessionIdAsync(
        string userId,
        Guid? sessionId,
        CancellationToken cancellationToken)
    {
        if (sessionId.HasValue)
        {
            var session = await _learningSessionRepository.GetByIdAsync(sessionId.Value, cancellationToken);
            if (session is not null && session.UserId == userId && session.CompletedAt is null)
            {
                return session.Id;
            }
        }

        var active = await _learningSessionRepository.GetActiveAsync(userId, cancellationToken);
        return active?.Id;
    }

    private static LearningCardDto MapUserWordCard(UserWord userWord, Guid? sessionId)
    {
        var word = userWord.Word
            ?? throw new InvalidOperationException("UserWord.Word navigation must be loaded.");

        return new LearningCardDto(
            word.Id,
            word.WordText,
            word.PartOfSpeech,
            word.Definition,
            word.Translation,
            word.Pronunciation,
            word.Phonetic,
            word.ExampleSentence,
            word.DifficultyLevel,
            IsInVocabulary: true,
            Status: userWord.Status,
            KnowledgeLevel: userWord.KnowledgeLevel,
            SessionId: sessionId);
    }
}

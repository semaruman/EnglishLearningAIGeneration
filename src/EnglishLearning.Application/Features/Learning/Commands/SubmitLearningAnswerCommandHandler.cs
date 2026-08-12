using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.Learning.DTOs;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;
using MediatR;

namespace EnglishLearning.Application.Features.Learning.Commands;

public class SubmitLearningAnswerCommandHandler
    : IRequestHandler<SubmitLearningAnswerCommand, SubmitLearningAnswerResultDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWordRepository _wordRepository;
    private readonly IUserWordRepository _userWordRepository;
    private readonly ILearningSessionRepository _learningSessionRepository;
    private readonly IReviewScheduler _reviewScheduler;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitLearningAnswerCommandHandler(
        ICurrentUserService currentUser,
        IWordRepository wordRepository,
        IUserWordRepository userWordRepository,
        ILearningSessionRepository learningSessionRepository,
        IReviewScheduler reviewScheduler,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _wordRepository = wordRepository;
        _userWordRepository = userWordRepository;
        _learningSessionRepository = learningSessionRepository;
        _reviewScheduler = reviewScheduler;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitLearningAnswerResultDto> Handle(
        SubmitLearningAnswerCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var word = await _wordRepository.GetByIdAsync(request.WordId, cancellationToken)
            ?? throw new AppException("WORD_NOT_FOUND", $"Word '{request.WordId}' was not found.", 404);

        var userWord = await _userWordRepository.GetByUserAndWordAsync(userId, request.WordId, cancellationToken);
        if (userWord is null)
        {
            userWord = UserWord.Create(userId, word.Id);
            await _userWordRepository.AddAsync(userWord, cancellationToken);
        }

        var nextReview = _reviewScheduler.CalculateNextReview(userWord, request.Answer);
        userWord.ApplyAnswer(request.Answer, nextReview);

        LearningSession? session = null;
        if (request.SessionId.HasValue)
        {
            session = await _learningSessionRepository.GetByIdAsync(request.SessionId.Value, cancellationToken);
            if (session is null || session.UserId != userId)
            {
                throw new AppException("SESSION_NOT_FOUND", "Learning session was not found.", 404);
            }
        }
        else
        {
            session = await _learningSessionRepository.GetActiveAsync(userId, cancellationToken);
        }

        if (session is not null)
        {
            var isCorrect = request.Answer is LearningAnswer.Know or LearningAnswer.KnowVeryWell;
            session.RecordAnswer(isCorrect);
            await _learningSessionRepository.UpdateAsync(session, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitLearningAnswerResultDto(
            word.Id,
            userWord.Status,
            userWord.KnowledgeLevel,
            userWord.NextReviewAt,
            session?.ToDto());
    }
}

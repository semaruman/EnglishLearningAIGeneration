using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Common.Options;
using EnglishLearning.Application.Features.Practice.DTOs;
using EnglishLearning.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Application.Features.Practice.Commands;

public class GeneratePracticeTextCommandHandler : IRequestHandler<GeneratePracticeTextCommand, PracticeTextDto>
{
    private const int MinimumVocabularyWords = 10;

    private readonly ICurrentUserService _currentUser;
    private readonly IVocabularySelectionStrategy _vocabularySelection;
    private readonly IPracticePromptBuilder _promptBuilder;
    private readonly ILanguageModelService _languageModel;
    private readonly ITextVocabularyValidator _textValidator;
    private readonly IPracticeSessionRepository _practiceSessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PracticeOptions _options;

    public GeneratePracticeTextCommandHandler(
        ICurrentUserService currentUser,
        IVocabularySelectionStrategy vocabularySelection,
        IPracticePromptBuilder promptBuilder,
        ILanguageModelService languageModel,
        ITextVocabularyValidator textValidator,
        IPracticeSessionRepository practiceSessionRepository,
        IUnitOfWork unitOfWork,
        IOptions<PracticeOptions> options)
    {
        _currentUser = currentUser;
        _vocabularySelection = vocabularySelection;
        _promptBuilder = promptBuilder;
        _languageModel = languageModel;
        _textValidator = textValidator;
        _practiceSessionRepository = practiceSessionRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<PracticeTextDto> Handle(GeneratePracticeTextCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var vocabulary = await _vocabularySelection.SelectAsync(
            userId,
            request.Difficulty,
            _options.MaxVocabularyWords,
            cancellationToken);

        if (vocabulary.Count < MinimumVocabularyWords)
        {
            throw new AppException(
                "INSUFFICIENT_VOCABULARY",
                $"At least {MinimumVocabularyWords} vocabulary words are required to generate practice text.");
        }

        var practiceRequest = new PracticeTextRequest
        {
            Topic = request.Topic,
            Difficulty = request.Difficulty,
            Length = request.Length,
            AllowedWords = vocabulary
        };

        var prompt = _promptBuilder.Build(practiceRequest);
        string? generatedText = null;
        TextValidationResult? lastValidation = null;

        for (var attempt = 1; attempt <= _options.MaxGenerationRetries; attempt++)
        {
            generatedText = await _languageModel.GeneratePracticeTextAsync(practiceRequest, cancellationToken);
            lastValidation = _textValidator.Validate(generatedText, vocabulary);

            if (lastValidation.IsValid)
            {
                break;
            }

            if (attempt == _options.MaxGenerationRetries)
            {
                throw new AppException(
                    "PRACTICE_GENERATION_FAILED",
                    lastValidation.Message ?? "Generated text contained words outside the allowed vocabulary.");
            }
        }

        if (string.IsNullOrWhiteSpace(generatedText))
        {
            throw new AppException("PRACTICE_GENERATION_FAILED", "Language model returned an empty text.");
        }

        var session = PracticeSession.Create(userId, request.Topic, prompt, generatedText, request.Difficulty);
        await _practiceSessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PracticeTextDto(
            session.Id,
            session.Topic,
            session.Difficulty,
            session.GeneratedText,
            session.WordCount,
            session.CreatedAt,
            vocabulary);
    }
}

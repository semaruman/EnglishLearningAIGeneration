using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Features.Learning.DTOs;

public record LearningSessionDto(
    Guid Id,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int WordsReviewed,
    int CorrectAnswers,
    int IncorrectAnswers);

public record LearningCardDto(
    Guid WordId,
    string WordText,
    string PartOfSpeech,
    string Definition,
    string Translation,
    string? Pronunciation,
    string? Phonetic,
    string? ExampleSentence,
    DifficultyLevel DifficultyLevel,
    bool IsInVocabulary,
    WordStatus? Status,
    int? KnowledgeLevel,
    Guid? SessionId);

public record SubmitLearningAnswerResultDto(
    Guid WordId,
    WordStatus Status,
    int KnowledgeLevel,
    DateTime? NextReviewAt,
    LearningSessionDto? Session);

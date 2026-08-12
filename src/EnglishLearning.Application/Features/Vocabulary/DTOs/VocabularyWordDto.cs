using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Features.Vocabulary.DTOs;

public record VocabularyWordDto(
    Guid WordId,
    string WordText,
    string PartOfSpeech,
    string Definition,
    string Translation,
    string? Pronunciation,
    string? Phonetic,
    string? ExampleSentence,
    DifficultyLevel DifficultyLevel,
    WordStatus Status,
    int KnowledgeLevel,
    DateTime AddedAt,
    DateTime? LastReviewedAt,
    DateTime? NextReviewAt,
    int CorrectAnswers,
    int IncorrectAnswers,
    int ReviewCount,
    bool AlreadyExisted = false);

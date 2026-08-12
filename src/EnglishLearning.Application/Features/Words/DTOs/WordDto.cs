using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Features.Words.DTOs;

public record WordDto(
    Guid Id,
    string WordText,
    string NormalizedText,
    string PartOfSpeech,
    string Definition,
    string Translation,
    string? Pronunciation,
    string? Phonetic,
    string? ExampleSentence,
    DifficultyLevel DifficultyLevel,
    DateTime CreatedAt);

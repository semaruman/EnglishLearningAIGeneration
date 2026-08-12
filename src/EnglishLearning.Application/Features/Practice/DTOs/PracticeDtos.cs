namespace EnglishLearning.Application.Features.Practice.DTOs;

public record PracticeTextDto(
    Guid SessionId,
    string Topic,
    string Difficulty,
    string GeneratedText,
    int WordCount,
    DateTime CreatedAt,
    IReadOnlyList<string> VocabularyUsed);

public record PracticeSessionDto(
    Guid Id,
    string Topic,
    string Difficulty,
    string GeneratedText,
    int WordCount,
    DateTime CreatedAt);

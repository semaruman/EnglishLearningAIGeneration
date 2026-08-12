namespace EnglishLearning.Application.Features.WordSets.DTOs;

public record WordSetDto(
    Guid Id,
    string Name,
    string Description,
    string Language,
    string Level,
    string Category,
    string? CoverImageUrl,
    int WordCount,
    DateTime CreatedAt);

public record WordSetDetailDto(
    Guid Id,
    string Name,
    string Description,
    string Language,
    string Level,
    string Category,
    string? CoverImageUrl,
    DateTime CreatedAt,
    IReadOnlyList<WordSetItemDto> Items);

public record WordSetItemDto(
    Guid WordId,
    string WordText,
    string PartOfSpeech,
    string Definition,
    string Translation,
    int Order);

public record AddWordSetResultDto(int AddedCount, int SkippedCount);

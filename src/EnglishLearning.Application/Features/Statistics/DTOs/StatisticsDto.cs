namespace EnglishLearning.Application.Features.Statistics.DTOs;

public record StatisticsDto(
    int TotalWords,
    int NewWords,
    int LearningWords,
    int KnownWords,
    int MasteredWords,
    int WordsReviewedToday,
    int PracticeSessions,
    int DueForReviewCount);

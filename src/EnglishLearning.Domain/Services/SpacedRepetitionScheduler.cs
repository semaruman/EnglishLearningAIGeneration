using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Interfaces;

namespace EnglishLearning.Domain.Services;

public class SpacedRepetitionScheduler : IReviewScheduler
{
    public DateTime CalculateNextReview(UserWord userWord, LearningAnswer answer)
    {
        var now = DateTime.UtcNow;
        var minutes = answer switch
        {
            LearningAnswer.DontKnow => 10,
            LearningAnswer.Know => userWord.KnowledgeLevel switch
            {
                < 30 => 60,
                < 60 => 60 * 6,
                < 90 => 60 * 24,
                _ => 60 * 72
            },
            LearningAnswer.KnowVeryWell => userWord.KnowledgeLevel switch
            {
                < 50 => 60 * 12,
                < 80 => 60 * 48,
                _ => 60 * 168
            },
            _ => 60
        };

        return now.AddMinutes(minutes);
    }
}

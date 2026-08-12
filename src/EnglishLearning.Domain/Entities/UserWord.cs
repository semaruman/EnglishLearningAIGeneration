using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class UserWord
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid WordId { get; private set; }
    public Word? Word { get; private set; }
    public WordStatus Status { get; private set; } = WordStatus.New;
    public int KnowledgeLevel { get; private set; }
    public DateTime AddedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; private set; }
    public DateTime? NextReviewAt { get; private set; }
    public int CorrectAnswers { get; private set; }
    public int IncorrectAnswers { get; private set; }
    public int ReviewCount { get; private set; }

    private UserWord()
    {
    }

    public static UserWord Create(string userId, Guid wordId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return new UserWord
        {
            UserId = userId,
            WordId = wordId,
            Status = WordStatus.New,
            KnowledgeLevel = 0,
            AddedAt = DateTime.UtcNow,
            NextReviewAt = DateTime.UtcNow
        };
    }

    public void ApplyAnswer(LearningAnswer answer, DateTime nextReviewAt)
    {
        switch (answer)
        {
            case LearningAnswer.DontKnow:
                DecreaseKnowledge(10);
                IncorrectAnswers++;
                break;
            case LearningAnswer.Know:
                IncreaseKnowledge(10);
                CorrectAnswers++;
                break;
            case LearningAnswer.KnowVeryWell:
                IncreaseKnowledge(20);
                CorrectAnswers++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(answer), answer, null);
        }

        ReviewCount++;
        LastReviewedAt = DateTime.UtcNow;
        NextReviewAt = nextReviewAt;
        RecalculateStatus();
    }

    public void MarkAsUnknown() => DecreaseKnowledge(10);
    public void MarkAsKnown() => IncreaseKnowledge(10);
    public void IncreaseKnowledge(int amount) => SetKnowledgeLevel(KnowledgeLevel + amount);
    public void DecreaseKnowledge(int amount) => SetKnowledgeLevel(KnowledgeLevel - amount);

    private void SetKnowledgeLevel(int value)
    {
        KnowledgeLevel = Math.Clamp(value, 0, 100);
        RecalculateStatus();
    }

    private void RecalculateStatus()
    {
        Status = KnowledgeLevel switch
        {
            >= 90 => WordStatus.Mastered,
            >= 60 => WordStatus.Known,
            >= 20 => WordStatus.Learning,
            _ => WordStatus.New
        };
    }

    public void SetNextReviewAt(DateTime nextReviewAt) => NextReviewAt = nextReviewAt;
}

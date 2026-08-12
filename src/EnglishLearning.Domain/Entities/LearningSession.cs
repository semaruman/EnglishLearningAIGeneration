namespace EnglishLearning.Domain.Entities;

public class LearningSession
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }
    public int WordsReviewed { get; private set; }
    public int CorrectAnswers { get; private set; }
    public int IncorrectAnswers { get; private set; }

    private LearningSession()
    {
    }

    public static LearningSession Start(string userId) =>
        new()
        {
            UserId = userId,
            StartedAt = DateTime.UtcNow
        };

    public void RecordAnswer(bool isCorrect)
    {
        WordsReviewed++;
        if (isCorrect)
        {
            CorrectAnswers++;
        }
        else
        {
            IncorrectAnswers++;
        }
    }

    public void Complete() => CompletedAt = DateTime.UtcNow;
}

namespace EnglishLearning.Domain.Entities;

public class PracticeSession
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public string Prompt { get; private set; } = string.Empty;
    public string GeneratedText { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public int WordCount { get; private set; }
    public string Difficulty { get; private set; } = "Easy";
    public string Topic { get; private set; } = string.Empty;

    private PracticeSession()
    {
    }

    public static PracticeSession Create(
        string userId,
        string topic,
        string prompt,
        string generatedText,
        string difficulty)
    {
        var words = generatedText
            .Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return new PracticeSession
        {
            UserId = userId,
            Topic = topic,
            Prompt = prompt,
            GeneratedText = generatedText,
            Difficulty = difficulty,
            WordCount = words,
            CreatedAt = DateTime.UtcNow
        };
    }
}

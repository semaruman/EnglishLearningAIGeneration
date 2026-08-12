namespace EnglishLearning.Domain.Entities;

public class WordSet
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Language { get; private set; } = "en";
    public string Level { get; private set; } = "A1";
    public string Category { get; private set; } = string.Empty;
    public string? CoverImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<WordSetItem> _items = [];
    public IReadOnlyCollection<WordSetItem> Items => _items.AsReadOnly();

    private WordSet()
    {
    }

    public static WordSet Create(
        string name,
        string description,
        string level,
        string category,
        string language = "en",
        string? coverImageUrl = null)
    {
        return new WordSet
        {
            Name = name.Trim(),
            Description = description.Trim(),
            Level = level.Trim(),
            Category = category.Trim(),
            Language = language.Trim(),
            CoverImageUrl = coverImageUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid wordId, int order)
    {
        if (_items.Any(i => i.WordId == wordId))
        {
            return;
        }

        _items.Add(WordSetItem.Create(Id, wordId, order));
    }
}

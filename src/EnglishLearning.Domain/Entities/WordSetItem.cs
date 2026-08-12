namespace EnglishLearning.Domain.Entities;

public class WordSetItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WordSetId { get; private set; }
    public Guid WordId { get; private set; }
    public Word? Word { get; private set; }
    public int Order { get; private set; }

    private WordSetItem()
    {
    }

    public static WordSetItem Create(Guid wordSetId, Guid wordId, int order) =>
        new()
        {
            WordSetId = wordSetId,
            WordId = wordId,
            Order = order
        };
}

namespace EnglishLearning.Application.Common.Options;

public class PracticeOptions
{
    public const string SectionName = "Practice";

    public int MaxVocabularyWords { get; set; } = 2000;
    public int MaxGenerationRetries { get; set; } = 3;
}

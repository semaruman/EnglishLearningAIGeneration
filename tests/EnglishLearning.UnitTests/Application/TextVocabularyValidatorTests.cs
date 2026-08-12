using EnglishLearning.Application.Services;
using FluentAssertions;

namespace EnglishLearning.UnitTests.Application;

public class TextVocabularyValidatorTests
{
    private readonly TextVocabularyValidator _sut = new(new BasicWordNormalizer());

    [Fact]
    public void Validate_AcceptsTextUsingOnlyAllowedWords()
    {
        var allowed = new[] { "cat", "dog", "run", "park" };

        var result = _sut.Validate("The cat and the dog run in the park.", allowed);

        result.IsValid.Should().BeTrue();
        result.DisallowedWords.Should().BeEmpty();
    }

    [Fact]
    public void Validate_RejectsUnknownWords()
    {
        var allowed = new[] { "cat", "dog" };

        var result = _sut.Validate("The cat saw a unicorn.", allowed);

        result.IsValid.Should().BeFalse();
        result.DisallowedWords.Should().Contain(w => w.Contains("unicorn", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_HandlesPunctuation()
    {
        var allowed = new[] { "hello", "world" };

        var result = _sut.Validate("Hello, world!", allowed);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyText_IsInvalid()
    {
        var result = _sut.Validate("   ", ["cat"]);

        result.IsValid.Should().BeFalse();
    }
}

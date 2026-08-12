using EnglishLearning.Application.Services;
using FluentAssertions;

namespace EnglishLearning.UnitTests.Application;

public class BasicWordNormalizerTests
{
    private readonly BasicWordNormalizer _sut = new();

    [Theory]
    [InlineData("  Hello! ", "hello")]
    [InlineData("CAT", "cat")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Normalize_CleansAndLowercases(string input, string expected)
    {
        _sut.Normalize(input).Should().Be(expected);
    }

    [Fact]
    public void Tokenize_SplitsWordsAndDropsStopWords()
    {
        var tokens = _sut.Tokenize("The cats are running in the park.");

        tokens.Should().Contain("cat");
        tokens.Should().Contain("run");
        tokens.Should().Contain("park");
        tokens.Should().NotContain("the");
        tokens.Should().NotContain("are");
        tokens.Should().NotContain("in");
    }

    [Theory]
    [InlineData("walks", "walk")]
    [InlineData("walked", "walk")]
    [InlineData("walking", "walk")]
    public void Normalize_StemsWalkInflections(string input, string expectedStem)
    {
        _sut.Normalize(input).Should().Be(expectedStem);
    }
}

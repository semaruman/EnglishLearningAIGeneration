using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Services;
using FluentAssertions;

namespace EnglishLearning.UnitTests.Application;

public class PracticePromptBuilderTests
{
    [Fact]
    public void Build_ContainsAllowedWordsAndTopicDataDelimiters()
    {
        var builder = new PracticePromptBuilder();
        var request = new PracticeTextRequest
        {
            Topic = "weekend picnic",
            Difficulty = "Easy",
            Length = "Short",
            AllowedWords = ["apple", "bread", "park", "friend"]
        };

        var prompt = builder.Build(request);

        prompt.Should().Contain("<<<TOPIC_DATA_START>>>");
        prompt.Should().Contain("<<<TOPIC_DATA_END>>>");
        prompt.Should().Contain("weekend picnic");
        prompt.Should().Contain("apple");
        prompt.Should().Contain("bread");
        prompt.Should().Contain("park");
        prompt.Should().Contain("friend");
        prompt.Should().Contain("ALLOWED VOCABULARY:");
    }
}

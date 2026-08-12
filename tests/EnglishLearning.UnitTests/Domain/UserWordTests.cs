using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using FluentAssertions;

namespace EnglishLearning.UnitTests.Domain;

public class UserWordTests
{
    [Fact]
    public void MarkAsUnknown_DecreasesKnowledge()
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());
        userWord.IncreaseKnowledge(30);

        userWord.MarkAsUnknown();

        userWord.KnowledgeLevel.Should().Be(20);
        userWord.Status.Should().Be(WordStatus.Learning);
    }

    [Fact]
    public void MarkAsKnown_IncreasesKnowledge()
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());

        userWord.MarkAsKnown();

        userWord.KnowledgeLevel.Should().Be(10);
        userWord.Status.Should().Be(WordStatus.New);
    }

    [Fact]
    public void IncreaseKnowledge_ClampsAt100()
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());

        userWord.IncreaseKnowledge(250);

        userWord.KnowledgeLevel.Should().Be(100);
        userWord.Status.Should().Be(WordStatus.Mastered);
    }

    [Fact]
    public void DecreaseKnowledge_ClampsAt0()
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());
        userWord.IncreaseKnowledge(15);

        userWord.DecreaseKnowledge(50);

        userWord.KnowledgeLevel.Should().Be(0);
        userWord.Status.Should().Be(WordStatus.New);
    }

    [Theory]
    [InlineData(LearningAnswer.DontKnow, 0, 0)]
    [InlineData(LearningAnswer.Know, 0, 10)]
    [InlineData(LearningAnswer.KnowVeryWell, 0, 20)]
    public void ApplyAnswer_AdjustsKnowledgeAndClamps(
        LearningAnswer answer,
        int startLevel,
        int expectedLevel)
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());
        if (startLevel > 0)
        {
            userWord.IncreaseKnowledge(startLevel);
        }

        var nextReview = DateTime.UtcNow.AddHours(1);
        userWord.ApplyAnswer(answer, nextReview);

        userWord.KnowledgeLevel.Should().Be(expectedLevel);
        userWord.NextReviewAt.Should().Be(nextReview);
        userWord.ReviewCount.Should().Be(1);
        userWord.LastReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public void ApplyAnswer_DontKnow_DoesNotGoBelowZero()
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());

        userWord.ApplyAnswer(LearningAnswer.DontKnow, DateTime.UtcNow.AddMinutes(10));

        userWord.KnowledgeLevel.Should().Be(0);
        userWord.IncorrectAnswers.Should().Be(1);
    }

    [Fact]
    public void ApplyAnswer_KnowVeryWell_DoesNotExceed100()
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());
        userWord.IncreaseKnowledge(95);

        userWord.ApplyAnswer(LearningAnswer.KnowVeryWell, DateTime.UtcNow.AddDays(1));

        userWord.KnowledgeLevel.Should().Be(100);
        userWord.CorrectAnswers.Should().Be(1);
        userWord.Status.Should().Be(WordStatus.Mastered);
    }

    [Theory]
    [InlineData(0, WordStatus.New)]
    [InlineData(19, WordStatus.New)]
    [InlineData(20, WordStatus.Learning)]
    [InlineData(59, WordStatus.Learning)]
    [InlineData(60, WordStatus.Known)]
    [InlineData(89, WordStatus.Known)]
    [InlineData(90, WordStatus.Mastered)]
    [InlineData(100, WordStatus.Mastered)]
    public void KnowledgeLevel_MapsToExpectedStatus(int knowledge, WordStatus expected)
    {
        var userWord = UserWord.Create("user-1", Guid.NewGuid());

        userWord.IncreaseKnowledge(knowledge);

        userWord.KnowledgeLevel.Should().Be(knowledge);
        userWord.Status.Should().Be(expected);
    }
}

using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Services;
using FluentAssertions;

namespace EnglishLearning.UnitTests.Domain;

public class SpacedRepetitionSchedulerTests
{
    [Fact]
    public void DontKnow_SchedulesSoonerThanKnowVeryWell()
    {
        var scheduler = new SpacedRepetitionScheduler();
        var userWord = UserWord.Create("user-1", Guid.NewGuid());

        var dontKnow = scheduler.CalculateNextReview(userWord, LearningAnswer.DontKnow);
        var knowVeryWell = scheduler.CalculateNextReview(userWord, LearningAnswer.KnowVeryWell);

        dontKnow.Should().BeBefore(knowVeryWell);
        (knowVeryWell - dontKnow).TotalMinutes.Should().BeGreaterThan(60);
    }

    [Fact]
    public void DontKnow_SchedulesAboutTenMinutesFromNow()
    {
        var scheduler = new SpacedRepetitionScheduler();
        var userWord = UserWord.Create("user-1", Guid.NewGuid());
        var before = DateTime.UtcNow;

        var next = scheduler.CalculateNextReview(userWord, LearningAnswer.DontKnow);

        next.Should().BeOnOrAfter(before.AddMinutes(9));
        next.Should().BeOnOrBefore(DateTime.UtcNow.AddMinutes(11));
    }
}

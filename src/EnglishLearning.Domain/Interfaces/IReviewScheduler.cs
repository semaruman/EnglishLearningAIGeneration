using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Domain.Interfaces;

public interface IReviewScheduler
{
    DateTime CalculateNextReview(UserWord userWord, LearningAnswer answer);
}

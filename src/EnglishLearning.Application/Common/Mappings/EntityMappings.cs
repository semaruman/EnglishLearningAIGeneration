using EnglishLearning.Domain.Entities;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.Application.Features.Words.DTOs;
using EnglishLearning.Application.Features.Learning.DTOs;
using EnglishLearning.Application.Features.Practice.DTOs;
using EnglishLearning.Application.Features.WordSets.DTOs;

namespace EnglishLearning.Application.Common.Mappings;

public static class EntityMappings
{
    public static WordDto ToDto(this Word word) =>
        new(
            word.Id,
            word.WordText,
            word.NormalizedText,
            word.PartOfSpeech,
            word.Definition,
            word.Translation,
            word.Pronunciation,
            word.Phonetic,
            word.ExampleSentence,
            word.DifficultyLevel,
            word.CreatedAt);

    public static VocabularyWordDto ToVocabularyDto(this UserWord userWord, bool alreadyExisted = false)
    {
        var word = userWord.Word
            ?? throw new InvalidOperationException("UserWord.Word navigation must be loaded.");

        return new VocabularyWordDto(
            word.Id,
            word.WordText,
            word.PartOfSpeech,
            word.Definition,
            word.Translation,
            word.Pronunciation,
            word.Phonetic,
            word.ExampleSentence,
            word.DifficultyLevel,
            userWord.Status,
            userWord.KnowledgeLevel,
            userWord.AddedAt,
            userWord.LastReviewedAt,
            userWord.NextReviewAt,
            userWord.CorrectAnswers,
            userWord.IncorrectAnswers,
            userWord.ReviewCount,
            alreadyExisted);
    }

    public static VocabularyWordDto ToVocabularyDto(this Word word, UserWord userWord, bool alreadyExisted = false) =>
        new(
            word.Id,
            word.WordText,
            word.PartOfSpeech,
            word.Definition,
            word.Translation,
            word.Pronunciation,
            word.Phonetic,
            word.ExampleSentence,
            word.DifficultyLevel,
            userWord.Status,
            userWord.KnowledgeLevel,
            userWord.AddedAt,
            userWord.LastReviewedAt,
            userWord.NextReviewAt,
            userWord.CorrectAnswers,
            userWord.IncorrectAnswers,
            userWord.ReviewCount,
            alreadyExisted);

    public static LearningSessionDto ToDto(this LearningSession session) =>
        new(
            session.Id,
            session.StartedAt,
            session.CompletedAt,
            session.WordsReviewed,
            session.CorrectAnswers,
            session.IncorrectAnswers);

    public static PracticeSessionDto ToDto(this PracticeSession session) =>
        new(
            session.Id,
            session.Topic,
            session.Difficulty,
            session.GeneratedText,
            session.WordCount,
            session.CreatedAt);

    public static WordSetDto ToDto(this WordSet wordSet) =>
        new(
            wordSet.Id,
            wordSet.Name,
            wordSet.Description,
            wordSet.Language,
            wordSet.Level,
            wordSet.Category,
            wordSet.CoverImageUrl,
            wordSet.Items.Count,
            wordSet.CreatedAt);

    public static WordSetDetailDto ToDetailDto(this WordSet wordSet) =>
        new(
            wordSet.Id,
            wordSet.Name,
            wordSet.Description,
            wordSet.Language,
            wordSet.Level,
            wordSet.Category,
            wordSet.CoverImageUrl,
            wordSet.CreatedAt,
            wordSet.Items
                .OrderBy(i => i.Order)
                .Select(i => new WordSetItemDto(
                    i.WordId,
                    i.Word?.WordText ?? string.Empty,
                    i.Word?.PartOfSpeech ?? string.Empty,
                    i.Word?.Definition ?? string.Empty,
                    i.Word?.Translation ?? string.Empty,
                    i.Order))
                .ToList());
}

using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.Domain.Entities;
using MediatR;

namespace EnglishLearning.Application.Features.Words.Commands;

public class AddWordByTextCommandHandler : IRequestHandler<AddWordByTextCommand, VocabularyWordDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWordRepository _wordRepository;
    private readonly IUserWordRepository _userWordRepository;
    private readonly ILanguageModelService _languageModel;
    private readonly IWordNormalizer _normalizer;
    private readonly IUnitOfWork _unitOfWork;

    public AddWordByTextCommandHandler(
        ICurrentUserService currentUser,
        IWordRepository wordRepository,
        IUserWordRepository userWordRepository,
        ILanguageModelService languageModel,
        IWordNormalizer normalizer,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _wordRepository = wordRepository;
        _userWordRepository = userWordRepository;
        _languageModel = languageModel;
        _normalizer = normalizer;
        _unitOfWork = unitOfWork;
    }

    public async Task<VocabularyWordDto> Handle(AddWordByTextCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var normalized = _normalizer.Normalize(request.WordText);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new AppException("INVALID_WORD", "Word text is required.");
        }

        var word = await _wordRepository.GetByNormalizedTextAsync(normalized, cancellationToken);

        if (word is null)
        {
            var generated = await _languageModel.GenerateWordDataAsync(request.WordText.Trim(), cancellationToken);
            word = Word.Create(
                string.IsNullOrWhiteSpace(generated.WordText) ? request.WordText.Trim() : generated.WordText,
                generated.PartOfSpeech,
                generated.Definition,
                generated.Translation,
                generated.DifficultyLevel,
                generated.Pronunciation,
                generated.Phonetic,
                generated.ExampleSentence);

            await _wordRepository.AddAsync(word, cancellationToken);
        }

        var existingUserWord = await _userWordRepository.GetByUserAndWordAsync(userId, word.Id, cancellationToken);
        if (existingUserWord is not null)
        {
            return word.ToVocabularyDto(existingUserWord, alreadyExisted: true);
        }

        var userWord = UserWord.Create(userId, word.Id);
        await _userWordRepository.AddAsync(userWord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return word.ToVocabularyDto(userWord, alreadyExisted: false);
    }
}

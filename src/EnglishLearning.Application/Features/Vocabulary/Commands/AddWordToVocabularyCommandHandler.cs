using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.Domain.Entities;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Commands;

public class AddWordToVocabularyCommandHandler : IRequestHandler<AddWordToVocabularyCommand, VocabularyWordDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWordRepository _wordRepository;
    private readonly IUserWordRepository _userWordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddWordToVocabularyCommandHandler(
        ICurrentUserService currentUser,
        IWordRepository wordRepository,
        IUserWordRepository userWordRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _wordRepository = wordRepository;
        _userWordRepository = userWordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VocabularyWordDto> Handle(AddWordToVocabularyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var word = await _wordRepository.GetByIdAsync(request.WordId, cancellationToken)
            ?? throw new AppException("WORD_NOT_FOUND", $"Word '{request.WordId}' was not found.", 404);

        var existing = await _userWordRepository.GetByUserAndWordAsync(userId, request.WordId, cancellationToken);
        if (existing is not null)
        {
            return word.ToVocabularyDto(existing, alreadyExisted: true);
        }

        var userWord = UserWord.Create(userId, word.Id);
        await _userWordRepository.AddAsync(userWord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return word.ToVocabularyDto(userWord, alreadyExisted: false);
    }
}

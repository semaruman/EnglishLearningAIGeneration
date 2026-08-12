using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Queries;

public class GetVocabularyWordQueryHandler : IRequestHandler<GetVocabularyWordQuery, VocabularyWordDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserWordRepository _userWordRepository;

    public GetVocabularyWordQueryHandler(ICurrentUserService currentUser, IUserWordRepository userWordRepository)
    {
        _currentUser = currentUser;
        _userWordRepository = userWordRepository;
    }

    public async Task<VocabularyWordDto> Handle(GetVocabularyWordQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var userWord = await _userWordRepository.GetByUserAndWordAsync(userId, request.WordId, cancellationToken)
            ?? throw new AppException("VOCABULARY_WORD_NOT_FOUND", "Word is not in your vocabulary.", 404);

        return userWord.ToVocabularyDto();
    }
}

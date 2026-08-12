using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Commands;

public class DeleteWordFromVocabularyCommandHandler : IRequestHandler<DeleteWordFromVocabularyCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserWordRepository _userWordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWordFromVocabularyCommandHandler(
        ICurrentUserService currentUser,
        IUserWordRepository userWordRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _userWordRepository = userWordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteWordFromVocabularyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var userWord = await _userWordRepository.GetByUserAndWordAsync(userId, request.WordId, cancellationToken)
            ?? throw new AppException("VOCABULARY_WORD_NOT_FOUND", "Word is not in your vocabulary.", 404);

        await _userWordRepository.RemoveAsync(userWord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

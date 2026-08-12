using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Features.WordSets.DTOs;
using EnglishLearning.Domain.Entities;
using MediatR;

namespace EnglishLearning.Application.Features.WordSets.Commands;

public class AddWordSetToVocabularyCommandHandler : IRequestHandler<AddWordSetToVocabularyCommand, AddWordSetResultDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWordSetRepository _wordSetRepository;
    private readonly IUserWordRepository _userWordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddWordSetToVocabularyCommandHandler(
        ICurrentUserService currentUser,
        IWordSetRepository wordSetRepository,
        IUserWordRepository userWordRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _wordSetRepository = wordSetRepository;
        _userWordRepository = userWordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddWordSetResultDto> Handle(
        AddWordSetToVocabularyCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var wordSet = await _wordSetRepository.GetByIdWithItemsAsync(request.WordSetId, cancellationToken)
            ?? throw new AppException("WORD_SET_NOT_FOUND", $"Word set '{request.WordSetId}' was not found.", 404);

        var existingIds = (await _userWordRepository.GetUserWordIdsAsync(userId, cancellationToken)).ToHashSet();
        var added = 0;
        var skipped = 0;

        foreach (var item in wordSet.Items)
        {
            if (existingIds.Contains(item.WordId))
            {
                skipped++;
                continue;
            }

            await _userWordRepository.AddAsync(UserWord.Create(userId, item.WordId), cancellationToken);
            existingIds.Add(item.WordId);
            added++;
        }

        if (added > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new AddWordSetResultDto(added, skipped);
    }
}

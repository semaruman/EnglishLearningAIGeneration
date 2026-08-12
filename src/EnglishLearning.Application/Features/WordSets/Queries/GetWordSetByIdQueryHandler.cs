using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.WordSets.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.WordSets.Queries;

public class GetWordSetByIdQueryHandler : IRequestHandler<GetWordSetByIdQuery, WordSetDetailDto>
{
    private readonly IWordSetRepository _wordSetRepository;

    public GetWordSetByIdQueryHandler(IWordSetRepository wordSetRepository)
    {
        _wordSetRepository = wordSetRepository;
    }

    public async Task<WordSetDetailDto> Handle(GetWordSetByIdQuery request, CancellationToken cancellationToken)
    {
        var wordSet = await _wordSetRepository.GetByIdWithItemsAsync(request.Id, cancellationToken)
            ?? throw new AppException("WORD_SET_NOT_FOUND", $"Word set '{request.Id}' was not found.", 404);

        return wordSet.ToDetailDto();
    }
}

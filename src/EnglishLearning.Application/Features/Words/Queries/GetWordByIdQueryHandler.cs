using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Mappings;
using EnglishLearning.Application.Features.Words.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Words.Queries;

public class GetWordByIdQueryHandler : IRequestHandler<GetWordByIdQuery, WordDto>
{
    private readonly IWordRepository _wordRepository;

    public GetWordByIdQueryHandler(IWordRepository wordRepository)
    {
        _wordRepository = wordRepository;
    }

    public async Task<WordDto> Handle(GetWordByIdQuery request, CancellationToken cancellationToken)
    {
        var word = await _wordRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new AppException("WORD_NOT_FOUND", $"Word '{request.Id}' was not found.", 404);

        return word.ToDto();
    }
}

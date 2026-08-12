using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.WordSets.Commands;
using EnglishLearning.Application.Features.WordSets.DTOs;
using EnglishLearning.Application.Features.WordSets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/word-sets")]
public sealed class WordSetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WordSetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult<IReadOnlyList<WordSetDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWordSetsQuery(), cancellationToken);
        return Ok(ApiResult<IReadOnlyList<WordSetDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResult<WordSetDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWordSetByIdQuery(id), cancellationToken);
        return Ok(ApiResult<WordSetDetailDto>.Ok(result));
    }

    [HttpPost("{id:guid}/add")]
    public async Task<ActionResult<ApiResult<AddWordSetResultDto>>> AddToVocabulary(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddWordSetToVocabularyCommand(id), cancellationToken);
        return Ok(ApiResult<AddWordSetResultDto>.Ok(result));
    }
}

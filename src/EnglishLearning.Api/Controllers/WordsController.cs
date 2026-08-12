using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.Application.Features.Words.Commands;
using EnglishLearning.Application.Features.Words.DTOs;
using EnglishLearning.Application.Features.Words.Queries;
using EnglishLearning.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class WordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult<PagedResult<WordDto>>>> GetWords(
        [FromQuery] string? search,
        [FromQuery] string? partOfSpeech,
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetWordsQuery(search, partOfSpeech, difficulty, page, pageSize),
            cancellationToken);
        return Ok(ApiResult<PagedResult<WordDto>>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResult<PagedResult<WordDto>>>> Search(
        [FromQuery] string? search,
        [FromQuery] string? partOfSpeech,
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new SearchWordsQuery(search, partOfSpeech, difficulty, page, pageSize),
            cancellationToken);
        return Ok(ApiResult<PagedResult<WordDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResult<WordDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWordByIdQuery(id), cancellationToken);
        return Ok(ApiResult<WordDto>.Ok(result));
    }

    [HttpPost]
    [EnableRateLimiting("ai-word-add")]
    public async Task<ActionResult<ApiResult<VocabularyWordDto>>> AddByText(
        [FromBody] AddWordByTextCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResult<VocabularyWordDto>.Ok(result));
    }
}

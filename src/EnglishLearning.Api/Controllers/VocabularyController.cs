using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Vocabulary.Commands;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.Application.Features.Vocabulary.Queries;
using EnglishLearning.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class VocabularyController : ControllerBase
{
    private readonly IMediator _mediator;

    public VocabularyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult<PagedResult<VocabularyWordDto>>>> GetMyVocabulary(
        [FromQuery] WordStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyVocabularyQuery(status, search, page, pageSize),
            cancellationToken);
        return Ok(ApiResult<PagedResult<VocabularyWordDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResult<VocabularyWordDto>>> Add(
        [FromBody] AddWordToVocabularyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddWordToVocabularyCommand(request.WordId), cancellationToken);
        return Ok(ApiResult<VocabularyWordDto>.Ok(result));
    }

    [HttpGet("{wordId:guid}")]
    public async Task<ActionResult<ApiResult<VocabularyWordDto>>> GetWord(
        Guid wordId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVocabularyWordQuery(wordId), cancellationToken);
        return Ok(ApiResult<VocabularyWordDto>.Ok(result));
    }

    [HttpDelete("{wordId:guid}")]
    public async Task<IActionResult> Delete(Guid wordId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteWordFromVocabularyCommand(wordId), cancellationToken);
        return Ok(new { success = true });
    }

    public sealed record AddWordToVocabularyRequest(Guid WordId);
}

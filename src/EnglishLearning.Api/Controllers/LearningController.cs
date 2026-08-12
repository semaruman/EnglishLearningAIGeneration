using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Learning.Commands;
using EnglishLearning.Application.Features.Learning.DTOs;
using EnglishLearning.Application.Features.Learning.Queries;
using EnglishLearning.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class LearningController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("session")]
    public async Task<ActionResult<ApiResult<LearningSessionDto>>> StartSession(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new StartLearningSessionCommand(), cancellationToken);
        return Ok(ApiResult<LearningSessionDto>.Ok(result));
    }

    [HttpGet("next")]
    public async Task<ActionResult<ApiResult<LearningCardDto?>>> GetNext(
        [FromQuery] Guid? sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNextLearningCardQuery(sessionId), cancellationToken);
        return Ok(ApiResult<LearningCardDto?>.Ok(result));
    }

    [HttpPost("{wordId:guid}/answer")]
    public async Task<ActionResult<ApiResult<SubmitLearningAnswerResultDto>>> SubmitAnswer(
        Guid wordId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitLearningAnswerCommand(wordId, request.Answer, request.SessionId),
            cancellationToken);
        return Ok(ApiResult<SubmitLearningAnswerResultDto>.Ok(result));
    }

    public sealed record SubmitAnswerRequest(LearningAnswer Answer, Guid? SessionId = null);
}

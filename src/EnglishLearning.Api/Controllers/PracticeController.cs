using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Practice.Commands;
using EnglishLearning.Application.Features.Practice.DTOs;
using EnglishLearning.Application.Features.Practice.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PracticeController : ControllerBase
{
    private readonly IMediator _mediator;

    public PracticeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate")]
    [EnableRateLimiting("practice-generate")]
    public async Task<ActionResult<ApiResult<PracticeTextDto>>> Generate(
        [FromBody] GeneratePracticeTextCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResult<PracticeTextDto>.Ok(result));
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResult<PagedResult<PracticeSessionDto>>>> History(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPracticeHistoryQuery(page, pageSize), cancellationToken);
        return Ok(ApiResult<PagedResult<PracticeSessionDto>>.Ok(result));
    }
}

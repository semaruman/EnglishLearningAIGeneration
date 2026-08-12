using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Statistics.DTOs;
using EnglishLearning.Application.Features.Statistics.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult<StatisticsDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatisticsQuery(), cancellationToken);
        return Ok(ApiResult<StatisticsDto>.Ok(result));
    }
}

using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Practice.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Practice.Queries;

public record GetPracticeHistoryQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<PracticeSessionDto>>;

using EnglishLearning.Application.Features.Statistics.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Statistics.Queries;

public record GetStatisticsQuery : IRequest<StatisticsDto>;

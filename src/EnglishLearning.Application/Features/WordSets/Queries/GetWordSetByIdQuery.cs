using EnglishLearning.Application.Features.WordSets.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.WordSets.Queries;

public record GetWordSetByIdQuery(Guid Id) : IRequest<WordSetDetailDto>;

using EnglishLearning.Application.Features.Learning.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Learning.Queries;

public record GetNextLearningCardQuery(Guid? SessionId = null) : IRequest<LearningCardDto?>;

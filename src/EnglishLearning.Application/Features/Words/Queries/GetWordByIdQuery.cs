using EnglishLearning.Application.Features.Words.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Words.Queries;

public record GetWordByIdQuery(Guid Id) : IRequest<WordDto>;

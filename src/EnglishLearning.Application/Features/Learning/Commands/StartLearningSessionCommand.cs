using EnglishLearning.Application.Features.Learning.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Learning.Commands;

public record StartLearningSessionCommand : IRequest<LearningSessionDto>;

using EnglishLearning.Application.Features.Learning.DTOs;
using EnglishLearning.Domain.Enums;
using MediatR;

namespace EnglishLearning.Application.Features.Learning.Commands;

public record SubmitLearningAnswerCommand(
    Guid WordId,
    LearningAnswer Answer,
    Guid? SessionId = null) : IRequest<SubmitLearningAnswerResultDto>;

using EnglishLearning.Application.Features.WordSets.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.WordSets.Commands;

public record AddWordSetToVocabularyCommand(Guid WordSetId) : IRequest<AddWordSetResultDto>;

using EnglishLearning.Application.Features.Vocabulary.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Commands;

public record AddWordToVocabularyCommand(Guid WordId) : IRequest<VocabularyWordDto>;

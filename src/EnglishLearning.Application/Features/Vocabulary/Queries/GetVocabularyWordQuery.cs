using EnglishLearning.Application.Features.Vocabulary.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Queries;

public record GetVocabularyWordQuery(Guid WordId) : IRequest<VocabularyWordDto>;

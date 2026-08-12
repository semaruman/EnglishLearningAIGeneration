using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Commands;

public record DeleteWordFromVocabularyCommand(Guid WordId) : IRequest<Unit>;

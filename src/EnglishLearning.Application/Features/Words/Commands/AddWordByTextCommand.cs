using EnglishLearning.Application.Features.Vocabulary.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Words.Commands;

public record AddWordByTextCommand(string WordText) : IRequest<VocabularyWordDto>;

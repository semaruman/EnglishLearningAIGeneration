using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.Domain.Enums;
using MediatR;

namespace EnglishLearning.Application.Features.Vocabulary.Queries;

public record GetMyVocabularyQuery(
    WordStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<VocabularyWordDto>>;

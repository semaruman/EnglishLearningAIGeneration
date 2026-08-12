using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Words.DTOs;
using EnglishLearning.Domain.Enums;
using MediatR;

namespace EnglishLearning.Application.Features.Words.Queries;

public record SearchWordsQuery(
    string? Search = null,
    string? PartOfSpeech = null,
    DifficultyLevel? Difficulty = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<WordDto>>;

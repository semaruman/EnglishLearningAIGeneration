using EnglishLearning.Application.Features.Practice.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Practice.Commands;

public record GeneratePracticeTextCommand(
    string Topic,
    string Difficulty,
    string Length) : IRequest<PracticeTextDto>;

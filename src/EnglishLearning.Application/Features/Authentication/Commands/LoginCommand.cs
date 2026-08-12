using EnglishLearning.Application.Features.Authentication.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Authentication.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

using EnglishLearning.Application.Features.Authentication.DTOs;
using MediatR;

namespace EnglishLearning.Application.Features.Authentication.Commands;

public record RegisterCommand(string Email, string UserName, string Password) : IRequest<AuthResultDto>;

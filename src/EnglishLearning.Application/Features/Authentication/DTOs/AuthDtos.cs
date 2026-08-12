namespace EnglishLearning.Application.Features.Authentication.DTOs;

public record AuthResultDto(string Token, string Email, string UserName, string UserId);

public record UserDto(string UserId, string Email, string UserName);

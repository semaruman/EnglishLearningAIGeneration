using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Authentication.DTOs;
using FluentAssertions;

namespace EnglishLearning.IntegrationTests.Infrastructure;

public static class TestAuthHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<(string Token, string UserId, string Email)> RegisterAsync(
        HttpClient client,
        string? email = null,
        string? userName = null,
        string password = "Password1")
    {
        email ??= $"user_{Guid.NewGuid():N}@example.com";
        userName ??= $"user_{Guid.NewGuid():N}"[..16];

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            userName,
            password
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResult<AuthResultDto>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();
        payload.Data!.Token.Should().NotBeNullOrWhiteSpace();

        return (payload.Data.Token, payload.Data.UserId, payload.Data.Email);
    }

    public static async Task<string> LoginAsync(HttpClient client, string email, string password = "Password1")
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResult<AuthResultDto>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data.Should().NotBeNull();
        return payload.Data!.Token;
    }

    public static HttpClient WithBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

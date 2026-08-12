using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Practice.DTOs;
using EnglishLearning.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace EnglishLearning.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class PracticeValidationTests
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PracticeValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureSeededWords();
    }

    [Fact]
    public async Task Generate_WithValidLlm_ReturnsPracticeText()
    {
        var client = await CreateClientWithVocabularyAsync(12);

        var response = await client.PostAsJsonAsync("/api/practice/generate", new
        {
            topic = "animals in the park",
            difficulty = "Easy",
            length = "Short"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiResult<PracticeTextDto>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();
        payload.Data!.GeneratedText.Should().NotBeNullOrWhiteSpace();
        payload.Data.VocabularyUsed.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Generate_WhenLlmReturnsUnknownWord_ReturnsPracticeGenerationFailed()
    {
        var client = await CreateClientWithVocabularyAsync(12);

        var response = await client.PostAsJsonAsync("/api/practice/generate", new
        {
            topic = "FORCE_INVALID animals",
            difficulty = "Easy",
            length = "Short"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("error").GetProperty("code").GetString()
            .Should().Be("PRACTICE_GENERATION_FAILED");
    }

    private async Task<HttpClient> CreateClientWithVocabularyAsync(int wordCount)
    {
        _factory.SeededWordIds.Count.Should().BeGreaterThanOrEqualTo(wordCount);
        var client = _factory.CreateClient();
        var (token, _, _) = await TestAuthHelper.RegisterAsync(client);
        client.WithBearer(token);

        foreach (var wordId in _factory.SeededWordIds.Take(wordCount))
        {
            var add = await client.PostAsJsonAsync("/api/vocabulary", new { wordId });
            add.EnsureSuccessStatusCode();
        }

        return client;
    }
}

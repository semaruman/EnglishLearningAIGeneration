using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Vocabulary.DTOs;
using EnglishLearning.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace EnglishLearning.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class AuthIsolationTests
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthIsolationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.EnsureSeededWords();
    }

    [Fact]
    public async Task UserB_CannotSeeOrDelete_UserA_Vocabulary()
    {
        _factory.SeededWordIds.Should().NotBeEmpty();
        var wordId = _factory.SeededWordIds[0];

        var clientA = _factory.CreateClient();
        var (tokenA, _, _) = await TestAuthHelper.RegisterAsync(clientA);
        clientA.WithBearer(tokenA);

        var addResponse = await clientA.PostAsJsonAsync("/api/vocabulary", new { wordId });
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var clientB = _factory.CreateClient();
        var (tokenB, _, _) = await TestAuthHelper.RegisterAsync(clientB);
        clientB.WithBearer(tokenB);

        var vocabResponse = await clientB.GetAsync("/api/vocabulary?page=1&pageSize=50");
        vocabResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var vocab = await vocabResponse.Content.ReadFromJsonAsync<ApiResult<PagedResult<VocabularyWordDto>>>(JsonOptions);
        vocab.Should().NotBeNull();
        vocab!.Success.Should().BeTrue();
        vocab.Data.Should().NotBeNull();
        vocab.Data!.Items.Should().NotContain(w => w.WordId == wordId);
        vocab.Data.TotalCount.Should().Be(0);

        var deleteResponse = await clientB.DeleteAsync($"/api/vocabulary/{wordId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var errorJson = await deleteResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(errorJson);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("error").GetProperty("code").GetString()
            .Should().Be("VOCABULARY_WORD_NOT_FOUND");

        // UserA still has the word
        var userAVocab = await clientA.GetAsync("/api/vocabulary?page=1&pageSize=50");
        userAVocab.StatusCode.Should().Be(HttpStatusCode.OK);
        var aPayload = await userAVocab.Content.ReadFromJsonAsync<ApiResult<PagedResult<VocabularyWordDto>>>(JsonOptions);
        aPayload!.Data!.Items.Should().Contain(w => w.WordId == wordId);
    }
}

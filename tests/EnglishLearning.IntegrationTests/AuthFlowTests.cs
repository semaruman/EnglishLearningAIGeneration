using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.Features.Authentication.DTOs;
using EnglishLearning.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace EnglishLearning.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class AuthFlowTests
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_Me_Works()
    {
        var client = _factory.CreateClient();
        var email = $"flow_{Guid.NewGuid():N}@example.com";
        var userName = $"flow_{Guid.NewGuid():N}"[..16];
        const string password = "Password1";

        var (token, userId, registeredEmail) = await TestAuthHelper.RegisterAsync(client, email, userName, password);
        registeredEmail.Should().Be(email);
        token.Should().NotBeNullOrWhiteSpace();
        userId.Should().NotBeNullOrWhiteSpace();

        var loginToken = await TestAuthHelper.LoginAsync(client, email, password);
        loginToken.Should().NotBeNullOrWhiteSpace();

        client.WithBearer(loginToken);
        var meResponse = await client.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await meResponse.Content.ReadFromJsonAsync<ApiResult<UserDto>>(JsonOptions);
        me.Should().NotBeNull();
        me!.Success.Should().BeTrue();
        me.Data.Should().NotBeNull();
        me.Data!.Email.Should().Be(email);
        me.Data.UserName.Should().Be(userName);
        me.Data.UserId.Should().Be(userId);
    }
}

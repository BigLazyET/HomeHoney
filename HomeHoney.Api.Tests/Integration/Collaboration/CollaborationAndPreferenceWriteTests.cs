using System.Net;
using System.Net.Http.Json;
using HomeHoney.Models;

namespace HomeHoney.Api.Tests.Integration.Collaboration;

public sealed class CollaborationAndPreferenceWriteTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public CollaborationAndPreferenceWriteTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        TestServiceOverrides.Reset();
    }

    [Fact]
    public async Task Fridge_note_can_be_saved_and_deleted_through_api()
    {
        using var client = _factory.CreateClient();

        var note = new FridgeNote
        {
            Title = "后端新增冰箱贴",
            Content = "验证协作写链路",
            DueAt = DateTime.UtcNow.AddDays(1),
        };

        using var saveResponse = await client.PostAsJsonAsync("/api/v1/collaboration/fridge-notes", note);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var envelope = await saveResponse.Content.ReadFromJsonAsync<ApiEnvelope<FridgeNote>>();
        Assert.NotNull(envelope);
        Assert.NotEqual(Guid.Empty, envelope.Data.Id);

        using var deleteResponse = await client.DeleteAsync($"/api/v1/collaboration/fridge-notes/{envelope.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Preference_can_be_updated_and_read_back()
    {
        using var client = _factory.CreateClient();
        var preference = new UserPreference
        {
            OnboardingCompleted = true,
            ThemeMode = ThemeMode.Dark,
        };

        using var response = await client.PutAsJsonAsync("/api/v1/preferences/me", preference);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var current = await client.GetFromJsonAsync<UserPreference>("/api/v1/preferences/me");
        Assert.NotNull(current);
        Assert.True(current.OnboardingCompleted);
        Assert.Equal(ThemeMode.Dark, current.ThemeMode);
    }

    private sealed class ApiEnvelope<T>
    {
        public required T Data { get; set; }
    }
}
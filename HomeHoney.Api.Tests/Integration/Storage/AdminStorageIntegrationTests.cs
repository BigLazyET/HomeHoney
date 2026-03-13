using System.Net;
using System.Net.Http.Json;
using HomeHoney.Api.Contracts.Storage;

namespace HomeHoney.Api.Tests.Integration.Storage;

public sealed class AdminStorageIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AdminStorageIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        TestServiceOverrides.Reset();
    }

    [Fact]
    public async Task Save_backend_profile_and_downstream_settings_round_trips()
    {
        TestServiceOverrides.FileBrowserValidationResult = new(true, "ok");
        TestServiceOverrides.MongoValidationResult = new(true, "ok");

        using var client = _factory.CreateClient();

        using var profileResponse = await client.PutAsJsonAsync("/api/v1/admin/backend-profile", new UpdateBackendServiceProfileRequest
        {
            DisplayName = "家庭后端",
            ApiBaseUrl = "http://localhost:7080"
        });

        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        using var storageResponse = await client.PutAsJsonAsync("/api/v1/admin/storage", new UpdateDownstreamStorageSettingsRequest
        {
            FileServiceBaseUrl = "http://localhost:8999",
            FileServiceApiPath = "/api",
            FileServiceUsername = "homehoney",
            FileServicePassword = "secret-token",
            MongoDatabaseName = "homehoney",
            MongoConnectionString = "mongodb://localhost:27017/homehoney"
        });

        Assert.Equal(HttpStatusCode.OK, storageResponse.StatusCode);

        var profile = await client.GetFromJsonAsync<BackendServiceProfileDto>("/api/v1/admin/backend-profile");
        var storage = await client.GetFromJsonAsync<DownstreamStorageSettingsDto>("/api/v1/admin/storage");

        Assert.Equal("家庭后端", profile?.DisplayName);
        Assert.Equal("http://localhost:8999", storage?.FileServiceBaseUrl);
        Assert.Equal("homehoney", storage?.FileServiceUsername);
        Assert.True(storage?.HasFileServicePassword);
        Assert.Equal(ValidationStatusDto.Valid, storage?.ValidationStatus);
        Assert.StartsWith("mongodb://", storage?.MongoConnectionStringPreview);
    }

    [Fact]
    public async Task Invalid_downstream_settings_returns_problem_details()
    {
        TestServiceOverrides.FileBrowserValidationResult = new(false, "文件服务不可用。");

        using var client = _factory.CreateClient();
        using var response = await client.PutAsJsonAsync("/api/v1/admin/storage", new UpdateDownstreamStorageSettingsRequest
        {
            FileServiceBaseUrl = "http://localhost:8999",
            FileServiceApiPath = "/api",
            MongoDatabaseName = "homehoney",
            MongoConnectionString = "mongodb://localhost:27017/homehoney"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("文件服务不可用", body);
    }
}
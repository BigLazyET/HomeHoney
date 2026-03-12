using System.Net;
using System.Net.Http.Json;
using HomeHoney.Api.Contracts.Storage;
using HomeHoney.Api.Tests.Integration;

namespace HomeHoney.Api.Tests.Contract.Storage;

public sealed class AdminStorageContractTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AdminStorageContractTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        TestServiceOverrides.Reset();
    }

    [Fact]
    public async Task Get_backend_profile_returns_contract_shape()
    {
        using var client = _factory.CreateClient();

        var profile = await client.GetFromJsonAsync<BackendServiceProfileDto>("/api/v1/admin/backend-profile");

        Assert.NotNull(profile);
        Assert.False(string.IsNullOrWhiteSpace(profile.ApiBaseUrl));
    }

    [Fact]
    public async Task Put_storage_returns_operation_result_with_updated_payload()
    {
        TestServiceOverrides.FileBrowserValidationResult = new(true, "ok");
        TestServiceOverrides.MongoValidationResult = new(true, "ok");

        using var client = _factory.CreateClient();
        using var response = await client.PutAsJsonAsync("/api/v1/admin/storage", new UpdateDownstreamStorageSettingsRequest
        {
            FileServiceBaseUrl = "http://localhost:8999",
            FileServiceApiPath = "/api",
            MongoDatabaseName = "homehoney",
            MongoConnectionString = "mongodb://localhost:27017/homehoney"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AdminOperationEnvelope<DownstreamStorageSettingsDto>>();
        Assert.NotNull(payload);
        Assert.True(payload.Result.IsSuccess);
        Assert.Equal("homehoney", payload.Data.MongoDatabaseName);
        Assert.Equal(ValidationStatusDto.Valid, payload.Data.ValidationStatus);
    }

    private sealed class AdminOperationEnvelope<T>
    {
        public required OperationResultDto Result { get; set; }
        public required T Data { get; set; }
    }

    private sealed class OperationResultDto
    {
        public bool IsSuccess { get; set; }
    }
}
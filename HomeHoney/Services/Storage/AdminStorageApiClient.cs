using System.Net.Http.Json;
using System.Text.Json;
using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class AdminStorageApiClient : IAdminStorageApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public AdminStorageApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<StorageConnectionProfile> GetProfileAsync(string? backendBaseUrl = null, CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(backendBaseUrl, cancellationToken);
        var backendProfile = await GetRequiredAsync<BackendProfileDto>(client, "api/v1/admin/backend-profile", cancellationToken);
        var downstream = await GetRequiredAsync<DownstreamSettingsDto>(client, "api/v1/admin/storage", cancellationToken);
        return MapProfile(backendProfile, downstream);
    }

    public async Task<StorageProfileSaveResult> SaveProfileAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(profile.ApiBaseUrl, cancellationToken);

        var backendResponse = await PutAsync<BackendProfileDto>(client, "api/v1/admin/backend-profile", new UpdateBackendProfileRequest
        {
            DisplayName = profile.DisplayName,
            ApiBaseUrl = profile.ApiBaseUrl,
        }, cancellationToken);

        if (!backendResponse.Result.IsSuccess || backendResponse.Data is null)
        {
            profile.ValidationStatus = StorageValidationStatus.Invalid;
            profile.ValidationMessage = backendResponse.Result.Message;
            profile.LastValidatedAt = DateTime.UtcNow;
            return new(false, profile, backendResponse.Result.Message);
        }

        var storageResponse = await PutAsync<DownstreamSettingsDto>(client, "api/v1/admin/storage", new UpdateDownstreamSettingsRequest
        {
            FileServiceBaseUrl = profile.FileServiceBaseUrl,
            FileServiceApiPath = profile.FileServiceApiPath,
            MongoDatabaseName = profile.MongoDatabaseName,
            MongoConnectionString = mongoConnectionString,
        }, cancellationToken);

        if (!storageResponse.Result.IsSuccess || storageResponse.Data is null)
        {
            profile.ValidationStatus = StorageValidationStatus.Invalid;
            profile.ValidationMessage = storageResponse.Result.Message;
            profile.LastValidatedAt = DateTime.UtcNow;
            return new(false, profile, storageResponse.Result.Message);
        }

        var savedProfile = MapProfile(backendResponse.Data, storageResponse.Data);
        savedProfile.MongoConnectionStringSecretKey = profile.MongoConnectionStringSecretKey;
        return new(true, savedProfile, storageResponse.Result.Message);
    }

    private static StorageConnectionProfile MapProfile(BackendProfileDto backendProfile, DownstreamSettingsDto downstream)
        => new()
        {
            ProfileId = backendProfile.ProfileId,
            DisplayName = backendProfile.DisplayName,
            ApiBaseUrl = backendProfile.ApiBaseUrl,
            FileServiceBaseUrl = downstream.FileServiceBaseUrl,
            FileServiceApiPath = downstream.FileServiceApiPath,
            MongoDatabaseName = downstream.MongoDatabaseName,
            MongoConnectionStringPreview = downstream.MongoConnectionStringPreview,
            IsActive = backendProfile.IsActive,
            LastValidatedAt = downstream.LastValidatedAt ?? backendProfile.LastValidatedAt,
            ValidationStatus = MapStatus(downstream.ValidationStatus),
            ValidationMessage = downstream.ValidationMessage ?? backendProfile.ValidationMessage,
        };

    private static StorageValidationStatus MapStatus(string? status)
        => status?.ToLowerInvariant() switch
        {
            "valid" => StorageValidationStatus.Valid,
            "invalid" => StorageValidationStatus.Invalid,
            _ => StorageValidationStatus.Unknown,
        };

    private static async Task<T> GetRequiredAsync<T>(HttpClient client, string requestUri, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException($"接口 {requestUri} 返回了空响应。");
    }

    private static async Task<ApiEnvelope<T>> PutAsync<T>(HttpClient client, string requestUri, object request, CancellationToken cancellationToken)
    {
        using var response = await client.PutAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions, cancellationToken);
            return payload ?? new ApiEnvelope<T> { Result = new OperationResultDto { IsSuccess = false, Message = "接口返回了空结果。" } };
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(JsonOptions, cancellationToken);
        return new ApiEnvelope<T>
        {
            Result = new OperationResultDto
            {
                IsSuccess = false,
                Message = problem?.Detail ?? problem?.Title ?? $"请求失败：{(int)response.StatusCode}",
            },
        };
    }

    private sealed class ApiEnvelope<T>
    {
        public OperationResultDto Result { get; set; } = new();
        public T? Data { get; set; }
    }

    private sealed class OperationResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private sealed class ProblemDetailsDto
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
    }

    private sealed class BackendProfileDto
    {
        public Guid ProfileId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastValidatedAt { get; set; }
        public string ValidationStatus { get; set; } = string.Empty;
        public string? ValidationMessage { get; set; }
    }

    private sealed class DownstreamSettingsDto
    {
        public string FileServiceBaseUrl { get; set; } = string.Empty;
        public string FileServiceApiPath { get; set; } = string.Empty;
        public string MongoDatabaseName { get; set; } = string.Empty;
        public DateTime? LastValidatedAt { get; set; }
        public string MongoConnectionStringPreview { get; set; } = string.Empty;
        public string ValidationStatus { get; set; } = string.Empty;
        public string? ValidationMessage { get; set; }
    }

    private sealed class UpdateBackendProfileRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = string.Empty;
    }

    private sealed class UpdateDownstreamSettingsRequest
    {
        public string FileServiceBaseUrl { get; set; } = string.Empty;
        public string FileServiceApiPath { get; set; } = string.Empty;
        public string MongoDatabaseName { get; set; } = string.Empty;
        public string MongoConnectionString { get; set; } = string.Empty;
    }
}
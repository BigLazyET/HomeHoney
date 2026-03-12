using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class AdminStorageApiClient : IAdminStorageApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public AdminStorageApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<StorageConnectionProfile> GetProfileAsync(string? backendBaseUrl = null, CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(backendBaseUrl, cancellationToken);
        var backendProfile = await GetRequiredAsync<BackendProfileDto>(client, "api/v1/admin/backend-profile", cancellationToken);
        return MapProfile(backendProfile);
    }

    public async Task<StorageProfileSaveResult> SaveProfileAsync(StorageConnectionProfile profile, CancellationToken cancellationToken = default)
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

        var savedProfile = MapProfile(backendResponse.Data);
        savedProfile.MongoConnectionStringSecretKey = profile.MongoConnectionStringSecretKey;
        return new(true, savedProfile, backendResponse.Result.Message);
    }

    private static StorageConnectionProfile MapProfile(BackendProfileDto backendProfile)
        => new()
        {
            ProfileId = backendProfile.ProfileId,
            DisplayName = backendProfile.DisplayName,
            ApiBaseUrl = backendProfile.ApiBaseUrl,
            IsActive = backendProfile.IsActive,
            LastValidatedAt = backendProfile.LastValidatedAt,
            ValidationStatus = backendProfile.ValidationStatus,
            ValidationMessage = backendProfile.ValidationMessage,
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
        public StorageValidationStatus ValidationStatus { get; set; } = StorageValidationStatus.Unknown;
        public string? ValidationMessage { get; set; }
    }

    private sealed class UpdateBackendProfileRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = string.Empty;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
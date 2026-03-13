using System.Net.Http.Json;
using HomeHoney.Models;
using HomeHoney.Services.Storage;
using System.Text.Json;

namespace HomeHoney.Services.Documents;

public sealed class DocumentApiClient : IDocumentApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public DocumentApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<InsuranceRecord>("api/v1/documents/insurance", cancellationToken);

    public Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<ManualRecord>("api/v1/documents/manuals", cancellationToken);

    public Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => GetItemAsync<InsuranceRecord>($"api/v1/documents/insurance/{id}", cancellationToken);

    public Task<ManualRecord?> GetManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => GetItemAsync<ManualRecord>($"api/v1/documents/manuals/{id}", cancellationToken);

    public Task<InsuranceRecord> SaveInsuranceRecordAsync(InsuranceRecord record, CancellationToken cancellationToken = default)
        => PostEntityAsync<UpsertInsuranceDocumentRequest, InsuranceRecord>($"api/v1/documents/insurance", Map(record), cancellationToken);

    public Task<ManualRecord> SaveManualRecordAsync(ManualRecord record, CancellationToken cancellationToken = default)
        => PostEntityAsync<UpsertManualDocumentRequest, ManualRecord>($"api/v1/documents/manuals", Map(record), cancellationToken);

    public Task<bool> DeleteInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/v1/documents/insurance/{id}", cancellationToken);

    public Task<bool> DeleteManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/v1/documents/manuals/{id}", cancellationToken);

    public Task<FileStorageResult> UploadInsuranceFileAsync(Guid id, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
        => UploadAsync($"api/v1/documents/insurance/{id}/files:upload", content, fileName, contentType, cancellationToken);

    public Task<FileStorageResult> UploadManualFileAsync(Guid id, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
        => UploadAsync($"api/v1/documents/manuals/{id}/files:upload", content, fileName, contentType, cancellationToken);

    public Task<FileDownloadResult> DownloadInsuranceFileAsync(Guid id, CancellationToken cancellationToken = default)
        => DownloadAsync($"api/v1/documents/insurance/{id}/files/primary:download", cancellationToken);

    public Task<FileDownloadResult> DownloadManualFileAsync(Guid id, CancellationToken cancellationToken = default)
        => DownloadAsync($"api/v1/documents/manuals/{id}/files/primary:download", cancellationToken);

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(cancellationToken: cancellationToken) ?? [];
    }

    private async Task<TResponse> PostEntityAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.PostAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<TResponse>>(JsonOptions, cancellationToken);
        if (envelope is null)
        {
            throw new InvalidOperationException($"接口 {requestUri} 返回了空结果。");
        }

        return envelope.Data;
    }

    private async Task<T?> GetItemAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync(requestUri, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private async Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.DeleteAsync(requestUri, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task<FileStorageResult> UploadAsync(string requestUri, Stream content, string fileName, string? contentType, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType ?? "application/octet-stream");
        form.Add(streamContent, "file", fileName);

        using var response = await client.PostAsync(requestUri, form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var failureMessage = await ReadFailureMessageAsync(response, cancellationToken);
            return new(false, failureMessage, AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }

        var envelope = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        var message = envelope.TryGetProperty("result", out var resultElement) && resultElement.TryGetProperty("message", out var messageElement)
            ? messageElement.GetString() ?? "上传成功。"
            : "上传成功。";

        FileResource? file = null;
        if (envelope.TryGetProperty("data", out var dataElement) && dataElement.TryGetProperty("primaryFile", out var fileElement))
        {
            file = fileElement.Deserialize<FileResource>(JsonOptions);
        }

        return new(true, message, file?.ExternalPath, file?.ExternalFileId, file?.FileName, file?.ContentType, file?.SizeBytes, file?.LastSyncedAt, file?.AvailabilityStatus ?? FileAvailabilityStatus.Available);
    }

    private static async Task<string> ReadFailureMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);

            if (payload.ValueKind == JsonValueKind.Object)
            {
                if (payload.TryGetProperty("result", out var resultElement) &&
                    resultElement.ValueKind == JsonValueKind.Object &&
                    resultElement.TryGetProperty("message", out var messageElement) &&
                    messageElement.GetString() is { Length: > 0 } resultMessage)
                {
                    return resultMessage;
                }

                if (payload.TryGetProperty("title", out var titleElement) &&
                    payload.TryGetProperty("detail", out var detailElement))
                {
                    var title = titleElement.GetString();
                    var detail = detailElement.GetString();
                    if (!string.IsNullOrWhiteSpace(detail))
                    {
                        return detail!;
                    }

                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        return title!;
                    }
                }

                if (payload.TryGetProperty("message", out var directMessageElement) && directMessageElement.GetString() is { Length: > 0 } payloadMessage)
                {
                    return payloadMessage;
                }
            }
        }
        catch
        {
            // Fall back to plain-text response parsing below.
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            return raw.Trim();
        }

        return $"上传失败：{(int)response.StatusCode} {response.ReasonPhrase}";
    }

    private async Task<FileDownloadResult> DownloadAsync(string requestUri, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new(false, $"下载失败：{(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "download.bin";
        return new(true, "下载成功。", bytes, fileName, response.Content.Headers.ContentType?.MediaType);
    }

    private sealed class ApiEnvelope<T>
    {
        public OperationResult? Result { get; set; }
        public T Data { get; set; } = default!;
    }

    private sealed class OperationResult
    {
        public string? Message { get; set; }
    }

    private sealed class UpsertInsuranceDocumentRequest
    {
        public Guid Id { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public Guid InsuredMemberId { get; set; }
        public InsuranceCategory InsuranceCategory { get; set; }
        public InsuranceStatus Status { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = [];
    }

    private sealed class UpsertManualDocumentRequest
    {
        public Guid Id { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public Guid SpaceId { get; set; }
        public ManualCategory ManualCategory { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public DateOnly WarrantyExpiryDate { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = [];
    }

    private static UpsertInsuranceDocumentRequest Map(InsuranceRecord record)
        => new()
        {
            Id = record.Id,
            PolicyName = record.PolicyName,
            InsuredMemberId = record.InsuredMemberId,
            InsuranceCategory = record.InsuranceCategory,
            Status = record.Status,
            EffectiveDate = record.EffectiveDate,
            ExpiryDate = record.ExpiryDate,
            ProviderName = record.ProviderName,
            ContactName = record.ContactName,
            ContactPhone = record.ContactPhone,
            Summary = record.Summary,
            Tags = [.. record.Tags],
        };

    private static UpsertManualDocumentRequest Map(ManualRecord record)
        => new()
        {
            Id = record.Id,
            DeviceName = record.DeviceName,
            Brand = record.Brand,
            Model = record.Model,
            SpaceId = record.SpaceId,
            ManualCategory = record.ManualCategory,
            PurchaseDate = record.PurchaseDate,
            WarrantyExpiryDate = record.WarrantyExpiryDate,
            Summary = record.Summary,
            Tags = [.. record.Tags],
        };
}

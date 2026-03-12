using System.Net.Http.Json;
using HomeHoney.Models;
using HomeHoney.Services.Storage;
using System.Text.Json;

namespace HomeHoney.Services.Collaboration;

public sealed class CollaborationApiClient : ICollaborationApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public CollaborationApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default)
        => GetListAsync<FridgeNoteDto, FridgeNote>("api/v1/collaboration/fridge-notes", Map, cancellationToken);

    public Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default)
        => GetListAsync<MemoDto, Memo>("api/v1/collaboration/memos", Map, cancellationToken);

    public Task<FridgeNote> SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default)
        => PostEntityAsync<FridgeNoteDto, FridgeNoteDto, FridgeNote>("api/v1/collaboration/fridge-notes", Map(fridgeNote), Map, cancellationToken);

    public Task<Memo> SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default)
        => PostEntityAsync<MemoDto, MemoDto, Memo>("api/v1/collaboration/memos", Map(memo), Map, cancellationToken);

    public Task<bool> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/v1/collaboration/fridge-notes/{id}", cancellationToken);

    public Task<bool> DeleteMemoAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/v1/collaboration/memos/{id}", cancellationToken);

    private async Task<IReadOnlyList<TModel>> GetListAsync<TDto, TModel>(string requestUri, Func<TDto, TModel> map, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<List<TDto>>(cancellationToken: cancellationToken) ?? [];
        return payload.Select(map).ToList();
    }

    private async Task<TModel> PostEntityAsync<TRequest, TDto, TModel>(string requestUri, TRequest request, Func<TDto, TModel> map, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.PostAsJsonAsync(requestUri, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<TDto>>(JsonOptions, cancellationToken);
        if (envelope is null)
        {
            throw new InvalidOperationException($"接口 {requestUri} 返回了空结果。");
        }

        return map(envelope.Data);
    }

    private async Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.DeleteAsync(requestUri, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private sealed class ApiEnvelope<T>
    {
        public T Data { get; set; } = default!;
    }

    private sealed class FridgeNoteDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public FridgeNoteCategory Category { get; set; }
        public NotePriority Priority { get; set; }
        public string ColorStyle { get; set; } = "yellow";
        public bool IsPinned { get; set; }
        public bool IsCompleted { get; set; }
        public Guid? OwnerMemberId { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    private sealed class MemoDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public MemoCategory MemoCategory { get; set; }
        public MemoImportance Importance { get; set; }
        public MemoStatus Status { get; set; }
        public Guid? OwnerMemberId { get; set; }
        public Guid? RelatedSpaceId { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    private static FridgeNote Map(FridgeNoteDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Content = dto.Content,
            Category = dto.Category,
            Priority = dto.Priority,
            ColorStyle = dto.ColorStyle,
            IsPinned = dto.IsPinned,
            IsCompleted = dto.IsCompleted,
            OwnerMemberId = dto.OwnerMemberId,
            DueAt = dto.DueAt,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
        };

    private static FridgeNoteDto Map(FridgeNote model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            Content = model.Content,
            Category = model.Category,
            Priority = model.Priority,
            ColorStyle = model.ColorStyle,
            IsPinned = model.IsPinned,
            IsCompleted = model.IsCompleted,
            OwnerMemberId = model.OwnerMemberId,
            DueAt = model.DueAt,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
        };

    private static Memo Map(MemoDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Content = dto.Content,
            MemoCategory = dto.MemoCategory,
            Importance = dto.Importance,
            Status = dto.Status,
            OwnerMemberId = dto.OwnerMemberId,
            RelatedSpaceId = dto.RelatedSpaceId,
            DueAt = dto.DueAt,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
        };

    private static MemoDto Map(Memo model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            Content = model.Content,
            MemoCategory = model.MemoCategory,
            Importance = model.Importance,
            Status = model.Status,
            OwnerMemberId = model.OwnerMemberId,
            RelatedSpaceId = model.RelatedSpaceId,
            DueAt = model.DueAt,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
        };
}
namespace HomeHoney.Models;

public enum CollectionViewStatus
{
    Loading,
    Ready,
    Empty,
    Error,
    Retrying,
}

public sealed class RetryableCollectionState<T>
{
    public CollectionViewStatus Status { get; private set; } = CollectionViewStatus.Loading;

    public IReadOnlyList<T> Items { get; private set; } = [];

    public DateTime? LastAttemptAt { get; private set; }

    public DateTime? LastSuccessAt { get; private set; }

    public string? Message { get; private set; }

    public bool CanRetry => Status is CollectionViewStatus.Error or CollectionViewStatus.Retrying;

    public static RetryableCollectionState<T> CreateLoading(IReadOnlyList<T>? retainedItems = null)
        => new()
        {
            Status = CollectionViewStatus.Loading,
            Items = retainedItems ?? [],
            LastAttemptAt = DateTime.UtcNow,
        };

    public static RetryableCollectionState<T> CreateRetrying(IReadOnlyList<T>? retainedItems = null, string? message = null)
        => new()
        {
            Status = CollectionViewStatus.Retrying,
            Items = retainedItems ?? [],
            LastAttemptAt = DateTime.UtcNow,
            Message = message,
        };

    public static RetryableCollectionState<T> CreateReady(IReadOnlyList<T> items, string? message = null)
        => new()
        {
            Status = items.Count == 0 ? CollectionViewStatus.Empty : CollectionViewStatus.Ready,
            Items = items,
            LastAttemptAt = DateTime.UtcNow,
            LastSuccessAt = DateTime.UtcNow,
            Message = message,
        };

    public static RetryableCollectionState<T> CreateEmpty(string? message = null)
        => new()
        {
            Status = CollectionViewStatus.Empty,
            Items = [],
            LastAttemptAt = DateTime.UtcNow,
            LastSuccessAt = DateTime.UtcNow,
            Message = message,
        };

    public static RetryableCollectionState<T> CreateError(string message, IReadOnlyList<T>? retainedItems = null, DateTime? lastSuccessAt = null)
        => new()
        {
            Status = CollectionViewStatus.Error,
            Items = retainedItems ?? [],
            LastAttemptAt = DateTime.UtcNow,
            LastSuccessAt = lastSuccessAt,
            Message = message,
        };
}

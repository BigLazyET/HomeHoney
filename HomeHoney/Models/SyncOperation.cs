namespace HomeHoney.Models;

public enum SyncExecutionStatus
{
    Queued,
    Running,
    Succeeded,
    PartiallySucceeded,
    Failed,
    Retried,
    Dismissed,
}

public sealed class SyncOperation
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    public string OperationType { get; set; } = string.Empty;

    public string TargetEntityType { get; set; } = string.Empty;

    public Guid TargetEntityId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public SyncExecutionStatus RemoteFileStatus { get; set; } = SyncExecutionStatus.Queued;

    public SyncExecutionStatus RemoteDataStatus { get; set; } = SyncExecutionStatus.Queued;

    public SyncExecutionStatus LocalCacheStatus { get; set; } = SyncExecutionStatus.Queued;

    public string? UserVisibleMessage { get; set; }

    public int RetryCount { get; set; }
}

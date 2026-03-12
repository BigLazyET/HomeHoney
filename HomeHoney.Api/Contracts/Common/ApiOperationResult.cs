namespace HomeHoney.Api.Contracts.Common;

public sealed record ApiOperationResult(
    Guid OperationId,
    string OperationType,
    bool IsSuccess,
    string Code,
    string Message,
    DateTime OccurredAt,
    string? TargetType = null,
    Guid? TargetId = null,
    bool IsPartialSuccess = false,
    IReadOnlyDictionary<string, object?>? Details = null)
{
    public static ApiOperationResult Success(string operationType, string message, string code = "ok", string? targetType = null, Guid? targetId = null, IReadOnlyDictionary<string, object?>? details = null)
        => new(Guid.NewGuid(), operationType, true, code, message, DateTime.UtcNow, targetType, targetId, false, details);

    public static ApiOperationResult Failure(string operationType, string message, string code = "error", string? targetType = null, Guid? targetId = null, bool isPartialSuccess = false, IReadOnlyDictionary<string, object?>? details = null)
        => new(Guid.NewGuid(), operationType, false, code, message, DateTime.UtcNow, targetType, targetId, isPartialSuccess, details);
}

public sealed record ApiOperationResult<T>(ApiOperationResult Result, T Data);

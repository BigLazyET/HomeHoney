using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed record FileStorageResult(
    bool IsSuccess,
    string Message,
    string? RemotePath = null,
    string? RemoteId = null,
    string? ContentType = null,
    long? SizeBytes = null,
    FileAvailabilityStatus AvailabilityStatus = FileAvailabilityStatus.Unknown);

public sealed record FileDownloadResult(
    bool IsSuccess,
    string Message,
    byte[]? Content = null,
    string? FileName = null,
    string? ContentType = null,
    string? RemotePath = null);

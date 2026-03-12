namespace HomeHoney.Api.Infrastructure.FileBrowser;

public sealed class FileBrowserFileDescriptor
{
    public string? Path { get; set; }

    public string Name { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string ContentType { get; set; } = "application/octet-stream";
}

public sealed record FileBrowserUploadResult(bool IsSuccess, string Message, FileBrowserFileDescriptor? File = null);

public sealed record FileBrowserDownloadResult(bool IsSuccess, string Message, byte[]? Content = null, string? FileName = null, string? ContentType = null, string? RemotePath = null);

public sealed record FileBrowserDeleteResult(bool IsSuccess, string Message);

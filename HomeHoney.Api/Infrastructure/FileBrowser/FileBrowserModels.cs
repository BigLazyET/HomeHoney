namespace HomeHoney.Api.Infrastructure.FileBrowser;

public sealed class FileBrowserConnectionOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiPath { get; set; } = "/api";

    public string FileServiceUsername { get; set; } = string.Empty;

    public string FileServicePassword { get; set; } = string.Empty;

    public string FileServiceAuthHeaderName { get; set; } = string.Empty;

    public string FileServiceAuthHeaderValue { get; set; } = string.Empty;

    public bool HasJsonCredentials
        => !string.IsNullOrWhiteSpace(FileServiceUsername) || !string.IsNullOrWhiteSpace(FileServicePassword);

    public bool HasProxyHeader
        => !string.IsNullOrWhiteSpace(FileServiceAuthHeaderName) || !string.IsNullOrWhiteSpace(FileServiceAuthHeaderValue);
}

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

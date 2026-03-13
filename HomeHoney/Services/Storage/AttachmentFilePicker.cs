using System.Buffers;

#if ANDROID || IOS || MACCATALYST || WINDOWS
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
#endif

namespace HomeHoney.Services.Storage;

public interface IAttachmentFilePicker
{
    Task<AttachmentFilePickResult?> PickAsync(long maxBytes = 25 * 1024 * 1024, CancellationToken cancellationToken = default);
}

public sealed class AttachmentFilePickResult
{
    public AttachmentFilePickResult(string fileName, string? contentType, byte[] content)
    {
        FileName = fileName;
        ContentType = contentType;
        Content = content;
    }

    public string FileName { get; }

    public string? ContentType { get; }

    public byte[] Content { get; }

    public long SizeBytes => Content.LongLength;
}

public sealed class MauiAttachmentFilePicker : IAttachmentFilePicker
{
    public async Task<AttachmentFilePickResult?> PickAsync(long maxBytes = 25 * 1024 * 1024, CancellationToken cancellationToken = default)
    {
#if ANDROID || IOS || MACCATALYST || WINDOWS
        try
        {
            var fileResult = MainThread.IsMainThread
                ? await FilePicker.Default.PickAsync(CreatePickOptions())
                : await MainThread.InvokeOnMainThreadAsync(async () => await FilePicker.Default.PickAsync(CreatePickOptions()));

            if (fileResult is null)
            {
                return null;
            }

            await using var stream = await fileResult.OpenReadAsync();
            var content = await ReadToLimitAsync(stream, maxBytes, cancellationToken);
            var contentType = string.IsNullOrWhiteSpace(fileResult.ContentType)
                ? "application/octet-stream"
                : fileResult.ContentType;

            return new AttachmentFilePickResult(fileResult.FileName, contentType, content);
        }
        catch (TaskCanceledException)
        {
            return null;
        }
#else
        await Task.CompletedTask;
        return null;
#endif
    }

#if ANDROID || IOS || MACCATALYST || WINDOWS
    private static PickOptions CreatePickOptions()
        => new()
        {
            PickerTitle = "选择要上传的附件",
        };

    private static async Task<byte[]> ReadToLimitAsync(Stream stream, long maxBytes, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var rented = ArrayPool<byte>.Shared.Rent(81920);

        try
        {
            while (true)
            {
                var read = await stream.ReadAsync(rented.AsMemory(0, rented.Length), cancellationToken);
                if (read <= 0)
                {
                    break;
                }

                if (buffer.Length + read > maxBytes)
                {
                    throw new InvalidOperationException($"附件大小不能超过 {maxBytes / (1024 * 1024)} MB。");
                }

                await buffer.WriteAsync(rented.AsMemory(0, read), cancellationToken);
            }

            return buffer.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
#endif
}
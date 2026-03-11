namespace HomeHoney.Services.Storage;

public sealed class LocalFileCache : ILocalFileCache
{
    private static readonly string CacheRoot = Path.Combine(AppContext.BaseDirectory, "file-cache");

    public async Task<string> SaveAsync(string cacheKey, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(CacheRoot);
        var safeFileName = string.IsNullOrWhiteSpace(fileName) ? "attachment.bin" : fileName;
        var filePath = Path.Combine(CacheRoot, $"{Sanitize(cacheKey)}-{safeFileName}");
        await File.WriteAllBytesAsync(filePath, content, cancellationToken);
        return filePath;
    }

    public async Task<byte[]?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        var filePath = await GetFilePathAsync(cacheKey, cancellationToken);
        if (filePath is null || !File.Exists(filePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public Task<string?> GetFilePathAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(CacheRoot))
        {
            return Task.FromResult<string?>(null);
        }

        var prefix = $"{Sanitize(cacheKey)}-";
        var match = Directory.EnumerateFiles(CacheRoot).FirstOrDefault(path => Path.GetFileName(path).StartsWith(prefix, StringComparison.Ordinal));
        return Task.FromResult<string?>(match);
    }

    private static string Sanitize(string key)
        => string.Concat(key.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
}

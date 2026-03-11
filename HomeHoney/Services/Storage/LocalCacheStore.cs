using System.Text.Json;

namespace HomeHoney.Services.Storage;

public sealed class LocalCacheStore : ILocalCacheStore
{
    private static readonly string CacheRoot = Path.Combine(AppContext.BaseDirectory, "storage-cache");

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetFilePath(key);
            if (!File.Exists(filePath))
            {
                return default;
            }

            var raw = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<T>(raw);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(CacheRoot);
        var raw = JsonSerializer.Serialize(value);
        await File.WriteAllTextAsync(GetFilePath(key), raw, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private static string GetFilePath(string key)
        => Path.Combine(CacheRoot, $"{Sanitize(key)}.json");

    private static string Sanitize(string key)
        => string.Concat(key.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
}

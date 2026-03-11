namespace HomeHoney.Services.Storage;

public interface ILocalFileCache
{
    Task<string> SaveAsync(string cacheKey, string fileName, byte[] content, CancellationToken cancellationToken = default);

    Task<byte[]?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<string?> GetFilePathAsync(string cacheKey, CancellationToken cancellationToken = default);
}

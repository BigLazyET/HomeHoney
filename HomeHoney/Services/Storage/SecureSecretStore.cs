using System.Collections.Concurrent;

namespace HomeHoney.Services.Storage;

public sealed class SecureSecretStore : ISecretStore
{
    private static readonly ConcurrentDictionary<string, string> FallbackSecrets = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS
            await global::Microsoft.Maui.Storage.SecureStorage.Default.SetAsync(key, value);
#else
            FallbackSecrets[key] = value;
            await Task.CompletedTask;
#endif
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS
            return await global::Microsoft.Maui.Storage.SecureStorage.Default.GetAsync(key);
#else
            return FallbackSecrets.TryGetValue(key, out var value) ? value : null;
#endif
        }
        finally
        {
            _gate.Release();
        }
    }
}

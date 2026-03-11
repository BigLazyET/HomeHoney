using System.Text.Json;

namespace HomeHoney.Services.Storage;

public sealed class SecureSecretStore : ISecretStore
{
    private static readonly string SecretFilePath = Path.Combine(AppContext.BaseDirectory, "homehoney.secrets.json");
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var secrets = await LoadAsync(cancellationToken);
            secrets[key] = value;
            var raw = JsonSerializer.Serialize(secrets);
            await File.WriteAllTextAsync(SecretFilePath, raw, cancellationToken);
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
            var secrets = await LoadAsync(cancellationToken);
            return secrets.TryGetValue(key, out var value) ? value : null;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task<Dictionary<string, string>> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(SecretFilePath))
            {
                return [];
            }

            var raw = await File.ReadAllTextAsync(SecretFilePath, cancellationToken);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(raw) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

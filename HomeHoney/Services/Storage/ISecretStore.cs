namespace HomeHoney.Services.Storage;

public interface ISecretStore
{
    Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default);

    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
}

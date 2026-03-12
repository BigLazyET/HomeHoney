using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class BackendApiHttpClientFactory
{
    private readonly HttpClient _prototypeClient;
    private readonly HomeHoney.Services.Preferences.UserPreferenceService _userPreferenceService;
    private readonly BackendApiOptions _options;

    public BackendApiHttpClientFactory(HttpClient httpClient, HomeHoney.Services.Preferences.UserPreferenceService userPreferenceService, BackendApiOptions options)
    {
        _prototypeClient = httpClient;
        _userPreferenceService = userPreferenceService;
        _options = options;
    }

    public Task<HttpClient> CreateAsync(CancellationToken cancellationToken = default)
        => CreateAsync(null, cancellationToken);

    public Task<HttpClient> CreateAsync(string? backendBaseUrl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configuredBaseUrl = string.IsNullOrWhiteSpace(backendBaseUrl)
            ? _userPreferenceService.GetPreferences().StoragePreference.ApiBaseUrl
            : backendBaseUrl;

        if (string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            configuredBaseUrl = StorageConnectionProfile.DefaultBackendApiBaseUrl;
        }

        Uri? uri = null;
        if (Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var configuredUri))
        {
            uri = configuredUri;
        }
        else if (Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var fallbackUri))
        {
            uri = fallbackUri;
        }

        var client = new HttpClient
        {
            Timeout = _prototypeClient.Timeout,
            BaseAddress = uri is null ? null : new Uri(uri.ToString().TrimEnd('/') + "/"),
        };

        foreach (var header in _prototypeClient.DefaultRequestHeaders)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        return Task.FromResult(client);
    }
}

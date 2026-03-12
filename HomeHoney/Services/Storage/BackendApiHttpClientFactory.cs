namespace HomeHoney.Services.Storage;

public sealed class BackendApiHttpClientFactory
{
    private readonly HttpClient _httpClient;
    private readonly HomeHoney.Services.Preferences.UserPreferenceService _userPreferenceService;
    private readonly BackendApiOptions _options;

    public BackendApiHttpClientFactory(HttpClient httpClient, HomeHoney.Services.Preferences.UserPreferenceService userPreferenceService, BackendApiOptions options)
    {
        _httpClient = httpClient;
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

        Uri? uri = null;
        if (Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var configuredUri))
        {
            uri = configuredUri;
        }
        else if (Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var fallbackUri))
        {
            uri = fallbackUri;
        }

        if (uri is not null)
        {
            _httpClient.BaseAddress = new Uri(uri.ToString().TrimEnd('/') + "/");
        }

        return Task.FromResult(_httpClient);
    }
}

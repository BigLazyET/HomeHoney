namespace HomeHoney.Services.Storage;

public sealed class BackendApiOptions
{
    public string BaseUrl { get; set; } = HomeHoney.Models.StorageConnectionProfile.DefaultBackendApiBaseUrl;
}

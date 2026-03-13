namespace HomeHoney.Api.Infrastructure.Configuration;

public sealed class BackendStorageOptions
{
    public const string SectionName = "BackendStorage";

    public string FileServiceBaseUrl { get; set; } = "http://localhost:8999";

    public string FileServiceApiPath { get; set; } = "/api";

    public string FileServiceUsername { get; set; } = string.Empty;

    public string FileServicePassword { get; set; } = string.Empty;

    public string FileServiceAuthHeaderName { get; set; } = string.Empty;

    public string FileServiceAuthHeaderValue { get; set; } = string.Empty;

    public string MongoConnectionString { get; set; } = string.Empty;

    public string MongoDatabaseName { get; set; } = "homehoney";
}

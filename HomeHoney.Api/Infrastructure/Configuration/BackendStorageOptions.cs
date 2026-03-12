namespace HomeHoney.Api.Infrastructure.Configuration;

public sealed class BackendStorageOptions
{
    public const string SectionName = "BackendStorage";

    public string FileServiceBaseUrl { get; set; } = "http://localhost:8999";

    public string FileServiceApiPath { get; set; } = "/api";

    public string MongoConnectionString { get; set; } = string.Empty;

    public string MongoDatabaseName { get; set; } = "homehoney";
}

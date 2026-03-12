using HomeHoney.Api.Application.Collaboration;
using HomeHoney.Api.Application.Documents;
using HomeHoney.Api.Application.Preferences;
using HomeHoney.Api.Application.Reminders;
using HomeHoney.Api.Application.Search;
using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Application.Storage;
using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;

namespace HomeHoney.Api.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackendStorageOptions>(configuration.GetSection(BackendStorageOptions.SectionName));
        services.AddSingleton<SecureSettingsStore>();
        services.AddSingleton<IFileBrowserGateway, FileBrowserGateway>();
        services.AddSingleton<IMongoDatabaseFactory, MongoDatabaseFactory>();
        services.AddSingleton<MongoIndexInitializer>();
        services.AddSingleton<SeedDataService>();
        services.AddSingleton<DocumentRepository>();
        services.AddSingleton<DocumentFileOrchestrator>();
        services.AddSingleton<CollaborationRepository>();
        services.AddSingleton<PreferenceRepository>();
        services.AddSingleton<ReferenceDataRepository>();
        services.AddSingleton<DocumentReadService>();
        services.AddSingleton<DocumentWriteService>();
        services.AddSingleton<InsuranceDocumentService>();
        services.AddSingleton<ManualDocumentService>();
        services.AddSingleton<CollaborationReadService>();
        services.AddSingleton<CollaborationWriteService>();
        services.AddSingleton<FridgeNoteService>();
        services.AddSingleton<MemoService>();
        services.AddSingleton<PreferenceReadService>();
        services.AddSingleton<PreferenceWriteService>();
        services.AddSingleton<UserPreferenceService>();
        services.AddSingleton<ReminderReadService>();
        services.AddSingleton<ReminderAggregationService>();
        services.AddSingleton<SearchReadService>();
        services.AddSingleton<SearchAggregationService>();
        services.AddSingleton<BackendProfileService>();
        services.AddSingleton<StorageValidationService>();
        services.AddSingleton<IBackendStorageSettingsService, BackendStorageSettingsService>();
        return services;
    }

    public static async Task<IServiceProvider> EnsureMongoIndexesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("HomeHoney.Api.MongoIndexes");

        try
        {
            await services.GetRequiredService<MongoIndexInitializer>().EnsureIndexesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Mongo 索引初始化失败，应用将继续启动。请检查数据库连接和集合权限。");
        }

        return services;
    }

    public static IServiceProvider ValidateBackendStorageConfiguration(this IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("HomeHoney.Api.Startup");
        var configuration = services.GetRequiredService<IConfiguration>();
        var fallbackMessage = configuration["BackendStartup:StorageFallbackMessage"] ?? "后端已启动，但下游存储尚未配置完成。";
        var store = services.GetRequiredService<SecureSettingsStore>();
        var options = store.GetEffectiveStorageOptions();

        if (!Uri.TryCreate(options.FileServiceBaseUrl, UriKind.Absolute, out _))
        {
            logger.LogWarning("{Message} 当前文件服务地址无效：{BaseUrl}", fallbackMessage, options.FileServiceBaseUrl);
        }

        if (string.IsNullOrWhiteSpace(options.MongoConnectionString))
        {
            logger.LogWarning("{Message} 当前 Mongo 连接字符串为空。", fallbackMessage);
        }

        return services;
    }
}

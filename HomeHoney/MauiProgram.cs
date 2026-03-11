using HomeHoney.Services;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Preferences;
using HomeHoney.Services.Reminders;
using HomeHoney.Services.Search;
using HomeHoney.Services.Storage;
using Microsoft.Extensions.Logging;

namespace HomeHoney;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton(_ => new HttpClient());

		// Application services
		builder.Services.AddSingleton<IGreetingService, GreetingService>();
		builder.Services.AddSingleton<NavigationStructureService>();
		builder.Services.AddSingleton<ISecretStore, SecureSecretStore>();
		builder.Services.AddSingleton<StorageConfigurationValidator>();
		builder.Services.AddSingleton<IStorageConnectionProfileService, StorageConnectionProfileService>();
		builder.Services.AddSingleton<IMongoContextFactory, MongoContextFactory>();
		builder.Services.AddSingleton<ILocalCacheStore, LocalCacheStore>();
		builder.Services.AddSingleton<ILocalFileCache, LocalFileCache>();
		builder.Services.AddSingleton<IFileStorageGateway, FileBrowserFileStorageGateway>();
		builder.Services.AddSingleton<IDocumentMetadataRepository, DocumentMetadataRepository>();
		builder.Services.AddSingleton<IBusinessAggregateRepository, BusinessAggregateRepository>();
		builder.Services.AddSingleton<CollaborationRepository>();
		builder.Services.AddSingleton<PreferenceRepository>();
		builder.Services.AddSingleton<ReminderRepository>();
		builder.Services.AddSingleton<DocumentFileOrchestrator>();
		builder.Services.AddSingleton<UserPreferenceService>();
		builder.Services.AddSingleton<DocumentCatalogService>();
		builder.Services.AddSingleton<FamilyCollaborationService>();
		builder.Services.AddSingleton<SearchIndexService>();
		builder.Services.AddSingleton<ReminderCenterService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

using HomeHoney.Services;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Preferences;
using HomeHoney.Services.Reminders;
using HomeHoney.Services.Search;
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

		// Application services
		builder.Services.AddSingleton<IGreetingService, GreetingService>();
		builder.Services.AddSingleton<NavigationStructureService>();
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

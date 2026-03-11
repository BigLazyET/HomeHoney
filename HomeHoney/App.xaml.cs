using HomeHoney.Services.Diagnostics;

namespace HomeHoney;

public partial class App : Application
{
	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
		TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;

		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "HomeHoney" };
	}

	private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
	{
		if (args.ExceptionObject is Exception exception)
		{
			RuntimeDiagnostics.Record(nameof(App), exception);
			return;
		}

		RuntimeDiagnostics.Record(nameof(App), args.ExceptionObject?.ToString() ?? "Unknown unhandled exception.");
	}

	private static void HandleUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
	{
		RuntimeDiagnostics.Record(nameof(App), args.Exception);
		args.SetObserved();
	}
}

using HomeHoney.Services.Diagnostics;
using Microsoft.AspNetCore.Components.Web;

namespace HomeHoney.Components.Shared;

public sealed class AppErrorBoundary : ErrorBoundary
{
    protected override Task OnErrorAsync(Exception exception)
    {
        RuntimeDiagnostics.Record(nameof(AppErrorBoundary), exception);
        return Task.CompletedTask;
    }
}
using HomeHoney.Api.Application.Preferences;
using HomeHoney.Api.Contracts.Preferences;

namespace HomeHoney.Api.Endpoints.Preferences;

public static class PreferenceEndpoints
{
    public static IEndpointRouteBuilder MapPreferenceEndpointsV1(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/preferences/me", async (UserPreferenceService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetCurrentAsync(cancellationToken)));

        endpoints.MapPut("/api/v1/preferences/me", async (UserPreferenceDto preference, UserPreferenceService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.SaveAsync(preference, cancellationToken)));

        return endpoints;
    }
}

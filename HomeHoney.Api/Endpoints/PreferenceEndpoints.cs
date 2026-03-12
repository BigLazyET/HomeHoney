using HomeHoney.Api.Endpoints.Preferences;

namespace HomeHoney.Api.Endpoints;

public static class PreferenceEndpoints
{
    public static IEndpointRouteBuilder MapPreferenceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPreferenceEndpointsV1();
    }
}
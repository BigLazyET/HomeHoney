using HomeHoney.Api.Endpoints.Reminders;

namespace HomeHoney.Api.Endpoints;

public static class ReminderEndpoints
{
    public static IEndpointRouteBuilder MapReminderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapReminderEndpointsV1();
    }
}
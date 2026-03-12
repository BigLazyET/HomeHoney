using HomeHoney.Api.Application.Reminders;

namespace HomeHoney.Api.Endpoints.Reminders;

public static class ReminderEndpoints
{
    public static IEndpointRouteBuilder MapReminderEndpointsV1(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/reminders", async (int? days, ReminderAggregationService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetUpcomingAsync(days ?? 30, cancellationToken)));

        return endpoints;
    }
}

namespace HomeHoney.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }))
            .WithName("Health");

        return endpoints;
    }
}

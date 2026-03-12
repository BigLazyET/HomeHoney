using HomeHoney.Api.Application.Search;

namespace HomeHoney.Api.Endpoints.Search;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpointsV1(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/search", async (string q, SearchAggregationService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.SearchAsync(q, cancellationToken)));

        return endpoints;
    }
}

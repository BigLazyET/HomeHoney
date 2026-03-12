using HomeHoney.Api.Endpoints.Search;

namespace HomeHoney.Api.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapSearchEndpointsV1();
    }
}
using HomeHoney.Api.Endpoints.Collaboration;

namespace HomeHoney.Api.Endpoints;

public static class CollaborationEndpoints
{
    public static IEndpointRouteBuilder MapCollaborationEndpoints(this IEndpointRouteBuilder endpoints)
        => endpoints.MapCollaborationEndpointsV1();
}
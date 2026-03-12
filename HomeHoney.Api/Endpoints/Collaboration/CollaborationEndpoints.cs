namespace HomeHoney.Api.Endpoints.Collaboration;

public static class CollaborationEndpoints
{
    public static IEndpointRouteBuilder MapCollaborationEndpointsV1(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/v1/collaboration/fridge-notes")
            .MapFridgeNoteEndpoints();

        endpoints.MapGroup("/api/v1/collaboration/memos")
            .MapMemoEndpoints();

        return endpoints;
    }
}

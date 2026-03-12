using HomeHoney.Api.Application.Collaboration;
using HomeHoney.Api.Contracts.Collaboration;

namespace HomeHoney.Api.Endpoints.Collaboration;

public static class FridgeNoteEndpoints
{
    public static RouteGroupBuilder MapFridgeNoteEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(string.Empty, async (FridgeNoteService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        group.MapPost(string.Empty, async (FridgeNoteDto note, FridgeNoteService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.SaveAsync(note, cancellationToken)));

        group.MapDelete("/{recordId:guid}", async (Guid recordId, FridgeNoteService service, CancellationToken cancellationToken) =>
        {
            var result = await service.DeleteAsync(recordId, cancellationToken);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        });

        return group;
    }
}

using HomeHoney.Api.Application.Collaboration;
using HomeHoney.Api.Contracts.Collaboration;

namespace HomeHoney.Api.Endpoints.Collaboration;

public static class MemoEndpoints
{
    public static RouteGroupBuilder MapMemoEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(string.Empty, async (MemoService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        group.MapPost(string.Empty, async (MemoDto memo, MemoService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.SaveAsync(memo, cancellationToken)));

        group.MapDelete("/{recordId:guid}", async (Guid recordId, MemoService service, CancellationToken cancellationToken) =>
        {
            var result = await service.DeleteAsync(recordId, cancellationToken);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        });

        return group;
    }
}

using HomeHoney.Api.Application.Storage;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Storage;

namespace HomeHoney.Api.Endpoints.Admin;

public static class StorageAdminEndpoints
{
    public static RouteGroupBuilder MapStorageAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/storage", async (IBackendStorageSettingsService service, CancellationToken cancellationToken) =>
        {
            var settings = await service.GetDownstreamSettingsAsync(cancellationToken);
            return Results.Ok(settings);
        });

        group.MapPut("/storage", async (UpdateDownstreamStorageSettingsRequest request, IBackendStorageSettingsService service, CancellationToken cancellationToken) =>
        {
            var result = await service.SaveDownstreamSettingsAsync(request, cancellationToken);
            return result.Result.IsSuccess
                ? Results.Ok(result)
                : result.Result.Message.ToProblem(StatusCodes.Status400BadRequest, "下游存储配置无效");
        });

        return group;
    }
}

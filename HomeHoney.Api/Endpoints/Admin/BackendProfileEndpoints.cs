using HomeHoney.Api.Application.Storage;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Storage;

namespace HomeHoney.Api.Endpoints.Admin;

public static class BackendProfileEndpoints
{
    public static RouteGroupBuilder MapBackendProfileEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/backend-profile", async (IBackendStorageSettingsService service, CancellationToken cancellationToken) =>
        {
            var profile = await service.GetBackendProfileAsync(cancellationToken);
            return Results.Ok(profile);
        });

        group.MapPut("/backend-profile", async (UpdateBackendServiceProfileRequest request, IBackendStorageSettingsService service, CancellationToken cancellationToken) =>
        {
            var result = await service.SaveBackendProfileAsync(request, cancellationToken);
            return result.Result.IsSuccess
                ? Results.Ok(result)
                : result.Result.Message.ToProblem(StatusCodes.Status400BadRequest, "后端连接配置无效");
        });

        return group;
    }
}

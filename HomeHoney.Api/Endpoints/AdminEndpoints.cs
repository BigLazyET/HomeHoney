using HomeHoney.Api.Endpoints.Admin;

namespace HomeHoney.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin");
        group.MapBackendProfileEndpoints();
        group.MapStorageAdminEndpoints();
        return endpoints;
    }
}

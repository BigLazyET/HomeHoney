using HomeHoney.Api.Endpoints.Documents;

namespace HomeHoney.Api.Endpoints;

public static class DocumentsEndpoints
{
    public static IEndpointRouteBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/v1/documents/insurance")
            .MapInsuranceDocumentEndpoints();

        endpoints.MapGroup("/api/v1/documents/manuals")
            .MapManualDocumentEndpoints();

        return endpoints;
    }
}
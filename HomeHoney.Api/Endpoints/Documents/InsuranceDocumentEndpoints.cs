using HomeHoney.Api.Application.Documents;
using HomeHoney.Api.Contracts.Documents;

namespace HomeHoney.Api.Endpoints.Documents;

public static class InsuranceDocumentEndpoints
{
    public static RouteGroupBuilder MapInsuranceDocumentEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(string.Empty, async (InsuranceDocumentService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        group.MapGet("/{documentId:guid}", async (Guid documentId, InsuranceDocumentService service, CancellationToken cancellationToken) =>
        {
            var item = await service.GetByIdAsync(documentId, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        group.MapPost(string.Empty, async (UpsertInsuranceDocumentRequest request, InsuranceDocumentService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.SaveAsync(request, cancellationToken)));

        group.MapDelete("/{documentId:guid}", async (Guid documentId, InsuranceDocumentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.DeleteAsync(documentId, cancellationToken);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        });

        group.MapPost("/{documentId:guid}/files:upload", async (Guid documentId, IFormFile file, InsuranceDocumentService service, CancellationToken cancellationToken) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await service.UploadFileAsync(documentId, stream, file.FileName, file.ContentType, cancellationToken);
            return result.Result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }).DisableAntiforgery();

        group.MapGet("/{documentId:guid}/files/{fileId}:download", async (Guid documentId, string fileId, InsuranceDocumentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.DownloadFileAsync(documentId, cancellationToken);
            return result.IsSuccess
                ? Results.File(result.Content!, result.ContentType ?? "application/octet-stream", result.FileName)
                : Results.NotFound(result.Message);
        });

        return group;
    }
}

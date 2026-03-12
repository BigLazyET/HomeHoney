using HomeHoney.Api.Contracts.Collaboration;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;

namespace HomeHoney.Api.Application.Collaboration;

public sealed class FridgeNoteService
{
    private readonly CollaborationRepository _repository;

    public FridgeNoteService(CollaborationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FridgeNoteDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _repository.GetFridgeNotesAsync(cancellationToken)).Select(CollaborationDtoMapper.ToDto).ToList();

    public async Task<ApiOperationResult<FridgeNoteDto>> SaveAsync(FridgeNoteDto note, CancellationToken cancellationToken = default)
    {
        var model = CollaborationDtoMapper.ToModel(note);
        model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
        if (model.CreatedAt == default)
        {
            model.CreatedAt = DateTime.UtcNow;
        }

        model.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveFridgeNoteAsync(model, cancellationToken);
        return new(ApiOperationResult.Success("upsert-fridge-note", "冰箱贴已保存。", targetType: "fridge-note", targetId: model.Id), CollaborationDtoMapper.ToDto(model));
    }

    public async Task<ApiOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = await _repository.DeleteFridgeNoteAsync(id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-fridge-note", "冰箱贴已删除。", targetType: "fridge-note", targetId: id)
            : ApiOperationResult.Failure("delete-fridge-note", "未找到要删除的冰箱贴。", "not_found", targetType: "fridge-note", targetId: id);
    }
}

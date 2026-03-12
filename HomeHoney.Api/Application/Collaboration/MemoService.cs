using HomeHoney.Api.Contracts.Collaboration;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;

namespace HomeHoney.Api.Application.Collaboration;

public sealed class MemoService
{
    private readonly CollaborationRepository _repository;

    public MemoService(CollaborationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MemoDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _repository.GetMemosAsync(cancellationToken)).Select(CollaborationDtoMapper.ToDto).ToList();

    public async Task<ApiOperationResult<MemoDto>> SaveAsync(MemoDto memo, CancellationToken cancellationToken = default)
    {
        var model = CollaborationDtoMapper.ToModel(memo);
        model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
        if (model.CreatedAt == default)
        {
            model.CreatedAt = DateTime.UtcNow;
        }

        model.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveMemoAsync(model, cancellationToken);
        return new(ApiOperationResult.Success("upsert-memo", "备忘录已保存。", targetType: "memo", targetId: model.Id), CollaborationDtoMapper.ToDto(model));
    }

    public async Task<ApiOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = await _repository.DeleteMemoAsync(id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-memo", "备忘录已删除。", targetType: "memo", targetId: id)
            : ApiOperationResult.Failure("delete-memo", "未找到要删除的备忘录。", "not_found", targetType: "memo", targetId: id);
    }
}

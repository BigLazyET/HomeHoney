using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class CollaborationRepository
{
    private readonly IBusinessAggregateRepository _repository;

    public CollaborationRepository(IBusinessAggregateRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default)
        => _repository.GetFridgeNotesAsync(cancellationToken);

    public Task<FridgeNote?> GetFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetFridgeNoteAsync(id, cancellationToken);

    public Task SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default)
        => _repository.SaveFridgeNoteAsync(fridgeNote, cancellationToken);

    public Task<bool> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.DeleteFridgeNoteAsync(id, cancellationToken);

    public Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default)
        => _repository.GetMemosAsync(cancellationToken);

    public Task<Memo?> GetMemoAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetMemoAsync(id, cancellationToken);

    public Task SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default)
        => _repository.SaveMemoAsync(memo, cancellationToken);
}

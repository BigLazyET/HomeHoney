using HomeHoney.Models;

namespace HomeHoney.Services.Collaboration;

public interface ICollaborationApiClient
{
	Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default);

	Task<FridgeNote> SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default);

	Task<Memo> SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default);

	Task<bool> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default);

	Task<bool> DeleteMemoAsync(Guid id, CancellationToken cancellationToken = default);
}

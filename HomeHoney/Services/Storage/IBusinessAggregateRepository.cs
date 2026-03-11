using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public interface IBusinessAggregateRepository
{
    Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default);

    Task<FridgeNote?> GetFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default);

    Task<bool> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default);

    Task<Memo?> GetMemoAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HouseholdMember>> GetMembersAsync(CancellationToken cancellationToken = default);

    Task SaveMembersAsync(IReadOnlyList<HouseholdMember> members, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Space>> GetSpacesAsync(CancellationToken cancellationToken = default);

    Task SaveSpacesAsync(IReadOnlyList<Space> spaces, CancellationToken cancellationToken = default);

    Task<UserPreference?> GetUserPreferenceAsync(CancellationToken cancellationToken = default);

    Task SaveUserPreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default);
}

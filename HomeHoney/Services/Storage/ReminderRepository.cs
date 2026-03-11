using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class ReminderRepository
{
    private readonly ILocalCacheStore _cacheStore;
    private const string ReminderCacheKey = "reminders.upcoming";

    public ReminderRepository(ILocalCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    public Task<IReadOnlyList<ReminderItem>?> GetUpcomingSnapshotAsync(CancellationToken cancellationToken = default)
        => _cacheStore.GetAsync<IReadOnlyList<ReminderItem>>(ReminderCacheKey, cancellationToken);

    public Task SaveUpcomingSnapshotAsync(IReadOnlyList<ReminderItem> reminders, CancellationToken cancellationToken = default)
        => _cacheStore.SetAsync(ReminderCacheKey, reminders, cancellationToken);
}

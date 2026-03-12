using HomeHoney.Models;

namespace HomeHoney.Services.Reminders;

public interface IReminderApiClient
{
	Task<IReadOnlyList<ReminderItem>> GetUpcomingAsync(int days = 30, CancellationToken cancellationToken = default);
}

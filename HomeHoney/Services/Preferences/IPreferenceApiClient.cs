using HomeHoney.Models;

namespace HomeHoney.Services.Preferences;

public interface IPreferenceApiClient
{
	Task<UserPreference?> GetCurrentAsync(CancellationToken cancellationToken = default);

	Task<UserPreference?> UpdateAsync(UserPreference preference, CancellationToken cancellationToken = default);
}

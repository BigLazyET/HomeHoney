using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class PreferenceRepository
{
    private readonly IBusinessAggregateRepository _repository;

    public PreferenceRepository(IBusinessAggregateRepository repository)
    {
        _repository = repository;
    }

    public Task<UserPreference?> GetUserPreferenceAsync(CancellationToken cancellationToken = default)
        => _repository.GetUserPreferenceAsync(cancellationToken);

    public Task SaveUserPreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default)
        => _repository.SaveUserPreferenceAsync(preference, cancellationToken);
}

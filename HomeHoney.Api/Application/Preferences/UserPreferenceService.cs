using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Preferences;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;

namespace HomeHoney.Api.Application.Preferences;

public sealed class UserPreferenceService
{
    private readonly PreferenceRepository _repository;

    public UserPreferenceService(PreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferenceDto> GetCurrentAsync(CancellationToken cancellationToken = default)
        => PreferenceDtoMapper.ToDto(await _repository.GetCurrentAsync(cancellationToken));

    public async Task<ApiOperationResult<UserPreferenceDto>> SaveAsync(UserPreferenceDto preference, CancellationToken cancellationToken = default)
    {
        var model = PreferenceDtoMapper.ToModel(preference);
        await _repository.SaveAsync(model, cancellationToken);
        return new(ApiOperationResult.Success("update-preferences", "用户偏好已更新。", targetType: "preference"), PreferenceDtoMapper.ToDto(model));
    }
}

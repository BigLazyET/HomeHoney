using HomeHoney.Api.Application.Shared;
using HomeHoney.Models;

namespace HomeHoney.Api.Infrastructure.Mongo.Repositories;

public sealed class ReferenceDataRepository
{
    private readonly SeedDataService _seedDataService;

    public ReferenceDataRepository(SeedDataService seedDataService)
    {
        _seedDataService = seedDataService;
    }

    public IReadOnlyList<HouseholdMember> GetMembers() => _seedDataService.GetMembers();

    public IReadOnlyList<Space> GetSpaces() => _seedDataService.GetSpaces();
}

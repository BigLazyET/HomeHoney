using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;

namespace HomeHoney.Api.Application.Shared;

public sealed class SeedDataService
{
    private readonly List<InsuranceRecord> _insuranceRecords;
    private readonly List<ManualRecord> _manualRecords;
    private readonly List<FridgeNote> _fridgeNotes;
    private readonly List<Memo> _memos;
    private readonly List<HouseholdMember> _members;
    private readonly List<Space> _spaces;
    private UserPreference _userPreference;

    public SeedDataService()
    {
        var documentCatalogService = new DocumentCatalogService();
        var collaborationService = new FamilyCollaborationService();

        _insuranceRecords = documentCatalogService.GetInsuranceRecordsAsync().GetAwaiter().GetResult().ToList();
        _manualRecords = documentCatalogService.GetManualRecordsAsync().GetAwaiter().GetResult().ToList();
        _fridgeNotes = collaborationService.GetFridgeNotesAsync().GetAwaiter().GetResult().ToList();
        _memos = collaborationService.GetMemosAsync().GetAwaiter().GetResult().ToList();
        _members = documentCatalogService.GetMembers().ToList();
        _spaces = documentCatalogService.GetSpaces().ToList();
        _userPreference = new UserPreference();
    }

    public Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync()
        => Task.FromResult<IReadOnlyList<InsuranceRecord>>(_insuranceRecords.ToList());

    public Task SaveInsuranceRecordAsync(InsuranceRecord record)
    {
        Upsert(_insuranceRecords, record, item => item.Id == record.Id);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteInsuranceRecordAsync(Guid id)
        => Task.FromResult(Delete(_insuranceRecords, item => item.Id == id));

    public Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync()
        => Task.FromResult<IReadOnlyList<ManualRecord>>(_manualRecords.ToList());

    public Task SaveManualRecordAsync(ManualRecord record)
    {
        Upsert(_manualRecords, record, item => item.Id == record.Id);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteManualRecordAsync(Guid id)
        => Task.FromResult(Delete(_manualRecords, item => item.Id == id));

    public IReadOnlyList<HouseholdMember> GetMembers()
        => _members.ToList();

    public IReadOnlyList<Space> GetSpaces()
        => _spaces.ToList();

    public Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync()
        => Task.FromResult<IReadOnlyList<FridgeNote>>(_fridgeNotes.OrderByDescending(note => note.IsPinned).ThenBy(note => note.DueAt).ToList());

    public Task SaveFridgeNoteAsync(FridgeNote fridgeNote)
    {
        Upsert(_fridgeNotes, fridgeNote, item => item.Id == fridgeNote.Id);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteFridgeNoteAsync(Guid id)
        => Task.FromResult(Delete(_fridgeNotes, item => item.Id == id));

    public Task<IReadOnlyList<Memo>> GetMemosAsync()
        => Task.FromResult<IReadOnlyList<Memo>>(_memos.OrderBy(item => item.DueAt).ToList());

    public Task SaveMemoAsync(Memo memo)
    {
        Upsert(_memos, memo, item => item.Id == memo.Id);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteMemoAsync(Guid id)
        => Task.FromResult(Delete(_memos, item => item.Id == id));

    public UserPreference GetDefaultPreference()
        => _userPreference;

    public Task SavePreferenceAsync(UserPreference preference)
    {
        _userPreference = preference;
        return Task.CompletedTask;
    }

    private static void Upsert<T>(List<T> target, T value, Func<T, bool> predicate)
    {
        var existing = target.FirstOrDefault(predicate);
        if (existing is null)
        {
            target.Add(value);
            return;
        }

        var index = target.IndexOf(existing);
        target[index] = value;
    }

    private static bool Delete<T>(List<T> target, Func<T, bool> predicate)
    {
        var existing = target.FirstOrDefault(predicate);
        if (existing is null)
        {
            return false;
        }

        target.Remove(existing);
        return true;
    }
}
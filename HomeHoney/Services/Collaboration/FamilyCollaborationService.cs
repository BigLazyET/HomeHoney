using System.Diagnostics;
using HomeHoney.Models;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHoney.Services.Collaboration;

public sealed class FamilyCollaborationService
{
    public string? LastErrorMessage { get; private set; }

    private const string RequiredFieldCode = "required";

    private readonly List<FridgeNote> _fridgeNotes = [];

    private readonly List<Memo> _memos = [];

    private readonly IStorageConnectionProfileService? _storageConnectionProfileService;
    private readonly ICollaborationApiClient? _collaborationApiClient;

    public FamilyCollaborationService()
    {
    }

    [ActivatorUtilitiesConstructor]
    public FamilyCollaborationService(
        IStorageConnectionProfileService storageConnectionProfileService,
        ICollaborationApiClient collaborationApiClient)
    {
        _storageConnectionProfileService = storageConnectionProfileService;
        _collaborationApiClient = collaborationApiClient;
    }

    public async Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync()
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _collaborationApiClient is not null)
        {
            try
            {
                var remote = (await _collaborationApiClient.GetFridgeNotesAsync()).OrderByDescending(note => note.IsPinned).ThenBy(note => note.DueAt).ToList();
                ReplaceLocal(_fridgeNotes, remote);
                return remote;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "读取冰箱贴失败，当前展示的是应用内现有内容。";
                Debug.WriteLine($"GetFridgeNotesAsync Error: {ex}");
            }
        }

        return _fridgeNotes.OrderByDescending(note => note.IsPinned).ThenBy(note => note.DueAt).ToList();
    }

    public async Task<IReadOnlyList<Memo>> GetMemosAsync()
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _collaborationApiClient is not null)
        {
            try
            {
                var remote = (await _collaborationApiClient.GetMemosAsync()).OrderBy(item => item.DueAt).ToList();
                ReplaceLocal(_memos, remote);
                return remote;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "读取备忘录失败，当前展示的是应用内现有内容。";
                Debug.WriteLine($"GetMemosAsync Error: {ex}");
            }
        }

        return _memos.OrderBy(item => item.DueAt).ToList();
    }

    public async Task<FridgeNote?> GetFridgeNoteAsync(Guid id) => (await GetFridgeNotesAsync()).FirstOrDefault(note => note.Id == id);

    public async Task<Memo?> GetMemoAsync(Guid id) => (await GetMemosAsync()).FirstOrDefault(memo => memo.Id == id);

    public async Task SaveFridgeNoteAsync(FridgeNote fridgeNote)
    {
        LastErrorMessage = null;
        NormalizeFridgeNote(fridgeNote);
        fridgeNote.CreatedAt = fridgeNote.CreatedAt == default ? DateTime.Now : fridgeNote.CreatedAt;
        fridgeNote.UpdatedAt = DateTime.Now;

        if (await UseBackendApiAsync() && _collaborationApiClient is not null)
        {
            try
            {
                var saved = await _collaborationApiClient.SaveFridgeNoteAsync(fridgeNote);
                var existingRemote = _fridgeNotes.FirstOrDefault(item => item.Id == saved.Id);
                if (existingRemote is null)
                {
                    _fridgeNotes.Add(saved);
                }
                else
                {
                    _fridgeNotes[_fridgeNotes.IndexOf(existingRemote)] = saved;
                }

                return;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "保存冰箱贴失败，未能同步到后端，请稍后重试。";
                Debug.WriteLine($"SaveFridgeNoteAsync Error: {ex}");
            }
        }

        var existing = _fridgeNotes.FirstOrDefault(item => item.Id == fridgeNote.Id);
        if (existing is null)
        {
            fridgeNote.Id = fridgeNote.Id == Guid.Empty ? Guid.NewGuid() : fridgeNote.Id;
            _fridgeNotes.Add(fridgeNote);
        }
        else
        {
            var index = _fridgeNotes.IndexOf(existing);
            _fridgeNotes[index] = fridgeNote;
        }

    }

    public async Task SaveMemoAsync(Memo memo)
    {
        LastErrorMessage = null;
        NormalizeMemo(memo);
        memo.CreatedAt = memo.CreatedAt == default ? DateTime.Now : memo.CreatedAt;
        memo.UpdatedAt = DateTime.Now;

        if (await UseBackendApiAsync() && _collaborationApiClient is not null)
        {
            try
            {
                var saved = await _collaborationApiClient.SaveMemoAsync(memo);
                var existingRemote = _memos.FirstOrDefault(item => item.Id == saved.Id);
                if (existingRemote is null)
                {
                    _memos.Add(saved);
                }
                else
                {
                    _memos[_memos.IndexOf(existingRemote)] = saved;
                }

                return;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "保存备忘录失败，未能同步到后端，请稍后重试。";
                Debug.WriteLine($"SaveMemoAsync Error: {ex}");
            }
        }

        var existing = _memos.FirstOrDefault(item => item.Id == memo.Id);
        if (existing is null)
        {
            memo.Id = memo.Id == Guid.Empty ? Guid.NewGuid() : memo.Id;
            _memos.Add(memo);
        }
        else
        {
            var index = _memos.IndexOf(existing);
            _memos[index] = memo;
        }

    }

    public async Task<bool> DeleteFridgeNoteAsync(Guid id)
    {
        LastErrorMessage = null;
        var existing = _fridgeNotes.FirstOrDefault(item => item.Id == id);
        if (existing is null)
        {
            return false;
        }

        if (await UseBackendApiAsync() && _collaborationApiClient is not null)
        {
            try
            {
                var deleted = await _collaborationApiClient.DeleteFridgeNoteAsync(id);
                if (deleted)
                {
                    _fridgeNotes.Remove(existing);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "删除冰箱贴失败，后端未确认本次删除操作。";
                Debug.WriteLine($"DeleteFridgeNoteAsync Error: {ex}");
            }
        }

        _fridgeNotes.Remove(existing);
        return true;
    }

    public FridgeNote CreateFridgeNoteTemplate() => new()
    {
        Id = Guid.NewGuid(),
        DueAt = null,
        ColorStyle = string.Empty,
    };

    public Memo CreateMemoTemplate() => new()
    {
        Id = Guid.NewGuid(),
        DueAt = null,
    };

    public IReadOnlyList<ValidationIssue> ValidateFridgeNote(FridgeNote note, bool hasCategorySelection = true, bool hasPrioritySelection = true)
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(note.Title))
        {
            issues.Add(new("Title", "请填写标题。", Code: RequiredFieldCode));
        }

        if (string.IsNullOrWhiteSpace(note.Content))
        {
            issues.Add(new("Content", "请填写内容。", Code: RequiredFieldCode));
        }

        if (!hasCategorySelection)
        {
            issues.Add(new("Category", "请选择冰箱贴分类。", Code: RequiredFieldCode));
        }

        if (!hasPrioritySelection)
        {
            issues.Add(new("Priority", "请选择优先级。", Code: RequiredFieldCode));
        }

        return issues;
    }

    public IReadOnlyList<ValidationIssue> ValidateMemo(Memo memo, bool hasCategorySelection = true, bool hasImportanceSelection = true, bool hasStatusSelection = true)
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(memo.Title))
        {
            issues.Add(new("Title", "请填写标题。", Code: RequiredFieldCode));
        }

        if (string.IsNullOrWhiteSpace(memo.Content))
        {
            issues.Add(new("Content", "请填写内容。", Code: RequiredFieldCode));
        }

        if (!hasCategorySelection)
        {
            issues.Add(new("MemoCategory", "请选择分类。", Code: RequiredFieldCode));
        }

        if (!hasImportanceSelection)
        {
            issues.Add(new("Importance", "请选择重要级别。", Code: RequiredFieldCode));
        }

        if (!hasStatusSelection)
        {
            issues.Add(new("Status", "请选择当前状态。", Code: RequiredFieldCode));
        }

        return issues;
    }

    public string GetNoteColorStyle(FridgeNote note)
        => !string.IsNullOrWhiteSpace(note.ColorStyle) ? note.ColorStyle : note.Priority switch
        {
            NotePriority.Urgent => "pink",
            NotePriority.Important => "yellow",
            _ => "blue",
        };

    public static string NormalizeUserInput(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    public async Task SetFridgeNoteCompletedAsync(Guid id, bool isCompleted)
    {
        var note = _fridgeNotes.FirstOrDefault(item => item.Id == id);
        if (note is not null)
        {
            note.IsCompleted = isCompleted;
            note.UpdatedAt = DateTime.Now;
            await SaveFridgeNoteAsync(note);
        }
    }

    public async Task SetMemoStatusAsync(Guid id, MemoStatus status)
    {
        var memo = _memos.FirstOrDefault(item => item.Id == id);
        if (memo is not null)
        {
            memo.Status = status;
            memo.UpdatedAt = DateTime.Now;
            await SaveMemoAsync(memo);
        }
    }

    public async Task<bool> DeleteMemoAsync(Guid id)
    {
        LastErrorMessage = null;
        var existing = _memos.FirstOrDefault(item => item.Id == id);
        if (existing is null)
        {
            return false;
        }

        if (await UseBackendApiAsync() && _collaborationApiClient is not null)
        {
            try
            {
                var deleted = await _collaborationApiClient.DeleteMemoAsync(id);
                if (deleted)
                {
                    _memos.Remove(existing);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "删除备忘录失败，后端未确认本次删除操作。";
                Debug.WriteLine($"DeleteMemoAsync Error: {ex}");
            }
        }

        _memos.Remove(existing);
        return true;
    }

    public IEnumerable<CollaborationSummary> GetRecentMessages(int maxItems = 3)
    {
        var fridge = _fridgeNotes.Select(note => new CollaborationSummary(note.Id, PresentationFallbacks.TextOrFallback(note.Title, "未命名冰箱贴"), PresentationFallbacks.TextOrFallback(note.Content, "暂未填写摘要"), AppRoutes.FridgeNoteEdit(note.Id), note.Priority.ToString(), note.DueAt));
        var memos = _memos.Select(memo => new CollaborationSummary(memo.Id, PresentationFallbacks.TextOrFallback(memo.Title, "未命名备忘录"), PresentationFallbacks.TextOrFallback(memo.Content, "暂未填写摘要"), AppRoutes.MemoDetail(memo.Id), memo.Importance.ToString(), memo.DueAt));
        return fridge.Concat(memos).OrderBy(item => item.DueAt ?? DateTime.MaxValue).Take(maxItems).ToList();
    }

    private async Task<bool> UseBackendApiAsync()
    {
        if (_storageConnectionProfileService is null)
        {
            return false;
        }

        var profile = await _storageConnectionProfileService.GetActiveProfileAsync();
        var apiBaseUrl = string.IsNullOrWhiteSpace(profile.ApiBaseUrl)
            ? StorageConnectionProfile.DefaultBackendApiBaseUrl
            : profile.ApiBaseUrl;

        return profile.IsActive
            && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out _);
    }

    private static void ReplaceLocal<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private void NormalizeFridgeNote(FridgeNote fridgeNote)
    {
        fridgeNote.Title = NormalizeUserInput(fridgeNote.Title);
        fridgeNote.Content = NormalizeUserInput(fridgeNote.Content);
        fridgeNote.ColorStyle = GetNoteColorStyle(fridgeNote);
    }

    private static void NormalizeMemo(Memo memo)
    {
        memo.Title = NormalizeUserInput(memo.Title);
        memo.Content = NormalizeUserInput(memo.Content);
    }
}

public sealed record CollaborationSummary(Guid Id, string Title, string Summary, string Href, string Importance, DateTime? DueAt);

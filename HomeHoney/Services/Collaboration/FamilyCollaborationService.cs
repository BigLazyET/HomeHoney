using HomeHoney.Models;
using HomeHoney.Services.Navigation;

namespace HomeHoney.Services.Collaboration;

public sealed class FamilyCollaborationService
{
    private readonly List<FridgeNote> _fridgeNotes =
    [
        new()
        {
            Id = Guid.Parse("7b7d9983-8f89-49b4-8fd1-9cbf00221001"),
            Title = "周末采购",
            Content = "牛奶、鸡蛋、厨房纸、猫粮",
            Category = FridgeNoteCategory.Shopping,
            Priority = NotePriority.Important,
            ColorStyle = "yellow",
            IsPinned = true,
            DueAt = DateTime.Today.AddDays(1).AddHours(10),
        },
        new()
        {
            Id = Guid.Parse("7b7d9983-8f89-49b4-8fd1-9cbf00221002"),
            Title = "接娃提醒",
            Content = "周三 17:20 幼儿园门口接孩子，记得带校服袋。",
            Category = FridgeNoteCategory.Reminder,
            Priority = NotePriority.Urgent,
            ColorStyle = "pink",
            DueAt = DateTime.Today.AddDays(2).AddHours(17),
        },
    ];

    private readonly List<Memo> _memos =
    [
        new()
        {
            Id = Guid.Parse("0d83f8fe-91ee-4f16-92bb-3ce2c1931001"),
            Title = "家庭证件整理计划",
            Content = "本月把身份证、户口本、出生证明、保单扫描件统一整理到 HomeHoney。",
            MemoCategory = MemoCategory.LongTermPlan,
            Importance = MemoImportance.Important,
            DueAt = DateTime.Today.AddDays(10),
        },
        new()
        {
            Id = Guid.Parse("0d83f8fe-91ee-4f16-92bb-3ce2c1931002"),
            Title = "儿童疫苗记录补充",
            Content = "下次社区门诊前确认疫苗本、医保卡、既往病史备忘已同步。",
            MemoCategory = MemoCategory.Medical,
            Importance = MemoImportance.Critical,
            DueAt = DateTime.Today.AddDays(6),
        },
    ];

    public Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync() => Task.FromResult<IReadOnlyList<FridgeNote>>(_fridgeNotes.OrderByDescending(note => note.IsPinned).ThenBy(note => note.DueAt).ToList());

    public Task<IReadOnlyList<Memo>> GetMemosAsync() => Task.FromResult<IReadOnlyList<Memo>>(_memos.OrderBy(item => item.DueAt).ToList());

    public Task<FridgeNote?> GetFridgeNoteAsync(Guid id) => Task.FromResult(_fridgeNotes.FirstOrDefault(note => note.Id == id));

    public Task<Memo?> GetMemoAsync(Guid id) => Task.FromResult(_memos.FirstOrDefault(memo => memo.Id == id));

    public Task SaveFridgeNoteAsync(FridgeNote fridgeNote)
    {
        fridgeNote.UpdatedAt = DateTime.Now;
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

        return Task.CompletedTask;
    }

    public Task SaveMemoAsync(Memo memo)
    {
        memo.UpdatedAt = DateTime.Now;
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

        return Task.CompletedTask;
    }

    public FridgeNote CreateFridgeNoteTemplate() => new() { DueAt = DateTime.Today.AddDays(1).AddHours(18) };

    public Memo CreateMemoTemplate() => new() { DueAt = DateTime.Today.AddDays(7).AddHours(9) };

    public Task SetFridgeNoteCompletedAsync(Guid id, bool isCompleted)
    {
        var note = _fridgeNotes.FirstOrDefault(item => item.Id == id);
        if (note is not null)
        {
            note.IsCompleted = isCompleted;
            note.UpdatedAt = DateTime.Now;
        }

        return Task.CompletedTask;
    }

    public Task SetMemoStatusAsync(Guid id, MemoStatus status)
    {
        var memo = _memos.FirstOrDefault(item => item.Id == id);
        if (memo is not null)
        {
            memo.Status = status;
            memo.UpdatedAt = DateTime.Now;
        }

        return Task.CompletedTask;
    }

    public IEnumerable<CollaborationSummary> GetRecentMessages(int maxItems = 3)
    {
        var fridge = _fridgeNotes.Select(note => new CollaborationSummary(note.Id, note.Title, note.Content, AppRoutes.FridgeNotes, note.Priority.ToString(), note.DueAt));
        var memos = _memos.Select(memo => new CollaborationSummary(memo.Id, memo.Title, memo.Content, AppRoutes.MemoDetail(memo.Id), memo.Importance.ToString(), memo.DueAt));
        return fridge.Concat(memos).OrderBy(item => item.DueAt ?? DateTime.MaxValue).Take(maxItems).ToList();
    }
}

public sealed record CollaborationSummary(Guid Id, string Title, string Summary, string Href, string Importance, DateTime? DueAt);

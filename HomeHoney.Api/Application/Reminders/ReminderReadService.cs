using HomeHoney.Api.Application.Collaboration;
using HomeHoney.Api.Application.Documents;
using HomeHoney.Models;
using HomeHoney.Services.Navigation;

namespace HomeHoney.Api.Application.Reminders;

public sealed class ReminderReadService
{
    private readonly DocumentReadService _documentReadService;
    private readonly CollaborationReadService _collaborationReadService;

    public ReminderReadService(DocumentReadService documentReadService, CollaborationReadService collaborationReadService)
    {
        _documentReadService = documentReadService;
        _collaborationReadService = collaborationReadService;
    }

    public async Task<IReadOnlyList<ReminderItem>> GetUpcomingAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.Today.AddDays(days);
        var reminders = new List<ReminderItem>();

        var insuranceRecords = await _documentReadService.GetInsuranceRecordsAsync(cancellationToken);
        reminders.AddRange(insuranceRecords
            .Where(record => record.Status != InsuranceStatus.Archived)
            .Select(record =>
            {
                var dueAt = record.ExpiryDate.ToDateTime(TimeOnly.MinValue);
                return new ReminderItem
                {
                    SourceType = ReminderSourceType.Insurance,
                    SourceId = record.Id,
                    Title = record.PolicyName,
                    Summary = $"{record.ProviderName} · 到期日 {record.ExpiryDate:yyyy-MM-dd}",
                    DueAt = dueAt,
                    Priority = dueAt <= DateTime.Today.AddDays(7) ? ReminderPriority.High : ReminderPriority.Medium,
                    Status = dueAt <= DateTime.Today.AddDays(7) ? ReminderStatus.Upcoming : ReminderStatus.Pending,
                    Href = AppRoutes.InsuranceDetail(record.Id),
                };
            })
            .Where(item => item.DueAt <= threshold));

        var manuals = await _documentReadService.GetManualRecordsAsync(cancellationToken);
        reminders.AddRange(manuals
            .Select(record =>
            {
                var dueAt = record.WarrantyExpiryDate.ToDateTime(TimeOnly.MinValue);
                return new ReminderItem
                {
                    SourceType = ReminderSourceType.Manual,
                    SourceId = record.Id,
                    Title = record.DeviceName,
                    Summary = $"保修截止 {record.WarrantyExpiryDate:yyyy-MM-dd}",
                    DueAt = dueAt,
                    Priority = dueAt <= DateTime.Today.AddDays(14) ? ReminderPriority.High : ReminderPriority.Medium,
                    Status = dueAt <= DateTime.Today.AddDays(14) ? ReminderStatus.Upcoming : ReminderStatus.Pending,
                    Href = AppRoutes.ManualDetail(record.Id),
                };
            })
            .Where(item => item.DueAt <= threshold));

        var fridgeNotes = await _collaborationReadService.GetFridgeNotesAsync(cancellationToken);
        reminders.AddRange(fridgeNotes
            .Where(note => note.DueAt is not null && !note.IsCompleted)
            .Select(note => new ReminderItem
            {
                SourceType = ReminderSourceType.FridgeNote,
                SourceId = note.Id,
                Title = note.Title,
                Summary = note.Content,
                DueAt = note.DueAt ?? DateTime.Today,
                Priority = note.Priority switch
                {
                    NotePriority.Urgent => ReminderPriority.High,
                    NotePriority.Important => ReminderPriority.Medium,
                    _ => ReminderPriority.Low,
                },
                Status = ReminderStatus.Pending,
                Href = AppRoutes.FridgeNoteEdit(note.Id),
            })
            .Where(item => item.DueAt <= threshold));

        var memos = await _collaborationReadService.GetMemosAsync(cancellationToken);
        reminders.AddRange(memos
            .Where(memo => memo.DueAt is not null && memo.Status != MemoStatus.Archived)
            .Select(memo => new ReminderItem
            {
                SourceType = ReminderSourceType.Memo,
                SourceId = memo.Id,
                Title = memo.Title,
                Summary = memo.Content,
                DueAt = memo.DueAt ?? DateTime.Today,
                Priority = memo.Importance switch
                {
                    MemoImportance.Critical => ReminderPriority.High,
                    MemoImportance.Important => ReminderPriority.Medium,
                    _ => ReminderPriority.Low,
                },
                Status = memo.Status == MemoStatus.Completed ? ReminderStatus.Completed : ReminderStatus.Pending,
                Href = AppRoutes.MemoDetail(memo.Id),
            })
            .Where(item => item.DueAt <= threshold));

        return reminders.OrderBy(item => item.DueAt).ToList();
    }
}
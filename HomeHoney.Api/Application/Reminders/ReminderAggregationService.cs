using HomeHoney.Api.Contracts.Reminders;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;
using HomeHoney.Models;
using HomeHoney.Services.Navigation;

namespace HomeHoney.Api.Application.Reminders;

public sealed class ReminderAggregationService
{
    private readonly DocumentRepository _documentRepository;
    private readonly CollaborationRepository _collaborationRepository;

    public ReminderAggregationService(DocumentRepository documentRepository, CollaborationRepository collaborationRepository)
    {
        _documentRepository = documentRepository;
        _collaborationRepository = collaborationRepository;
    }

    public async Task<IReadOnlyList<ReminderViewDto>> GetUpcomingAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.Today.AddDays(days);
        var reminders = new List<ReminderItem>();

        var insuranceRecords = await _documentRepository.GetInsuranceRecordsAsync(cancellationToken);
        reminders.AddRange(insuranceRecords
            .Where(record => record.Status != InsuranceStatus.Archived)
            .Select(record =>
            {
                var dueAt = record.ExpiryDate.ToDateTime(TimeOnly.MinValue);
                return new ReminderItem
                {
                    SourceType = ReminderSourceType.Insurance,
                    SourceId = record.Id,
                    Title = FallbackText(record.PolicyName, "未命名保单"),
                    Summary = BuildInsuranceSummary(record.ProviderName, record.ExpiryDate),
                    DueAt = dueAt,
                    Priority = dueAt <= DateTime.Today.AddDays(7) ? ReminderPriority.High : ReminderPriority.Medium,
                    Status = dueAt <= DateTime.Today.AddDays(7) ? ReminderStatus.Upcoming : ReminderStatus.Pending,
                    Href = AppRoutes.InsuranceDetail(record.Id),
                };
            })
            .Where(item => item.DueAt <= threshold));

        var manuals = await _documentRepository.GetManualRecordsAsync(cancellationToken);
        reminders.AddRange(manuals
            .Select(record =>
            {
                var dueAt = record.WarrantyExpiryDate.ToDateTime(TimeOnly.MinValue);
                return new ReminderItem
                {
                    SourceType = ReminderSourceType.Manual,
                    SourceId = record.Id,
                    Title = FallbackText(record.DeviceName, "未命名设备"),
                    Summary = $"保修截止 {record.WarrantyExpiryDate:yyyy-MM-dd}",
                    DueAt = dueAt,
                    Priority = dueAt <= DateTime.Today.AddDays(14) ? ReminderPriority.High : ReminderPriority.Medium,
                    Status = dueAt <= DateTime.Today.AddDays(14) ? ReminderStatus.Upcoming : ReminderStatus.Pending,
                    Href = AppRoutes.ManualDetail(record.Id),
                };
            })
            .Where(item => item.DueAt <= threshold));

        var fridgeNotes = await _collaborationRepository.GetFridgeNotesAsync(cancellationToken);
        reminders.AddRange(fridgeNotes
            .Where(note => note.DueAt is not null && !note.IsCompleted)
            .Select(note => new ReminderItem
            {
                SourceType = ReminderSourceType.FridgeNote,
                SourceId = note.Id,
                Title = FallbackText(note.Title, "未命名冰箱贴"),
                Summary = FallbackText(note.Content, "暂未填写内容"),
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

        var memos = await _collaborationRepository.GetMemosAsync(cancellationToken);
        reminders.AddRange(memos
            .Where(memo => memo.DueAt is not null && memo.Status != MemoStatus.Archived)
            .Select(memo => new ReminderItem
            {
                SourceType = ReminderSourceType.Memo,
                SourceId = memo.Id,
                Title = FallbackText(memo.Title, "未命名备忘录"),
                Summary = FallbackText(memo.Content, "暂未填写内容"),
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

        return reminders.OrderBy(item => item.DueAt).Select(ReminderDtoMapper.ToDto).ToList();
    }

    private static string FallbackText(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string BuildInsuranceSummary(string? providerName, DateOnly expiryDate)
        => $"{FallbackText(providerName, "保险公司待补充")} · 到期日 {expiryDate:yyyy-MM-dd}";
}

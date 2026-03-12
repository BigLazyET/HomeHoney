using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace HomeHoney.Services.Reminders;

public sealed class ReminderCenterService
{
    public string? LastErrorMessage { get; private set; }

    private readonly DocumentCatalogService _documentCatalogService;
    private readonly FamilyCollaborationService _familyCollaborationService;
    private readonly IReminderApiClient? _reminderApiClient;
    private readonly Storage.IStorageConnectionProfileService? _storageConnectionProfileService;

    public ReminderCenterService(DocumentCatalogService documentCatalogService, FamilyCollaborationService familyCollaborationService)
    {
        _documentCatalogService = documentCatalogService;
        _familyCollaborationService = familyCollaborationService;
    }

    [ActivatorUtilitiesConstructor]
    public ReminderCenterService(
        DocumentCatalogService documentCatalogService,
        FamilyCollaborationService familyCollaborationService,
        IReminderApiClient reminderApiClient,
        Storage.IStorageConnectionProfileService storageConnectionProfileService)
        : this(documentCatalogService, familyCollaborationService)
    {
        _reminderApiClient = reminderApiClient;
        _storageConnectionProfileService = storageConnectionProfileService;
    }

    public async Task<IReadOnlyList<ReminderItem>> GetUpcomingAsync(int days = 30)
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _reminderApiClient is not null)
        {
            try
            {
                return await _reminderApiClient.GetUpcomingAsync(days);
            }
            catch (Exception ex)
            {
                // Fall back to local aggregation when the backend service is unavailable.
                LastErrorMessage = "读取提醒中心失败，当前展示的是应用内现有内容。";
                Debug.WriteLine($"GetUpcomingAsync Error: {ex}");
            }
        }

        var threshold = DateTime.Today.AddDays(days);
        var reminders = new List<ReminderItem>();

        var insuranceRecords = await _documentCatalogService.GetInsuranceRecordsAsync();
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

        var manuals = await _documentCatalogService.GetManualRecordsAsync();
        reminders.AddRange(manuals.Select(record =>
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

        var fridgeNotes = await _familyCollaborationService.GetFridgeNotesAsync();
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

        var memos = await _familyCollaborationService.GetMemosAsync();
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
}

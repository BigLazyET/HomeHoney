using HomeHoney.Api.Contracts.Reminders;
using HomeHoney.Models;

namespace HomeHoney.Api.Application.Reminders;

public static class ReminderDtoMapper
{
    public static ReminderViewDto ToDto(ReminderItem reminder)
        => new()
        {
            Id = reminder.Id,
            SourceType = reminder.SourceType,
            SourceId = reminder.SourceId,
            Title = reminder.Title,
            Summary = reminder.Summary,
            DueAt = reminder.DueAt,
            Priority = reminder.Priority,
            Status = reminder.Status,
            OwnerMemberId = reminder.OwnerMemberId,
            Href = reminder.Href,
            CreatedAt = reminder.CreatedAt,
        };
}

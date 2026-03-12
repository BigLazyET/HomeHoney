using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Reminders;

public sealed class ReminderViewDto
{
    public Guid Id { get; set; }

    public ReminderSourceType SourceType { get; set; }

    public Guid SourceId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime DueAt { get; set; }

    public ReminderPriority Priority { get; set; } = ReminderPriority.Medium;

    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

    public Guid? OwnerMemberId { get; set; }

    public string Href { get; set; } = "/";

    public DateTime CreatedAt { get; set; }
}

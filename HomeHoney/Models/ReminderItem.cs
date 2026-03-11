namespace HomeHoney.Models;

public enum ReminderSourceType
{
    Insurance,
    Manual,
    FridgeNote,
    Memo,
}

public enum ReminderPriority
{
    Low,
    Medium,
    High,
}

public enum ReminderStatus
{
    Pending,
    Upcoming,
    Completed,
    Ignored,
}

public sealed class ReminderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public ReminderSourceType SourceType { get; set; }

    public Guid SourceId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime DueAt { get; set; } = DateTime.Now;

    public ReminderPriority Priority { get; set; } = ReminderPriority.Medium;

    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

    public Guid? OwnerMemberId { get; set; }

    public string Href { get; set; } = "/";

    public bool UsesCachedData { get; set; }

    public string? DataStateMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

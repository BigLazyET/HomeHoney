namespace HomeHoney.Models;

public enum FridgeNoteCategory
{
    Shopping,
    Reminder,
    Message,
    Todo,
}

public enum NotePriority
{
    Normal,
    Important,
    Urgent,
}

public sealed class FridgeNote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public FridgeNoteCategory Category { get; set; }

    public NotePriority Priority { get; set; }

    public string ColorStyle { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    public bool IsCompleted { get; set; }

    public Guid? OwnerMemberId { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Collaboration;

public sealed class FridgeNoteDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public FridgeNoteCategory Category { get; set; } = FridgeNoteCategory.Reminder;

    public NotePriority Priority { get; set; } = NotePriority.Normal;

    public string ColorStyle { get; set; } = "yellow";

    public bool IsPinned { get; set; }

    public bool IsCompleted { get; set; }

    public Guid? OwnerMemberId { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

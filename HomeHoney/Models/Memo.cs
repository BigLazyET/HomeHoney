namespace HomeHoney.Models;

public enum MemoCategory
{
    FamilyRule,
    LongTermPlan,
    ShoppingPlan,
    Medical,
    Education,
    Other,
}

public enum MemoImportance
{
    Normal,
    Important,
    Critical,
}

public enum MemoStatus
{
    InProgress,
    Completed,
    Archived,
}

public sealed class Memo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public MemoCategory MemoCategory { get; set; }

    public MemoImportance Importance { get; set; }

    public MemoStatus Status { get; set; }

    public Guid? OwnerMemberId { get; set; }

    public Guid? RelatedSpaceId { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

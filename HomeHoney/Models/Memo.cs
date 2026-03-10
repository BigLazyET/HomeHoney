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

    public MemoCategory MemoCategory { get; set; } = MemoCategory.Other;

    public MemoImportance Importance { get; set; } = MemoImportance.Normal;

    public MemoStatus Status { get; set; } = MemoStatus.InProgress;

    public Guid? OwnerMemberId { get; set; }

    public Guid? RelatedSpaceId { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

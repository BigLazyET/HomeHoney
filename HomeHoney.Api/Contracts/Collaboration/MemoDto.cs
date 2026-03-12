using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Collaboration;

public sealed class MemoDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public MemoCategory MemoCategory { get; set; } = MemoCategory.Other;

    public MemoImportance Importance { get; set; } = MemoImportance.Normal;

    public MemoStatus Status { get; set; } = MemoStatus.InProgress;

    public Guid? OwnerMemberId { get; set; }

    public Guid? RelatedSpaceId { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

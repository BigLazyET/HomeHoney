using HomeHoney.Api.Contracts.Collaboration;
using HomeHoney.Models;

namespace HomeHoney.Api.Application.Collaboration;

public static class CollaborationDtoMapper
{
    public static FridgeNoteDto ToDto(FridgeNote note)
        => new()
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Category = note.Category,
            Priority = note.Priority,
            ColorStyle = note.ColorStyle,
            IsPinned = note.IsPinned,
            IsCompleted = note.IsCompleted,
            OwnerMemberId = note.OwnerMemberId,
            DueAt = note.DueAt,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
        };

    public static FridgeNote ToModel(FridgeNoteDto note)
        => new()
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Category = note.Category,
            Priority = note.Priority,
            ColorStyle = note.ColorStyle,
            IsPinned = note.IsPinned,
            IsCompleted = note.IsCompleted,
            OwnerMemberId = note.OwnerMemberId,
            DueAt = note.DueAt,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
        };

    public static MemoDto ToDto(Memo memo)
        => new()
        {
            Id = memo.Id,
            Title = memo.Title,
            Content = memo.Content,
            MemoCategory = memo.MemoCategory,
            Importance = memo.Importance,
            Status = memo.Status,
            OwnerMemberId = memo.OwnerMemberId,
            RelatedSpaceId = memo.RelatedSpaceId,
            DueAt = memo.DueAt,
            CreatedAt = memo.CreatedAt,
            UpdatedAt = memo.UpdatedAt,
        };

    public static Memo ToModel(MemoDto memo)
        => new()
        {
            Id = memo.Id,
            Title = memo.Title,
            Content = memo.Content,
            MemoCategory = memo.MemoCategory,
            Importance = memo.Importance,
            Status = memo.Status,
            OwnerMemberId = memo.OwnerMemberId,
            RelatedSpaceId = memo.RelatedSpaceId,
            DueAt = memo.DueAt,
            CreatedAt = memo.CreatedAt,
            UpdatedAt = memo.UpdatedAt,
        };
}

namespace HomeHoney.Models;

public enum HouseholdRole
{
    Self,
    Spouse,
    Child,
    Parent,
    Other,
}

public sealed class HouseholdMember
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string DisplayName { get; set; } = string.Empty;

    public HouseholdRole Role { get; set; } = HouseholdRole.Self;

    public string AvatarColor { get; set; } = "#007aff";

    public bool IsPrimary { get; set; }
}

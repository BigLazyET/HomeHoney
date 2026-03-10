namespace HomeHoney.Models;

public enum HomeModuleType
{
    RecentDocuments,
    UpcomingReminders,
    QuickActions,
    FamilyMessages,
    RecommendedActions,
}

public sealed class HomeModule
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public HomeModuleType ModuleType { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int SortOrder { get; set; }

    public int MaxItems { get; set; } = 3;
}

public sealed class QuickActionItem
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = "✨";

    public string Href { get; set; } = "/";
}

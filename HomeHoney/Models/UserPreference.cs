namespace HomeHoney.Models;

public enum ThemeMode
{
    Light,
    Dark,
    System,
}

public enum PrivacyMode
{
    Standard,
    MaskSensitive,
    RequireDetailVerification,
}

public sealed class NotificationPreference
{
    public bool InsuranceEnabled { get; set; } = true;

    public bool ManualEnabled { get; set; } = true;

    public bool MemoEnabled { get; set; } = true;

    public bool FridgeNoteEnabled { get; set; } = true;
}

public sealed class UserPreference
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    public NotificationPreference NotificationSettings { get; set; } = new();

    public List<HomeModuleType> HomeModuleOrder { get; set; } =
    [
        HomeModuleType.QuickActions,
        HomeModuleType.UpcomingReminders,
        HomeModuleType.RecentDocuments,
        HomeModuleType.FamilyMessages,
        HomeModuleType.RecommendedActions,
    ];

    public HashSet<HomeModuleType> HiddenHomeModules { get; set; } = [];

    public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.Standard;

    public bool OnboardingCompleted { get; set; }
}

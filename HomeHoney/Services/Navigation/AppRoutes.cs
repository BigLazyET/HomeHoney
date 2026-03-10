namespace HomeHoney.Services.Navigation;

public static class AppRoutes
{
    public const string Home = "/";
    public const string Welcome = "/welcome";
    public const string WelcomeFeatures = "/welcome/features";
    public const string WelcomeGetStarted = "/welcome/get-started";

    public const string Documents = "/documents";
    public const string InsuranceList = "/documents/insurance";
    public const string InsuranceNew = "/documents/insurance/new";
    public const string ManualsList = "/documents/manuals";
    public const string ManualNew = "/documents/manuals/new";

    public const string FridgeNotes = "/fridge-notes";
    public const string FridgeNoteNew = "/fridge-notes/new";
    public const string Memos = "/memos";
    public const string MemoNew = "/memos/new";

    public const string Reminders = "/reminders";
    public const string Search = "/search";

    public const string Settings = "/settings";
    public const string SettingsTheme = "/settings/theme";
    public const string SettingsNotifications = "/settings/notifications";
    public const string SettingsHomeLayout = "/settings/home-layout";
    public const string SettingsPrivacy = "/settings/privacy";
    public const string SettingsAbout = "/settings/about";

    public static string InsuranceDetail(Guid id) => $"/documents/insurance/{id}";

    public static string InsuranceEdit(Guid id) => $"/documents/insurance/{id}/edit";

    public static string ManualDetail(Guid id) => $"/documents/manuals/{id}";

    public static string ManualEdit(Guid id) => $"/documents/manuals/{id}/edit";

    public static string MemoDetail(Guid id) => $"/memos/{id}";

    public static string MemoEdit(Guid id) => $"/memos/{id}/edit";

    public static string FridgeNoteEdit(Guid id) => $"/fridge-notes/{id}/edit";
}

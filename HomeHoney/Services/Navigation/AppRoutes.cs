namespace HomeHoney.Services.Navigation;

public static class AppRoutes
{
    public const string Home = "/";
    public const string Welcome = "/welcome";
    public const string WelcomePreview = "/welcome?mode=preview";
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

    public static bool IsWelcomeRoute(string? route) => NormalizeRoute(route).StartsWith(Welcome, StringComparison.OrdinalIgnoreCase);

    public static string NormalizeRoute(string? route)
    {
        var normalized = string.IsNullOrWhiteSpace(route) ? Home : route.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        var trimIndex = normalized.IndexOfAny(['?', '#']);
        if (trimIndex >= 0)
        {
            normalized = normalized[..trimIndex];
        }

        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }

    public static string GetParentRoute(string? route)
    {
        var normalized = NormalizeRoute(route);

        if (string.Equals(normalized, WelcomeFeatures, StringComparison.OrdinalIgnoreCase))
        {
            return Welcome;
        }

        if (string.Equals(normalized, WelcomeGetStarted, StringComparison.OrdinalIgnoreCase))
        {
            return WelcomeFeatures;
        }

        if (string.Equals(normalized, InsuranceList, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, ManualsList, StringComparison.OrdinalIgnoreCase))
        {
            return Documents;
        }

        if (string.Equals(normalized, SettingsTheme, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, SettingsNotifications, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, SettingsHomeLayout, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, SettingsPrivacy, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(normalized, SettingsAbout, StringComparison.OrdinalIgnoreCase))
        {
            return Settings;
        }

        if (normalized.StartsWith($"{InsuranceList}/", StringComparison.OrdinalIgnoreCase))
        {
            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 4 && string.Equals(segments[^1], "edit", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(segments[^2], out var id))
            {
                return InsuranceDetail(id);
            }

            return InsuranceList;
        }

        if (normalized.StartsWith($"{ManualsList}/", StringComparison.OrdinalIgnoreCase))
        {
            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 4 && string.Equals(segments[^1], "edit", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(segments[^2], out var id))
            {
                return ManualDetail(id);
            }

            return ManualsList;
        }

        if (normalized.StartsWith($"{FridgeNotes}/", StringComparison.OrdinalIgnoreCase))
        {
            return FridgeNotes;
        }

        return Home;
    }
}

using HomeHoney.Services.Navigation;

namespace HomeHoney.Services.Navigation;

public sealed record NavigationItem(string Title, string Icon, string Href, bool MatchAll = false, string? Description = null);

public sealed class NavigationStructureService
{
    public IReadOnlyList<NavigationItem> GetPrimaryTabs() =>
    [
        new("首页", "🏠", AppRoutes.Home, true, "摘要与快捷入口"),
        new("资料", "🗂️", AppRoutes.Documents, false, "保险与说明书"),
        new("冰箱贴", "🧲", AppRoutes.FridgeNotes, false, "家庭留言与待办"),
        new("提醒", "⏰", AppRoutes.Reminders, false, "待处理事项与搜索"),
        new("设置", "⚙️", AppRoutes.Settings, false, "主题与个性化"),
    ];

    public IReadOnlyList<NavigationItem> GetDocumentSections() =>
    [
        new("保险", "🛡️", AppRoutes.InsuranceList, false, "保障、到期与联系人"),
        new("说明书", "📘", AppRoutes.ManualsList, false, "设备、空间与保修"),
    ];

    public IReadOnlyList<NavigationItem> GetSettingSections() =>
    [
        new("外观", "🎨", AppRoutes.SettingsTheme),
        new("提醒", "🔔", AppRoutes.SettingsNotifications),
        new("首页布局", "🧩", AppRoutes.SettingsHomeLayout),
        new("隐私与安全", "🔐", AppRoutes.SettingsPrivacy),
    ];
}

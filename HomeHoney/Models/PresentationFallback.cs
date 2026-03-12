namespace HomeHoney.Models;

public enum FallbackKind
{
    MissingValue,
    UnknownValue,
    NotUploaded,
    NotSynced,
    NotApplicable,
}

public sealed record PresentationFallback(
    string FieldKey,
    FallbackKind FallbackKind,
    string DisplayText,
    string? Tone = null,
    bool IsUserActionSuggested = false);

public static class PresentationFallbacks
{
    public static PresentationFallback Missing(string fieldKey, string displayText = "未填写")
        => new(fieldKey, FallbackKind.MissingValue, displayText, Tone: "warning");

    public static PresentationFallback Unknown(string fieldKey, string displayText = "未知")
        => new(fieldKey, FallbackKind.UnknownValue, displayText, Tone: "warning");

    public static PresentationFallback NotUploaded(string fieldKey, string displayText = "暂无附件")
        => new(fieldKey, FallbackKind.NotUploaded, displayText, Tone: "warning", IsUserActionSuggested: true);

    public static PresentationFallback NotSynced(string fieldKey, string displayText = "尚未同步")
        => new(fieldKey, FallbackKind.NotSynced, displayText, Tone: "warning", IsUserActionSuggested: true);

    public static PresentationFallback NotApplicable(string fieldKey, string displayText = "不适用")
        => new(fieldKey, FallbackKind.NotApplicable, displayText);

    public static string TextOrFallback(string? value, string fallback = "未填写")
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    public static string TextOrUnknown(string? value)
        => TextOrFallback(value, "未知");

    public static string DateTimeOrFallback(DateTime? value, string fallback = "未设置")
        => value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm") : fallback;

    public static string DateOnlyOrFallback(DateOnly value, string fallback = "未设置")
        => value == default ? fallback : value.ToString("yyyy-MM-dd");

    public static string GuidOrFallback(Guid value, string fallback = "未选择")
        => value == Guid.Empty ? fallback : value.ToString();

    public static string TagsOrFallback(IReadOnlyList<string>? tags, string fallback = "暂无标签")
        => tags is { Count: > 0 } ? string.Join(" · ", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim())) : fallback;

    public static string FileSizeOrFallback(long? sizeBytes, string fallback = "未知大小")
    {
        if (!sizeBytes.HasValue || sizeBytes.Value <= 0)
        {
            return fallback;
        }

        var size = sizeBytes.Value;
        string[] units = ["B", "KB", "MB", "GB"];
        var unitIndex = 0;
        double normalized = size;
        while (normalized >= 1024 && unitIndex < units.Length - 1)
        {
            normalized /= 1024;
            unitIndex++;
        }

        return $"{normalized:0.#} {units[unitIndex]}";
    }
}

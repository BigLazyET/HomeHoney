using System.Collections.Concurrent;

namespace HomeHoney.Services.Preferences;

public static class PlatformPreferenceStore
{
    private static readonly ConcurrentDictionary<string, string> FallbackStore = new(StringComparer.Ordinal);

    public static string? GetString(string key)
    {
#if ANDROID || IOS || MACCATALYST || WINDOWS
        return global::Microsoft.Maui.Storage.Preferences.Default.ContainsKey(key)
            ? global::Microsoft.Maui.Storage.Preferences.Default.Get(key, string.Empty)
            : null;
#else
        return FallbackStore.TryGetValue(key, out var value) ? value : null;
#endif
    }

    public static void SetString(string key, string value)
    {
#if ANDROID || IOS || MACCATALYST || WINDOWS
        global::Microsoft.Maui.Storage.Preferences.Default.Set(key, value);
#else
        FallbackStore[key] = value;
#endif
    }

    public static void Remove(string key)
    {
#if ANDROID || IOS || MACCATALYST || WINDOWS
        global::Microsoft.Maui.Storage.Preferences.Default.Remove(key);
#else
        FallbackStore.TryRemove(key, out _);
#endif
    }
}
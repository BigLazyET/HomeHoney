using System.Diagnostics;
using System.Text;

namespace HomeHoney.Services.Diagnostics;

public static class RuntimeDiagnostics
{
    public static string LogTargetDescription => "IDE 调试输出 / Debug Console";

    public static void Record(string source, Exception exception)
        => Record(source, exception.ToString());

    public static void Record(string source, string message)
    {
        var entry = new StringBuilder()
            .Append('[').Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")).Append("] ")
            .Append(source)
            .AppendLine()
            .AppendLine(message)
            .AppendLine(new string('-', 80))
            .ToString();

        Debug.WriteLine(entry);
    }
}
namespace HomeHoney.Services;

/// <summary>
/// 问候服务接口，返回应用名称和欢迎信息。
/// </summary>
public interface IGreetingService
{
    /// <summary>
    /// 获取应用名称。
    /// </summary>
    string GetAppName();

    /// <summary>
    /// 获取欢迎信息。
    /// </summary>
    string GetWelcomeMessage();
}

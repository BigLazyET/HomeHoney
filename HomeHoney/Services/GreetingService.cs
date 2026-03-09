namespace HomeHoney.Services;

/// <summary>
/// 问候服务的默认实现。
/// </summary>
public class GreetingService : IGreetingService
{
    public string GetAppName() => "HomeHoney";

    public string GetWelcomeMessage() => "家庭说明书和保险合同库";
}

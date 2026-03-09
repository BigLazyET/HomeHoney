using HomeHoney.Services;

namespace HomeHoney.Tests.Unit;

public class GreetingServiceTests
{
    private readonly IGreetingService _sut;

    public GreetingServiceTests()
    {
        _sut = new GreetingService();
    }

    [Fact]
    public void GetAppName_ReturnsHomeHoney()
    {
        // Act
        var result = _sut.GetAppName();

        // Assert
        Assert.Equal("HomeHoney", result);
    }

    [Fact]
    public void GetWelcomeMessage_ReturnsExpectedChinese()
    {
        // Act
        var result = _sut.GetWelcomeMessage();

        // Assert
        Assert.Equal("家庭说明书和保险合同库", result);
    }

    [Fact]
    public void GetAppName_IsNotNullOrEmpty()
    {
        // Act
        var result = _sut.GetAppName();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}

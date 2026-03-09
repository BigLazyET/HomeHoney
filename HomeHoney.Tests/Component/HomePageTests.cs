using Bunit;
using HomeHoney.Components.Pages;

namespace HomeHoney.Tests.Component;

public class HomePageTests : BunitContext
{
    [Fact]
    public void Home_RendersAppTitle()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.Find("h1").MarkupMatches("<h1>HomeHoney</h1>");
    }

    [Fact]
    public void Home_RendersDescription()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        var paragraph = cut.Find("p");
        Assert.Contains("家庭说明书和保险合同库", paragraph.TextContent);
    }
}

using Bunit;
using HomeHoney.Components.Pages;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Preferences;
using HomeHoney.Services.Reminders;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHoney.Tests.Component;

public class HomePageTests : BunitContext
{
    public HomePageTests()
    {
        Services.AddSingleton<UserPreferenceService>();
        Services.AddSingleton<DocumentCatalogService>();
        Services.AddSingleton<FamilyCollaborationService>();
        Services.AddSingleton<ReminderCenterService>();
    }

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
        Assert.Contains("家庭说明书、保险、冰箱贴和备忘录", cut.Markup);
    }

    [Fact]
    public void Home_RendersQuickActions()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        Assert.Contains("新增保险", cut.Markup);
        Assert.Contains("写冰箱贴", cut.Markup);
    }
}

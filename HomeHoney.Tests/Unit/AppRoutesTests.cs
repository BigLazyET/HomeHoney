using HomeHoney.Services.Navigation;

namespace HomeHoney.Tests.Unit;

public class AppRoutesTests
{
    [Theory]
    [InlineData(AppRoutes.InsuranceList, AppRoutes.Documents)]
    [InlineData(AppRoutes.ManualsList, AppRoutes.Documents)]
    [InlineData(AppRoutes.SettingsTheme, AppRoutes.Settings)]
    [InlineData(AppRoutes.SettingsPrivacy, AppRoutes.Settings)]
    [InlineData(AppRoutes.WelcomeFeatures, AppRoutes.Welcome)]
    [InlineData(AppRoutes.WelcomeGetStarted, AppRoutes.WelcomeFeatures)]
    public void GetParentRoute_ReturnsExpectedParent_ForKnownRoutes(string route, string expected)
    {
        // Act
        var result = AppRoutes.GetParentRoute(route);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetParentRoute_ReturnsDetailRoute_ForInsuranceEditPage()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = AppRoutes.GetParentRoute(AppRoutes.InsuranceEdit(id));

        // Assert
        Assert.Equal(AppRoutes.InsuranceDetail(id), result);
    }

    [Fact]
    public void NormalizeRoute_RemovesQueryStringAndTrailingSlash()
    {
        // Act
        var result = AppRoutes.NormalizeRoute("/welcome/?mode=preview");

        // Assert
        Assert.Equal(AppRoutes.Welcome, result);
    }
}

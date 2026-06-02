using BlazorApp.Web.Theme;

namespace BlazorApp.UnitTests;

public sealed class AppThemeTests
{
    [Fact]
    public void Create_UsesCarbonThemeDefaults()
    {
        var theme = AppTheme.Create();

        Assert.Equal("#2DDE98", theme.PaletteDark?.Primary);
        Assert.Equal("60px", theme.LayoutProperties.DrawerWidthLeft);
    }
}

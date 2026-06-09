using MudBlazor;

namespace BlazorApp.Web.Theme;

public static class AppTheme
{
    public static MudTheme Create() => new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#2DDE98",
            PrimaryDarken = "#25C882",
            PrimaryLighten = "rgba(45,222,152,.14)",
            Secondary = "#F0B429",
            Success = "#2DDE98",
            Warning = "#F0B429",
            Error = "#F85149",
            Info = "#58A6FF",
            Background = "#0D1117",
            BackgroundGrey = "#080D13",
            Surface = "#161B22",
            DrawerBackground = "#080D13",
            DrawerText = "#8B949E",
            DrawerIcon = "#484F58",
            AppbarBackground = "rgba(13,17,23,.85)",
            AppbarText = "#E6EDF3",
            TextPrimary = "#E6EDF3",
            TextSecondary = "#8B949E",
            TextDisabled = "#484F58",
            ActionDefault = "#8B949E",
            Divider = "#21262D",
            DividerLight = "#30363D",
            TableLines = "rgba(255,255,255,.04)",
            TableHover = "rgba(255,255,255,.025)"
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "60px",
            DefaultBorderRadius = "8px"
        }
    };
}

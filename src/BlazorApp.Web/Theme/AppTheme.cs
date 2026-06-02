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
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Figtree", "sans-serif"],
                FontSize = "13px",
                FontWeight = "400",
                LineHeight = "1.5"
            },
            H1 = new H1Typography { FontFamily = ["Syne", "sans-serif"], FontSize = "24px", FontWeight = "800" },
            H2 = new H2Typography { FontFamily = ["Syne", "sans-serif"], FontSize = "20px", FontWeight = "700" },
            H3 = new H3Typography { FontSize = "16px", FontWeight = "700" },
            H4 = new H4Typography { FontSize = "14px", FontWeight = "700" },
            H5 = new H5Typography { FontSize = "12px", FontWeight = "700" },
            H6 = new H6Typography { FontSize = "9.5px", FontWeight = "700", LetterSpacing = "0.8px", LineHeight = "1.4", TextTransform = "uppercase" },
            Button = new ButtonTypography { FontFamily = ["Figtree", "sans-serif"], FontSize = "12px", FontWeight = "600", TextTransform = "none", LetterSpacing = "0" }
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "60px",
            DefaultBorderRadius = "8px"
        }
    };
}

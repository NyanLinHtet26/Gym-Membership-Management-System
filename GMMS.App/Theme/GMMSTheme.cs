using MudBlazor;

namespace GMMS.App.Theme
{
    public static class GMMSTheme
    {
        public static readonly MudTheme Theme = new()
        {
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px"
            },
            PaletteLight = new PaletteLight
            {
                Primary = "#7C3AED",
                Secondary = "#6366F1",
                Background = "#F8FAFC",
                Surface = "#FFFFFF",
                AppbarBackground = "#7C3AED",
                AppbarText = "#FFFFFF",
                TextPrimary = "#0F172A",
                TextSecondary = "#64748B",
                Divider = "#E2E8F0",
                TableLines = "#E2E8F0"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#7C3AED",
                Secondary = "#6366F1",
                Background = "#111111",
                Surface = "#1A1A1A",
                BackgroundGray = "rgba(255,255,255,0.07)",
                AppbarBackground = "#111111",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#111111",
                DrawerText = "#FFFFFF",
                DrawerIcon = "#FFFFFF",
                TextPrimary = "#F5F5F5",
                TextSecondary = "rgba(255,255,255,0.7)",
                ActionDefault = "rgba(255,255,255,0.7)",
                ActionDisabled = "rgba(255,255,255,0.3)",
                LinesDefault = "rgba(255,255,255,0.08)",
                LinesInputs = "rgba(255,255,255,0.12)",
                Divider = "rgba(255,255,255,0.12)",
                TableLines = "rgba(255,255,255,0.08)",
                TableHover = "rgba(255,255,255,0.04)",
                TableStriped = "rgba(255,255,255,0.03)",
                Dark = "#1A1A1A",
                DarkDarken = "#111111",
                DarkLighten = "#2A2A2A"
            }
        };
    }
}

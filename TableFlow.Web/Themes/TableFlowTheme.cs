using MudBlazor;

namespace TableFlow.Web.Themes
{
    public static class TableFlowTheme
    {
        public static MudTheme Theme => new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0D6B63",
                PrimaryContrastText = "#FFFFFF",
                PrimaryDarken = "#0A5550",
                PrimaryLighten = "#E6F7F5",

                Secondary = "#F59E0B",
                SecondaryContrastText = "#FFFFFF",
                SecondaryDarken = "#D97706",
                SecondaryLighten = "#FEF3C7",

                Success = "#22C55E",
                SuccessContrastText = "#FFFFFF",
                Warning = "#F59E0B",
                WarningContrastText = "#FFFFFF",
                Error = "#EF4444",
                ErrorContrastText = "#FFFFFF",
                Info = "#3B82F6",
                InfoContrastText = "#FFFFFF",

                Background = "#F8FAFC",
                Surface = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#1E293B",

                TextPrimary = "#1E293B",
                TextSecondary = "#64748B",
                TextDisabled = "#94A3B8",

                Divider = "#E2E8F0",
                TableLines = "#E2E8F0",
                LinesDefault = "#E2E8F0",
                LinesInputs = "#CBD5E1",

                ActionDefault = "#64748B",
                ActionDisabled = "#CBD5E1",
                OverlayDark = "rgba(15,23,42,0.5)",
            },

            PaletteDark = new PaletteDark
            {
                Primary = "#0F8A80",
                PrimaryContrastText = "#FFFFFF",
                PrimaryDarken = "#0D6B63",
                PrimaryLighten = "#134E4A",

                Secondary = "#F59E0B",
                SecondaryContrastText = "#1E293B",

                Success = "#22C55E",
                Warning = "#F59E0B",
                Error = "#EF4444",
                Info = "#3B82F6",

                Background = "#0F172A",
                Surface = "#1E293B",
                DrawerBackground = "#1E293B",
                AppbarBackground = "#1E293B",
                AppbarText = "#F1F5F9",

                TextPrimary = "#F1F5F9",
                TextSecondary = "#94A3B8",
                TextDisabled = "#475569",

                Divider = "#334155",
                TableLines = "#334155",
                LinesDefault = "#334155",
                LinesInputs = "#475569",
            },

            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "15px",
                    FontWeight = "400",
                    LineHeight = "1.6",
                    LetterSpacing = "0"
                },
                H1 = new H1Typography
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" },
                    FontSize = "28px",
                    FontWeight = "700",
                    LineHeight = "1.2",
                    LetterSpacing = "-0.01em"
                },
                H2 = new H2Typography
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" },
                    FontSize = "22px",
                    FontWeight = "500",
                    LineHeight = "1.3",
                    LetterSpacing = "-0.01em"
                },
                H3 = new H3Typography
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" },
                    FontSize = "17px",
                    FontWeight = "500",
                    LineHeight = "1.4"
                },
                H4 = new H4Typography
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" },
                    FontSize = "15px",
                    FontWeight = "500",
                    LineHeight = "1.4"
                },
                H5 = new H5Typography
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" },
                    FontSize = "13px",
                    FontWeight = "500",
                    LineHeight = "1.4"
                },
                H6 = new H6Typography
                {
                    FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" },
                    FontSize = "12px",
                    FontWeight = "500",
                    LineHeight = "1.4"
                },
                Subtitle1 = new Subtitle1Typography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "15px",
                    FontWeight = "500"
                },
                Subtitle2 = new Subtitle2Typography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "13px",
                    FontWeight = "500"
                },
                Body1 = new Body1Typography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "15px",
                    FontWeight = "400"
                },
                Body2 = new Body2Typography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "13px",
                    FontWeight = "400"
                },
                Button = new ButtonTypography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "14px",
                    FontWeight = "500",
                    TextTransform = "none"
                },
                Caption = new CaptionTypography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "12px",
                    FontWeight = "400"
                },
                Overline = new OverlineTypography
                {
                    FontFamily = new[] { "DM Sans", "sans-serif" },
                    FontSize = "11px",
                    FontWeight = "500",
                    LetterSpacing = "0.08em"
                }
            },

            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px",
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "260px",
                AppbarHeight = "64px"
            }
        };
    }
}

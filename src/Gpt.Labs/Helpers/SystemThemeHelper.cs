using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Gpt.Labs.Helpers
{
    public static class SystemThemeHelper
	{
        public static void ApplyTheme(FrameworkElement root)
        {
            if (root != null)
            {
                switch (ApplicationSettings.Instance.AppTheme)
                {
                    case ElementTheme.Default:
                        root.RequestedTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                        break;
                    default:
                        root.RequestedTheme = ApplicationSettings.Instance.AppTheme;
                        break;
                }

				if (AppWindowTitleBar.IsCustomizationSupported())
				{
                    var titleBar = App.Window.GetAppWindow().TitleBar;

                    switch (root.RequestedTheme)
                    {
                        case ElementTheme.Dark:
                            titleBar.ButtonBackgroundColor = "#FF202020".ToColor();
                            titleBar.ButtonInactiveBackgroundColor = "#FF202020".ToColor();
                            titleBar.ButtonHoverBackgroundColor = "#FF2D2D2D".ToColor();
                            titleBar.ButtonPressedBackgroundColor = "#FF292929".ToColor();
                            titleBar.ButtonForegroundColor = "#FFFFFFFF".ToColor();
                            titleBar.ButtonHoverForegroundColor = "#FFFFFFFF".ToColor();
                            titleBar.ButtonPressedForegroundColor = "#FFA7A7A7".ToColor();
                            titleBar.ButtonInactiveForegroundColor = "#5DFFFFFF".ToColor();
                            break;
                        case ElementTheme.Light:
                            titleBar.ButtonBackgroundColor = "#FFF3F3F3".ToColor();
                            titleBar.ButtonInactiveBackgroundColor = "#FFF3F3F3".ToColor();
                            titleBar.ButtonHoverBackgroundColor = "#FFE9E9E9".ToColor();
                            titleBar.ButtonPressedBackgroundColor = "#FFEDEDED".ToColor();
                            titleBar.ButtonForegroundColor = "#E4000000".ToColor();
                            titleBar.ButtonHoverForegroundColor = "#E4000000".ToColor();
                            titleBar.ButtonPressedForegroundColor = "#FF5F5F5F".ToColor();
                            titleBar.ButtonInactiveForegroundColor = "#5C000000".ToColor();
                            break;
                    }
                }
            }
        }
    }
}

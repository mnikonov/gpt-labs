using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Gpt.Labs.Helpers
{
    public static class SystemThemeHelper
	{
        public static void ApplyTheme(this Window window)
        {
            ElementTheme theme;

            switch (ApplicationSettings.Instance.AppTheme)
            {
                case ElementTheme.Default:
                    theme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                    break;
                default:
                    theme = ApplicationSettings.Instance.AppTheme;
                    break;
            }

            var content = window.Content as FrameworkElement;

            if (content != null)
            {
                content.RequestedTheme = theme;
            }

            if (AppWindowTitleBar.IsCustomizationSupported())
			{                    
                var titleBar = window.GetAppWindow().TitleBar;

                switch (theme)
                {
                    case ElementTheme.Dark:
                        titleBar.ButtonBackgroundColor = "#00FFFFFF".ToColor();
                        titleBar.ButtonInactiveBackgroundColor = "#00FFFFFF".ToColor();
                        titleBar.ButtonHoverBackgroundColor = "#FF2D2D2D".ToColor();
                        titleBar.ButtonPressedBackgroundColor = "#FF292929".ToColor();
                        titleBar.ButtonForegroundColor = "#FFFFFFFF".ToColor();
                        titleBar.ButtonHoverForegroundColor = "#FFFFFFFF".ToColor();
                        titleBar.ButtonPressedForegroundColor = "#FFA7A7A7".ToColor();
                        titleBar.ButtonInactiveForegroundColor = "#5DFFFFFF".ToColor();
                        break;
                    case ElementTheme.Light:
                        titleBar.ButtonBackgroundColor = "#00FFFFFF".ToColor();
                        titleBar.ButtonInactiveBackgroundColor = "#00FFFFFF".ToColor();
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

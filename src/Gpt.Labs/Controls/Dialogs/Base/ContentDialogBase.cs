namespace Gpt.Labs.Controls.Dialogs.Base
{
    using Gpt.Labs;
    using Gpt.Labs.ViewModels;

    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public class ContentDialogBase : ContentDialog
    {
        public ContentDialogBase()
        {
            switch (ApplicationSettings.Instance.AppTheme)
            {
                case ElementTheme.Default:
                    this.RequestedTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                    break;
                default:
                    this.RequestedTheme = ApplicationSettings.Instance.AppTheme;
                    break;
            }

            this.XamlRoot = App.Window.Content.XamlRoot;
        }
    }
}

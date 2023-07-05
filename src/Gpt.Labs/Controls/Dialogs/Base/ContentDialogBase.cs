namespace Gpt.Labs.Controls.Dialogs.Base
{
    using Gpt.Labs.ViewModels;

    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public class ContentDialogBase : ContentDialog
    {
        #region Fields

        protected readonly Window window;

        #endregion

        #region Constructors

        public ContentDialogBase(Window window)
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

            this.window = window;

            this.XamlRoot = window.Content.XamlRoot;

            this.Style = (Style)App.Current.Resources["DefaultContentDialogStyle"];
        }

        #endregion
    }
}

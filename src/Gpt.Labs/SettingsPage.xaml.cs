using Gpt.Labs.Helpers;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using Gpt.Labs.Controls.Dialogs;

namespace Gpt.Labs
{
    public sealed partial class SettingsPage : BasePage
    {
        #region Constructors

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public ApplicationSettings SettingsViewModel { get; } = ApplicationSettings.Instance;

        #endregion

        #region Private Methods

        private void OnApplyThemeClick(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.Tag != null)
            {
                this.SettingsViewModel.AppTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), radio.Tag.ToString());
                SystemThemeHelper.ApplyTheme(App.Window.Content as FrameworkElement);
            }
        }

        private async void OnEditOpenAISettingsClick(object sender, RoutedEventArgs e)
        {
            var dialog = new EditOpenAISettingsDialog();
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.SettingsViewModel.OpenAIOrganization = dialog.ViewModel.Organization;
                this.SettingsViewModel.OpenAIApiKey = dialog.ViewModel.ApiKey;
            }
        }

        #endregion
    }
}

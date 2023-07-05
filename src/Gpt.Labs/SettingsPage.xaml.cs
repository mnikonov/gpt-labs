using Gpt.Labs.Helpers;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using Gpt.Labs.Controls.Dialogs;
using Gpt.Labs.Helpers.Extensions;
using Windows.ApplicationModel;
using Windows.System;
using Windows.Services.Store;
using WinRT.Interop;

namespace Gpt.Labs
{
    public sealed partial class SettingsPage : StatePage
    {
        #region Constructors

        public SettingsPage()
        {            
            this.InitializeComponent();

            ApplicationVersion.Text = Package.Current.Id.Version.GetStringVersion(); 
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
                
                foreach (var window in WindowManager.Enumerate())
                {
                    window.ApplyTheme();
                }
            }
        }

        private async void OnEditOpenAISettingsClick(object sender, RoutedEventArgs e)
        {
            var dialog = new EditOpenAISettingsDialog(this.Window);
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.SettingsViewModel.OpenAIOrganization = dialog.ViewModel.Organization;
                this.SettingsViewModel.OpenAIApiKey = dialog.ViewModel.ApiKey;
            }
        }
        
        private async void OnQuestionButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/mnikonov/gpt-labs/issues/new?assignees=mnikonov&labels=question&projects=&template=question.md&title=%5BQUESTION%5D+-+")); 
        }

        private async void OnFeatureButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/mnikonov/gpt-labs/issues/new?assignees=mnikonov&labels=enhancement&projects=&template=feature_request.md&title=%5BFEATURE%5D+-+")); 
        }

        private async void OnBugButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/mnikonov/gpt-labs/issues/new?assignees=mnikonov&labels=bug&projects=&template=bug_report.md&title=%5BBUG%5D+-+")); 
        }

        private async void OnSponsorButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/sponsors/mnikonov")); 
        }

        private async void OnRatingButtonClick(object sender, RoutedEventArgs e)
        {
            var context = StoreContext.GetDefault();
            var hwnd = WindowNative.GetWindowHandle(this.Window);
            InitializeWithWindow.Initialize(context, hwnd);
            
            await context.RequestRateAndReviewAppAsync();
        }

        #endregion
    }
}

using Gpt.Labs.Controls.Dialogs;
using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Base;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;
using Windows.Services.Store;
using Windows.System;
using WinRT.Interop;

namespace Gpt.Labs
{
    public sealed partial class SettingsPage : StatePage
    {
        #region Constructors

        public SettingsPage()
        {
            SettingsViewModel = new NotifyTaskCompletion<ApplicationSettings>
            {
                Function = async token =>
            {
                var viewModel = ApplicationSettings.Instance;

                await viewModel.InitExtraSettings();

                return viewModel;
            }
            };

            SettingsViewModel.Start();

            InitializeComponent();

            ApplicationVersion.Text = Package.Current.Id.Version.GetStringVersion();
        }

        #endregion

        #region Properties

        public NotifyTaskCompletion<ApplicationSettings> SettingsViewModel { get; }

        #endregion

        #region Private Methods

        private void OnApplyThemeClick(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.Tag != null)
            {
                SettingsViewModel.Result.AppTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), radio.Tag.ToString());

                foreach (var window in WindowManager.Enumerate())
                {
                    window.ApplyTheme();
                }
            }
        }

        private async void OnEditOpenAISettingsClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                var dialog = new EditOpenAISettingsDialog(Window);
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    SettingsViewModel.Result.OpenAIOrganization = dialog.ViewModel.Organization;
                    SettingsViewModel.Result.OpenAIApiKey = dialog.ViewModel.ApiKey;
                }
            });
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
            var hwnd = WindowNative.GetWindowHandle(Window);
            InitializeWithWindow.Initialize(context, hwnd);

            await context.RequestRateAndReviewAppAsync();
        }

        private async void LaunchOnStartup_Toggled(object sender, RoutedEventArgs e)
        {
            var enable = ((ToggleSwitch)sender).IsOn;

            if (enable == SettingsViewModel.Result.LaunchOnStartup)
            {
                return;
            }

            var startup = await SettingsViewModel.Result.GetStartupTask();

            switch (startup.State)
            {
                case StartupTaskState.Enabled when !enable:
                    startup.Disable();
                    break;
                case StartupTaskState.Disabled when enable:
                    await startup.RequestEnableAsync();
                    break;
                case StartupTaskState.DisabledByUser when enable:
                    await Window.CreateOkDialog("Error", "UnableToChangeStateOfStartupTask2").ShowAsync();
                    break;
                default:
                    await Window.CreateOkDialog("Error", "UnableToChangeStateOfStartupTask1").ShowAsync();
                    break;
            }

            SettingsViewModel.Result.LaunchOnStartup = startup.State == StartupTaskState.Enabled;
        }

        #endregion
    }
}

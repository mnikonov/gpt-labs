using Gpt.Labs.Controls.Dialogs;
using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Gpt.Labs.Controls
{
    public sealed partial class OpenAiStopsSettingsControl : BaseUserControl
    {
        #region Fields

        public static readonly DependencyProperty ChatSettingsProperty = DependencyProperty.Register(
            nameof(ChatSettings),
            typeof(OpenAIChatSettings),
            typeof(OpenAiStopsSettingsControl),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public OpenAiStopsSettingsControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIChatSettings ChatSettings
        {
            get => (OpenAIChatSettings)this.GetValue(ChatSettingsProperty);
            set => this.SetValue(ChatSettingsProperty, value);
        }

        #endregion

        #region Private Methods

        private async void OnAddStopClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await this.AddEditStop(null);
            });
        }

        private async void OnEditStopClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await this.AddEditStop((OpenAIStop)((FrameworkElement)sender).DataContext);
            });
        }

        private async void OnDeleteStopClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await this.DeleteStop((OpenAIStop)((FrameworkElement)sender).DataContext);
            });
        }

        private async Task AddEditStop(OpenAIStop token)
        {
            var dialog = new EditStopDialog(this.Window, new OpenAIStop() { Token = token?.Token });
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (this.ChatSettings.Stop == null)
                {
                    this.ChatSettings.Stop = new ObservableCollection<OpenAIStop>();
                }

                if (token != null && this.ChatSettings.Stop.Contains(token))
                {
                    token.Token = dialog.ViewModel.Token;
                }
                else if (this.ChatSettings.Stop.All(p => p.Token != dialog.ViewModel.Token))
                {
                    this.ChatSettings.Stop.Add(dialog.ViewModel);
                }
            }
        }

        private async Task DeleteStop(OpenAIStop token)
        {
            var dialog = this.Window.CreateYesNoDialog("Confirm", "DeleteStopToken", token.Token);
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.ChatSettings.Stop.Remove(token);
            }
        }

        #endregion
    }
}

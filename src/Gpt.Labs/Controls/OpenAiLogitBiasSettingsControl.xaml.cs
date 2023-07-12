using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Gpt.Labs.Models;
using Gpt.Labs.Controls.Dialogs;
using Gpt.Labs.Helpers.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Gpt.Labs.Controls.Extensions;

namespace Gpt.Labs.Controls
{
    public sealed partial class OpenAiLogitBiasSettingsControl : BaseUserControl
    {
        #region Fields

        public static readonly DependencyProperty ChatSettingsProperty = DependencyProperty.Register(
            nameof(ChatSettings),
            typeof(OpenAIChatSettings),
            typeof(OpenAiLogitBiasSettingsControl),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public OpenAiLogitBiasSettingsControl()
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

        private async void OnAddLogitBiasClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await this.AddEditLogitBias(null);
            });
        }

        private async void OnEditLogitBiasClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await this.AddEditLogitBias((OpenAILogitBias)((FrameworkElement)sender).DataContext);
            });
        }

        private async void OnDeleteLogitBiasClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await this.DeleteLogitBias((OpenAILogitBias)((FrameworkElement)sender).DataContext);
            });
        }

        private async Task AddEditLogitBias(OpenAILogitBias token)
        {
            var dialog = new EditLogitBiasDialog(this.Window, new OpenAILogitBias() { Token = token?.Token, Bias = token?.Bias ?? 0 });
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (this.ChatSettings.LogitBias == null)
                {
                    this.ChatSettings.LogitBias = new ObservableCollection<OpenAILogitBias>();
                }

                var existToken = this.ChatSettings.LogitBias.FirstOrDefault(p => p.Token == dialog.ViewModel.Token);

                if (existToken != null && token != null && existToken != token)
                {
                    this.ChatSettings.LogitBias.Remove(existToken);
                }


                if (token != null && this.ChatSettings.LogitBias.Contains(token))
                {
                    token.Token = dialog.ViewModel.Token;
                    token.Bias = dialog.ViewModel.Bias;
                }
                else if (token == null && existToken != null)
                {
                    existToken.Bias = dialog.ViewModel.Bias;
                }
                else
                {
                    this.ChatSettings.LogitBias.Add(dialog.ViewModel);
                }
            }
        }

        private async Task DeleteLogitBias(OpenAILogitBias token)
        {
            var dialog = this.Window.CreateYesNoDialog("Confirm", "DeleteLogitBias", token.Token);
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.ChatSettings.LogitBias.Remove(token);
            }
        }

        #endregion
    }
}

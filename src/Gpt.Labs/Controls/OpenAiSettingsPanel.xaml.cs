using Gpt.Labs.Helpers;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls
{
    public sealed partial class OpenAiSettingsPanel : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MessagesListViewModel),
            typeof(OpenAiSettingsPanel),
            new PropertyMetadata(null, null));

        public static readonly DependencyProperty ChatSettingsProperty = DependencyProperty.Register(
            nameof(ChatSettings),
            typeof(OpenAISettings),
            typeof(OpenAiSettingsPanel),
            new PropertyMetadata(null, null));

        #endregion

        #region Constructors

        public OpenAiSettingsPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public MessagesListViewModel ViewModel
        {
            get => (MessagesListViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        public OpenAISettings ChatSettings
        {
            get => (OpenAISettings)this.GetValue(ChatSettingsProperty);
            set => this.SetValue(ChatSettingsProperty, value);
        }

        #endregion

        #region Private Methods

        private async void OnApplyClick(object sender, RoutedEventArgs e)
        {
            this.ChatSettings.Validate();

            if (!this.ChatSettings.HasErrors)
            {
                await this.ViewModel.SaveSettings();

                var chatsViewModel = this.GetParent<ChatsPage>().ViewModel.Result;

                if (chatsViewModel.SelectedElement.Id == ChatSettings.ChatId)
                {
                    chatsViewModel.SelectedElement.Settings = this.ChatSettings;
                }
            }
        }
        
        private async void OnCancelClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.CancelSettings();
        }
   
        private void OnCollapseClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CollapsePanel(ChatPanelTypes.ChatSettings);
        }

        #endregion
    }
}

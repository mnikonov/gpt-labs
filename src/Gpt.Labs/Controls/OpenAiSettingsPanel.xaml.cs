using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Exceptions;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Enums;
using Microsoft.UI.Xaml;
using OpenAI;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.Controls
{
    public sealed partial class OpenAiSettingsPanel : BaseUserControl
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

            if (this.ChatSettings.HasErrors)
            {
                return;
            }

            await CheckOpenAiAuthentication();

            if (this.ChatSettings.HasErrors)
            {
                return;
            }

            await this.ViewModel.SaveSettings();

            var chatsViewModel = this.GetParent<ChatsPage>().ViewModel.Result;

            if (chatsViewModel.SelectedElement.Id == ChatSettings.ChatId)
            {
                chatsViewModel.SelectedElement.Settings = this.ChatSettings;
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

        private async Task CheckOpenAiAuthentication()
        {
            if (string.IsNullOrEmpty(this.ChatSettings.OpenAIOrganization))
            {
                return;
            }

            try
            {
                var api = new OpenAIClient(new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, this.ChatSettings.OpenAIOrganization));
                await api.WrapAction((client) => client.ModelsEndpoint.GetModelsAsync());
            }
            catch (OpenAiException ex)
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    switch (ex.Code)
                    {
                        case "invalid_organization":
                            this.ChatSettings.AddError(nameof(this.ChatSettings.OpenAIOrganization), ex.Message);
                            break;
                        default:
                            this.ChatSettings.AddError(string.Empty, ex.Message);
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                ex.LogError();

                await this.DispatcherQueue.EnqueueAsync(async () =>
                {
                    await this.Window.CreateExceptionDialog(ex).ShowAsync();
                    this.ChatSettings.AddError(string.Empty, App.ResourceLoader.GetString("OpenAiUnexpectedAuthenticationError"));
                });
            }
        }

        #endregion
    }
}

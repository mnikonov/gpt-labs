using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Exceptions;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditChatDialog : ContentDialogBase
    {
        #region Constructors

        public EditChatDialog(Window window, OpenAIChat viewModel, IReadOnlyCollection<string> supportedModels)
            : base(window)
        {           
            this.SupportedAiModels = supportedModels;
            this.ViewModel = viewModel;

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIChat ViewModel { get; private set; }

        public IReadOnlyCollection<string> SupportedAiModels { get; private set; }

        #endregion

        #region Private Methods

        private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                this.ViewModel.Validate();
                this.ViewModel.Settings.Validate();

                if (this.ViewModel.HasErrors || this.ViewModel.Settings.HasErrors)
                {
                    return;
                }

                await CheckOpenAiAuthentication();
            }
            finally
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                { 
                    if (this.ViewModel.HasErrors || this.ViewModel.Settings.HasErrors)
                    {
                        args.Cancel = true;
                    }

                    deferral.Complete();
                });
            }
        }

        private async Task CheckOpenAiAuthentication()
        {
            if (string.IsNullOrEmpty(this.ViewModel.Settings.OpenAIOrganization))
            {
                return;
            }

            try
            {
                var api = new OpenAIClient(new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, this.ViewModel.Settings.OpenAIOrganization));
                await api.WrapAction((client) => client.ModelsEndpoint.GetModelsAsync());
            }
            catch (OpenAiException ex)
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    switch (ex.Code)
                    {
                        case "invalid_organization":
                            this.ViewModel.Settings.AddError(nameof(this.ViewModel.Settings.OpenAIOrganization), ex.Message);
                            break;
                        default:
                            this.ViewModel.AddError(string.Empty, ex.Message);
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                ex.LogError();

                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    this.ViewModel.AddError(string.Empty, App.ResourceLoader.GetString("OpenAiUnexpectedAuthenticationError"));
                });
            }
        }

        #endregion
    }
}

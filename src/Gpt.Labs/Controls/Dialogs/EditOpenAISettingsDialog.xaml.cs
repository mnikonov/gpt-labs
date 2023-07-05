using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Exceptions;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OpenAI;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditOpenAISettingsDialog : ContentDialogBase
    {
        #region Constructors

        public EditOpenAISettingsDialog(Window window)
            : base(window)
        {           
            this.ViewModel = new OpenAIApiSettings(ApplicationSettings.Instance.OpenAIOrganization, ApplicationSettings.Instance.OpenAIApiKey);

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIApiSettings ViewModel { get; private set; }

        #endregion

        #region Private Methods

        private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                this.ViewModel.Validate();

                if (this.ViewModel.HasErrors)
                {
                    return;
                }

                await CheckOpenAiAuthentication();
            }
            finally
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                { 
                    if (this.ViewModel.HasErrors)
                    {
                        args.Cancel = true;
                    }

                    deferral.Complete();
                });
            }
        }

        private async Task CheckOpenAiAuthentication()
        {
            try
            {
                var api = new OpenAIClient(new OpenAIAuthentication(this.ViewModel.ApiKey, this.ViewModel.Organization));
                await api.WrapAction((client) => client.ModelsEndpoint.GetModelsAsync());
            }
            catch (OpenAiException ex)
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    switch (ex.Code)
                    {
                        case "invalid_organization":
                            this.ViewModel.AddError(nameof(this.ViewModel.Organization), ex.Message);
                            break;
                        case "invalid_api_key":
                            this.ViewModel.AddError(nameof(this.ViewModel.ApiKey), ex.Message);
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

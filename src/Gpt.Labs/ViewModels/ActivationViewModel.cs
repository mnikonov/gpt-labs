using Gpt.Labs.Controls.Wizards.Activation;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Base;
using Gpt.Labs.Models.Exceptions;
using Gpt.Labs.ViewModels.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using OpenAI;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels
{
    public class ActivationViewModel : WizardViewModelBase
    {
        #region Fields

        private readonly Func<object[], Task> executeAndContinue;

        #endregion

        #region Constructors

        public ActivationViewModel(Func<BasePage> getBasePage, Func<object[], Task> executeAndContinue)
            : base(getBasePage)
        {
            this.executeAndContinue = executeAndContinue;
            this.step = this.GetInitializationStep();
        }

        #endregion

        #region Public Properties

        public Query NavigationParameter { get; set; }

        public bool HasAuthenticationSettings => !string.IsNullOrEmpty(ApplicationSettings.Instance.OpenAIApiKey);

        public bool CanNavigateToShell {get; private set; } = false;

        #endregion

        #region Private Methods

        public Task ExecuteAndContinue(params object[] args)
        {
            return this.executeAndContinue.Invoke(args);
        }

        private WizardStepCommandBase GetInitializationStep()
        {
            return new WizardStepCommand<ObservableValidationObject, InitializationControl>(
                null,
                (model) => new InitializationControl(),
                async (model, args) =>
                {
                    await this.MigrateDatabase();

                    if (this.HasAuthenticationSettings)
                    {
                        try
                        {
                            await CheckOpenAiAuthentication(ApplicationSettings.Instance.OpenAIApiKey, ApplicationSettings.Instance.OpenAIOrganization);
                        }
                        catch (OpenAiException ex)
                        {
                            await this.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                await this.Window.CreateErrorDialog(ex).ShowAsync();
                            });

                            return;
                        }
                        catch (Exception ex)
                        {
                            ex.LogError();

                            await this.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                await this.Window.CreateExceptionDialog(ex).ShowAsync();
                            });

                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                     
                    this.CanNavigateToShell = true;

                    await this.DispatcherQueue.EnqueueAsync(() =>
                    {
                        this.NavigateToShell(false);
                    });
                },
                null,
                () =>
                {
                    return !this.CanNavigateToShell ? GetOpenAISettingsStep() : null;
                },
                null,
                true,
                string.Empty,
                string.Empty);
        }

        private WizardStepCommandBase GetOpenAISettingsStep()
        {
            return new WizardStepCommand<OpenAIApiSettings, OpenAISettingsControl>(
                new OpenAIApiSettings(ApplicationSettings.Instance.OpenAIOrganization, ApplicationSettings.Instance.OpenAIApiKey),
                (model) => new OpenAISettingsControl(model),
                async (model, args) =>
                    {
                        try
                        {
                            await CheckOpenAiAuthentication(model.ApiKey, model.Organization);
                        }
                        catch (OpenAiException ex)
                        {
                            await this.DispatcherQueue.EnqueueAsync(() =>
                            {
                                switch (ex.Code)
                                {
                                    case "invalid_organization":
                                        model.AddError(nameof(model.Organization), ex.Message);
                                        break;
                                    case "invalid_api_key":
                                        model.AddError(nameof(model.ApiKey), ex.Message);
                                        break;
                                    default:
                                        model.AddError(string.Empty, ex.Message);
                                        break;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            ex.LogError();

                            await this.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                model.AddError(string.Empty, App.ResourceLoader.GetString("OpenAiUnexpectedAuthenticationError"));

                                var dialog = this.Window.CreateExceptionDialog(ex);

                                await dialog.ShowAsync();
                            });
                        }

                        if (model.HasErrors)
                        {
                            return;
                        }

                        ApplicationSettings.Instance.OpenAIOrganization = model.Organization;
                        ApplicationSettings.Instance.OpenAIApiKey = model.ApiKey;

                        await this.DispatcherQueue.EnqueueAsync(() =>
                        {
                            this.NavigateToShell(true);
                        });
                    }, 
                null, 
                null, 
                null, 
                false,
                "WizardDialog/Apply", 
                string.Empty);
        }

        private void NavigateToShell(bool setIsTerminated)
        {
            if (setIsTerminated)
            {
                this.NavigationParameter["IsTerminated"] = true;
            }

            ((Frame)this.Window.Content).Navigate(
                typeof(ShellPage),
                this.NavigationParameter.ToString(),
                new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private async Task CheckOpenAiAuthentication(string apiKey, string organization)
        {
            var api = new OpenAIClient(new OpenAIAuthentication(apiKey, organization));
            await api.WrapAction((client) => client.ModelsEndpoint.GetModelsAsync());
        }

        private async Task MigrateDatabase()
        {
            try
            {
                using (var db = new DataContext())
                {
                    await db.Database.MigrateAsync();

                    // var users = db.Profiles.ToList();
                    // var dbFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                }
            }
            catch (Exception ex)
            {
                ex.LogError("Unable to execute database migration");
                throw;
            }
        }

        #endregion
    }
}

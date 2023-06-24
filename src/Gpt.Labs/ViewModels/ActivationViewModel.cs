using Gpt.Labs.Controls.Wizards.Activation;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Base;
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

        public bool IsOpenAISettingsApplied 
        { 
            get
            {
                return !string.IsNullOrEmpty(ApplicationSettings.Instance.OpenAIOrganization) && !string.IsNullOrEmpty(ApplicationSettings.Instance.OpenAIApiKey);
            } 
        }

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

                    if (this.IsOpenAISettingsApplied)
                    {
                        await this.NavigateToShell(false);
                    }
                },
                null,
                GetOpenAISettingsStep,
                null,
                true,
                string.Empty,
                string.Empty);
        }

        private WizardStepCommandBase GetOpenAISettingsStep()
        {
            if (this.IsOpenAISettingsApplied)
            {
                return null;
            }

            return new WizardStepCommand<OpenAIApiSettings, OpenAISettingsControl>(
                new OpenAIApiSettings(ApplicationSettings.Instance.OpenAIOrganization, ApplicationSettings.Instance.OpenAIApiKey),
                (model) => new OpenAISettingsControl(model),
                async (model, args) =>
                    {
                        ApplicationSettings.Instance.OpenAIOrganization = model.Organization;
                        ApplicationSettings.Instance.OpenAIApiKey = model.ApiKey;

                        await this.NavigateToShell(true);
                    }, 
                null, 
                null, 
                null, 
                false,
                "WizardDialog/Apply", 
                string.Empty);
        }

        private async Task NavigateToShell(bool setIsTerminated)
        {
            if (setIsTerminated)
            {
                this.NavigationParameter["IsTerminated"] = true;
            }

            await InitAiModelsCollection(); 

            ((Frame)this.Window.Content).Navigate(
                typeof(ShellPage),
                this.NavigationParameter.ToString(),
                new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private async Task InitAiModelsCollection()
        {
            var api = new OpenAIClient(new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, ApplicationSettings.Instance.OpenAIOrganization ));
            ApplicationSettings.Instance.OpenAIModels = await api.ModelsEndpoint.GetModelsAsync();
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

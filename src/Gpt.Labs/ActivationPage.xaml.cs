using Gpt.Labs.ViewModels.Base;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models.Base;
using Gpt.Labs.Controls.Wizards.Activation;
using Gpt.Labs.Helpers;

namespace Gpt.Labs
{
    public sealed partial class ActivationPage : BasePage
    {
        #region Fields
        
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(ActivationViewModel),
            typeof(ActivationPage),
            new PropertyMetadata(null, null));
                
        #endregion

        #region Public Constructors

        public ActivationPage()
        {
            this.ViewModel = new ActivationViewModel(() => this, this.ExecuteStepAndContinue);
            this.InitializeComponent();
            
            this.Loaded += OnActivationWizardLoaded;
        }

        #endregion

        #region Properties

        public ActivationViewModel ViewModel
        {
            get => (ActivationViewModel)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        public ApplicationSettings Settings { get; } = ApplicationSettings.Instance;

        #endregion

        #region Private Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.NavigationParameter = Query.Parse(e.Parameter);
        }

        private async void OnActivationWizardLoaded(object sender, RoutedEventArgs e)
        {
            await this.InitWizardStep();
        }

        private async void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ExecuteStepAndContinue();
        }

        private async Task ExecuteStepAndContinue(params object[] executeArgs)
        {
            try
            {
                this.WizardFormContent.IsEnabled = false;
                this.Progress.Visibility = Visibility.Visible;

                await this.ViewModel.Step.ExecuteStepAsync(executeArgs);

                if (this.ViewModel.Step.Model != null)
                {
                    this.ErrorsPanel.Visibility = this.ViewModel.Step.Model.HasErrors ? Visibility.Visible : Visibility.Collapsed;
                    this.ErrorsList.ViewModel = this.ViewModel.Step.Model.Errors;
                }
                else
                {
                    this.ErrorsPanel.Visibility = Visibility.Collapsed;
                }

                if ((this.ViewModel.Step.Model == null || !this.ViewModel.Step.Model.HasErrors) && this.ViewModel.Step.HasNextStep)
                {
                    this.ViewModel.Step = this.ViewModel.Step.GetNextStep();

                    if (this.ViewModel.Step != null)
                    {
                        await this.InitWizardStep();
                    }
                }
            }
            finally
            {
                this.WizardFormContent.IsEnabled = true;
                this.Progress.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnPrevButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.Step.HasPrevStep)
            {
                this.ViewModel.Step = this.ViewModel.Step.PrevStep;
                await this.InitWizardStep();
            }
        }

        private async Task InitWizardStep()
        {
            if (this.ViewModel.Step is WizardStepCommand<ObservableValidationObject, InitializationControl>)
            {
                this.WizardFormContent.Visibility = Visibility.Collapsed;
                this.WizartInfoStepContainer.Visibility = Visibility.Visible;
                this.WizartInfoStepContainer.Child = this.ViewModel.Step.GetControl();
                this.WizardFormStepContainer.Child = null;
            }
            else
            {
                var nextText = this.ViewModel.Step.PrimaryButtonText;

                if (!string.IsNullOrEmpty(nextText))
                {
                    this.NextStep.Content = nextText;
                    this.NextStep.Visibility = Visibility.Visible;
                }
                else
                {
                    this.NextStep.Visibility = Visibility.Collapsed;
                }

                var prevText = this.ViewModel.Step.HasNextStep ? this.ViewModel.Step.SecondaryButtonText : string.Empty;

                if (!string.IsNullOrEmpty(prevText))
                {
                    this.PreviousStep.Content = prevText;
                    this.PreviousStep.Visibility = Visibility.Visible;
                }
                else
                {
                    this.PreviousStep.Visibility = Visibility.Collapsed;
                }

                this.WizartInfoStepContainer.Visibility = Visibility.Collapsed;
                this.WizardFormContent.Visibility = Visibility.Visible;
                this.WizardFormStepContainer.Child = this.ViewModel.Step.GetControl();
                this.WizartInfoStepContainer.Child = null;
            }

            await this.ViewModel.Step.InitializeStepAsync();

            if (this.ViewModel.Step.AutoExecute)
            {
                await ExecuteStepAndContinue();
            }
        }

        #endregion
    }
}

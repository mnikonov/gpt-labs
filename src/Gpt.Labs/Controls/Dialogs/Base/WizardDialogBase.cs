namespace Gpt.Labs.Controls.Dialogs.Base
{
    using System;
    using System.Threading.Tasks;
    using Gpt.Labs.ViewModels.Base;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public class WizardDialogBase : ContentDialogBase
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(WizardViewModelBase),
            typeof(WizardDialogBase),
            new PropertyMetadata(null, null));

        private Border container;

        private ProgressRing progress;

        #endregion

        #region Constructors

        public WizardDialogBase()
        {
            this.Loaded += this.OnDialogLoaded;

            this.PrimaryButtonClick += this.OnPrimaryButtonClick;
            this.SecondaryButtonClick += this.OnBackButtonClick;
            this.CloseButtonClick += this.OnCloseButtonClick;

            this.CloseButtonText = App.ResourceLoader.GetString("WizardDialog/Cancel");
        }

        #endregion

        #region Properties

        public WizardViewModelBase ViewModel
        {
            get => (WizardViewModelBase)this.GetValue(ViewModelProperty);

            protected set => this.SetValue(ViewModelProperty, value);
        }

        #endregion

        #region Private Methods

        protected async Task BlockUIAndExecute(Func<Task> action)
        {
            try
            {
                this.IsEnabled = false;

                if (this.progress != null)
                {
                    this.progress.IsActive = true;
                }

                await action();
            }
            finally
            {
                this.IsEnabled = true;
                if (this.progress != null)
                {
                    this.progress.IsActive = false;
                }
            }
        }

        protected async Task InitWizardStep()
        {
            this.PrimaryButtonText = this.ViewModel.Step.PrimaryButtonText;
            this.SecondaryButtonText = this.ViewModel.Step.SecondaryButtonText;

            await this.ViewModel.Step.InitializeStepAsync();

            this.container.Child = this.ViewModel.Step.GetControl();
        }

        protected async void OnBackButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            var cancelled = false;

            try
            {
                await this.BlockUIAndExecute(
                    async () =>
                        {
                            if (this.ViewModel.Step.HasPrevStep)
                            {
                                cancelled = true;

                                if (this.ViewModel.Step.CanCancel)
                                {
                                    this.ViewModel.Step.Cancel();
                                }

                                this.ViewModel.Step = this.ViewModel.Step.PrevStep;
                                await this.InitWizardStep();
                            }
                        });
            }
            finally
            {
                args.Cancel = cancelled;
                deferral.Complete();
            }
        }

        protected void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (this.ViewModel.Step.CanCancel)
            {
                this.ViewModel.Step.Cancel();
            }
        }

        protected async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            var cancelled = false;

            try
            {
                await this.BlockUIAndExecute(
                    async () =>
                        {
                            await this.ViewModel.Step.ExecuteStepAsync();

                            if (this.ViewModel.Step.HasNextStep && !this.ViewModel.Step.Model.HasErrors)
                            {
                                this.ViewModel.Step = this.ViewModel.Step.GetNextStep();

                                await this.InitWizardStep();

                                cancelled = true;
                            }
                            else if (this.ViewModel.Step.Model.HasErrors)
                            {
                                cancelled = true;
                            }
                        });
            }
            finally
            {
                args.Cancel = cancelled;
                deferral.Complete();
            }
        }

        protected void Init(Border container, ProgressRing progress)
        {
            this.container = container;
            this.progress = progress;
        }

        private async void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            await this.InitWizardStep();
        }

        #endregion
    }
}
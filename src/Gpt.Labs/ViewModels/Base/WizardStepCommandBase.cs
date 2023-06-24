using Gpt.Labs.Models.Base;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.Base
{
    public abstract class WizardStepCommandBase
    {
        #region Fields

        protected Func<WizardStepCommandBase> getNextStep;

        protected Func<object, object[], Task> execute;

        protected Func<object, Task> initialize;

        protected Func<object, UserControl> control;

        protected Action<object> cancel;

        protected string primaryButtonTextResource = "WizardDialog/Next";

        protected string secondaryButtonTextResource = "WizardDialog/Back";

        #endregion

        #region Properties

        public bool HasNextStep => this.getNextStep != null;
        
        public bool AutoExecute { get; set; }

        public WizardStepCommandBase PrevStep { get; set; }

        public bool HasPrevStep => this.PrevStep != null;

        public bool CanCancel => this.cancel != null;

        public ObservableValidationObject Model { get; protected set; }

        public string PrimaryButtonText => !string.IsNullOrEmpty(this.primaryButtonTextResource) 
                ? App.ResourceLoader.GetString(this.primaryButtonTextResource) : 
                string.Empty;

        public string SecondaryButtonText => !string.IsNullOrEmpty(this.secondaryButtonTextResource) && this.HasPrevStep
                ? App.ResourceLoader.GetString(this.secondaryButtonTextResource)
                : string.Empty;

        #endregion

        #region Public Methods

        public WizardStepCommandBase GetNextStep()
        {
            var step = this.getNextStep?.Invoke();
            if (step != null)
            {
                step.PrevStep = this;
            }

            return step;
        }

        public Control GetControl()
        {
            return this.control?.Invoke(this.Model);
        }

        public void Cancel()
        {
            this.cancel?.Invoke(this.Model);
        }

        public async Task ExecuteStepAsync(params object[] args)
        {
            this.Model?.Validate();

            if ((this.Model == null || (this.Model != null && !this.Model.HasErrors)) && this.execute != null)
            {
                await this.execute(this.Model, args);
            }
        }

        public async Task InitializeStepAsync()
        {
            if (this.initialize != null)
            {
                await this.initialize(this.Model);
            }
        }

        #endregion
    }
}

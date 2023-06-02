namespace Gpt.Labs.ViewModels.Base
{
    public class WizardViewModelBase : ViewModelBase
    {
        #region Fields

        protected WizardStepCommandBase step;

        #endregion

        #region Properties

        public WizardStepCommandBase Step
        {
            get => this.step;
            set => this.Set(ref this.step, value);
        }

        #endregion
    }
}

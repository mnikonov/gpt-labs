using Gpt.Labs.Models.Base;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.Base
{
    public class WizardStepCommand<TWizardModel, TWizardControl> : WizardStepCommandBase
        where TWizardModel : ObservableValidationObject
        where TWizardControl : UserControl
    {
        public WizardStepCommand(
            TWizardModel model,
            Func<TWizardModel, TWizardControl> control, 
            Func<TWizardModel, object[], Task> execute,
            Func<TWizardModel, Task> initialize = null, 
            Func<WizardStepCommandBase> getNextStep = null,
            Action<TWizardModel> cancel = null,
            bool autoExecute = false,
            string primaryButtonTextResource = null,
            string secondaryButtonTextResource = null)
        {
            this.Model = model;
            this.control = (stepModel) => control((TWizardModel)stepModel);

            if (execute != null)
            {
                this.execute = (stepModel, objects) => execute((TWizardModel)stepModel, objects);
            }

            this.AutoExecute = autoExecute;

            if (initialize != null)
            {
                this.initialize = (stepModel) => initialize((TWizardModel)stepModel);
            }

            if (cancel != null)
            {
                this.cancel = (stepModel) => cancel((TWizardModel)stepModel);
            }

            if (primaryButtonTextResource != null)
            {
                this.primaryButtonTextResource = primaryButtonTextResource;
            }

            if (secondaryButtonTextResource != null)
            {
                this.secondaryButtonTextResource = secondaryButtonTextResource;
            }

            this.getNextStep = getNextStep;
        }
    }
}

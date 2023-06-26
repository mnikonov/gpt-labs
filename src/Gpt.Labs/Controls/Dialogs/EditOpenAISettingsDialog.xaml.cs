using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.ViewModel.Validate();

            if (this.ViewModel.HasErrors)
            {
                args.Cancel = true;
            }
        }

        #endregion
    }
}

using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditStopDialog : ContentDialogBase
    {
        #region Constructors

        public EditStopDialog(OpenAIStop viewModel)
        {
            this.ViewModel = viewModel;

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIStop ViewModel { get; set; }

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

using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditLogitBiasDialog : ContentDialogBase
    {
        #region Constructors

        public EditLogitBiasDialog(Window window, OpenAILogitBias viewModel)
            : base(window)
        {
            this.ViewModel = viewModel;

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAILogitBias ViewModel { get; set; }

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

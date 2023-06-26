using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditImageDialog : ContentDialogBase
    {
        #region Constructors

        public EditImageDialog(Window window, OpenAIChat viewModel)
            : base(window)
        {           
            this.ViewModel = viewModel;

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIChat ViewModel { get; private set; }

        #endregion

        #region Private Methods

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.ViewModel.Validate();
            this.ViewModel.Settings.Validate();

            if (this.ViewModel.HasErrors || this.ViewModel.Settings.HasErrors)
            {
                args.Cancel = true;
            }
        }

        #endregion
    }
}

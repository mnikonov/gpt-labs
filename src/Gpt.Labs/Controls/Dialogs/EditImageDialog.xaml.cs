using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditImageDialog : ContentDialogBase
    {
        #region Constructors

        public EditImageDialog(Window window, OpenAIChat viewModel, IReadOnlyCollection<string> supportedModels)
            : base(window)
        {
            SupportedAiModels = supportedModels;
            ViewModel = viewModel;

            InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIChat ViewModel { get; private set; }

        public IReadOnlyCollection<string> SupportedAiModels { get; private set; }

        #endregion

        #region Private Methods

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.Validate();
            ViewModel.Settings.Validate();

            if (ViewModel.HasErrors || ViewModel.Settings.HasErrors)
            {
                args.Cancel = true;
            }
        }

        #endregion
    }
}

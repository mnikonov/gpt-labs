using Gpt.Labs.Controls.Dialogs.Base;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs.Controls.Dialogs
{
    public sealed partial class EditChatDialog : ContentDialogBase
    {
        #region Constructors

        public EditChatDialog(Window window, OpenAIChat viewModel)
            : base(window)
        {           
            this.SupportedAiModels = ApplicationSettings.Instance.OpenAIModels.Where(p => p.Id.Contains("gpt")).OrderByDescending(p => p.CreatedAt).Select(p => p.Id).ToList().AsReadOnly();
            this.ViewModel = viewModel;

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public OpenAIChat ViewModel { get; private set; }

        public IReadOnlyCollection<string> SupportedAiModels { get; private set; }

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

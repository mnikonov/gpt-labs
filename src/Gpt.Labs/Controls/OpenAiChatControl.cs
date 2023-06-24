using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Gpt.Labs.Controls
{
    public sealed class OpenAiChatControl : Control
    {
        #region Fields

        private Grid rootGrid;

        private AppBarButton openInWindowButton;

        private MenuFlyoutItem editButton;

        private MenuFlyoutItem deleteButton;

        #endregion

        #region Constructors

        public OpenAiChatControl()
        {
            this.DefaultStyleKey = typeof(OpenAiChatControl);
        }

        #endregion

        #region Properties

        private ChatsPage ParentPage => this.GetParent<ChatsPage>();

        private ChatsListViewModel ParentViewViewModel => this.ParentPage?.ViewModel?.Result;

        private OpenAIChat ViewModel => this.DataContext as OpenAIChat;

        #endregion

        #region Private Methods

        protected override void OnApplyTemplate()
        {
            this.rootGrid = (Grid)GetTemplateChild("RootGrid");

            this.openInWindowButton = (AppBarButton)GetTemplateChild("OpenChatInNewWindow");
            this.editButton = (MenuFlyoutItem)GetTemplateChild("Edit");
            this.deleteButton = (MenuFlyoutItem)GetTemplateChild("Delete");

            if (this.rootGrid != null)
            {
                this.rootGrid.PointerEntered -= OnRootGridPointerEntered;
                this.rootGrid.PointerEntered += OnRootGridPointerEntered;

                this.rootGrid.PointerExited -= OnRootGridPointerExited;
                this.rootGrid.PointerExited += OnRootGridPointerExited;
            }

            if (this.openInWindowButton != null)
            {
                this.openInWindowButton.Click -= this.OnOpenChatInNewWindowButtonClick;
                this.openInWindowButton.Click += this.OnOpenChatInNewWindowButtonClick;
            }

            if (this.editButton != null)
            {
                this.editButton.Click -= this.OnEditButtonClick;
                this.editButton.Click += this.OnEditButtonClick;
            }

            if (this.deleteButton != null)
            {
                this.deleteButton.Click -= this.OnDeleteButtonClick;
                this.deleteButton.Click += this.OnDeleteButtonClick;
            }

            base.OnApplyTemplate();
        }

        private void OnRootGridPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.ParentViewViewModel.HoveredElement = this.ViewModel;
            VisualStateManager.GoToState(this, "PointerEntered", true);
        }

        private void OnRootGridPointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.ParentViewViewModel.HoveredElement = null;
            VisualStateManager.GoToState(this, "PointerExited", true);
        }

        private async void OnOpenChatInNewWindowButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.OpenChatInNewWindows();
        }

        private async void OnEditButtonClick(object sender, RoutedEventArgs e)
        {
            var result = await this.ParentViewViewModel.AddEditChat(this.ViewModel);

            if (result && this.ParentViewViewModel.SelectedElement != null && this.ViewModel.Id == this.ParentViewViewModel.SelectedElement.Id)
            {
                ((MessagesPage)this.ParentPage.GetInnerFrame().Content).ViewModel.Result.Chat = this.ParentViewViewModel.SelectedElement;
            }
        }

        private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            await this.ParentViewViewModel.DeleteChats(this.ViewModel);

            await ParentPage.ClearBackState(this.ViewModel);
        }

        #endregion
    }
}

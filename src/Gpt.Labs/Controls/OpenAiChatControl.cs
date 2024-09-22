using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Gpt.Labs.Controls
{
    public sealed class OpenAiChatControl : BaseControl
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
            DefaultStyleKey = typeof(OpenAiChatControl);
        }

        #endregion

        #region Properties

        private ChatsPage ParentPage => this.GetParent<ChatsPage>();

        private ChatsListViewModel ParentViewViewModel => ParentPage?.ViewModel?.Result;

        private OpenAIChat ViewModel => DataContext as OpenAIChat;

        #endregion

        #region Private Methods

        protected override void OnApplyTemplate()
        {
            rootGrid = (Grid)GetTemplateChild("RootGrid");

            openInWindowButton = (AppBarButton)GetTemplateChild("OpenChatInNewWindow");
            editButton = (MenuFlyoutItem)GetTemplateChild("Edit");
            deleteButton = (MenuFlyoutItem)GetTemplateChild("Delete");

            if (rootGrid != null)
            {
                rootGrid.PointerEntered -= OnRootGridPointerEntered;
                rootGrid.PointerEntered += OnRootGridPointerEntered;

                rootGrid.PointerExited -= OnRootGridPointerExited;
                rootGrid.PointerExited += OnRootGridPointerExited;
            }

            if (openInWindowButton != null)
            {
                openInWindowButton.Click -= OnOpenChatInNewWindowButtonClick;
                openInWindowButton.Click += OnOpenChatInNewWindowButtonClick;
            }

            if (editButton != null)
            {
                editButton.Click -= OnEditButtonClick;
                editButton.Click += OnEditButtonClick;
            }

            if (deleteButton != null)
            {
                deleteButton.Click -= OnDeleteButtonClick;
                deleteButton.Click += OnDeleteButtonClick;
            }

            base.OnApplyTemplate();
        }

        private void OnRootGridPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (ParentViewViewModel != null)
            {
                ParentViewViewModel.HoveredElement = ViewModel;
            }

            VisualStateManager.GoToState(this, "PointerEntered", true);
        }

        private void OnRootGridPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (ParentViewViewModel != null)
            {
                ParentViewViewModel.HoveredElement = null;
            }

            VisualStateManager.GoToState(this, "PointerExited", true);
        }

        private async void OnOpenChatInNewWindowButtonClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.OpenChatInNewWindows();
        }

        private async void OnEditButtonClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                var result = await ParentViewViewModel.AddEditChat(ViewModel);

                if (result == ViewModels.Enums.SaveResult.Edited && ParentViewViewModel.SelectedElement != null && ViewModel.Id == ParentViewViewModel.SelectedElement.Id)
                {
                    ((MessagesPage)ParentPage.GetInnerFrame().Content).ViewModel.Result.Chat = ParentViewViewModel.SelectedElement;
                }
            });
        }

        private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                await ParentViewViewModel.DeleteChats(ViewModel);
                await ParentPage.ClearBackState(ViewModel);
            });
        }

        #endregion
    }
}

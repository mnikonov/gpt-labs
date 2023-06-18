using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Base;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Input;
using Gpt.Labs.Controls.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.ViewModels.Enums;
using Gpt.Labs.Helpers;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace Gpt.Labs
{
    public sealed partial class MessagesPage : BasePage
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(NotifyTaskCompletion<MessagesListViewModel>),
            typeof(MessagesPage),
            new PropertyMetadata(null, null));  

        #endregion

        #region Constructors

        public MessagesPage()
        {
            this.ViewModel = new NotifyTaskCompletion<MessagesListViewModel>();

            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public NotifyTaskCompletion<MessagesListViewModel> ViewModel
        {
            get => (NotifyTaskCompletion<MessagesListViewModel>)GetValue(ViewModelProperty);

            private set => SetValue(ViewModelProperty, value);
        }

        #endregion

        #region Public Methods

        public override void LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            base.LoadState(destinationPageType, parameters, state, mode);

            ViewModel.Function = async token =>
            {
                var viewModel = new MessagesListViewModel();

                await viewModel.LoadStateAsync(destinationPageType, parameters, state, mode);

                var chatPage = this.GetParent<ChatsPage>();
                
                if (chatPage != null && chatPage.ViewModel.IsCompleted)
                {
                    chatPage.ViewModel.Result.SetSelection(viewModel.ChatId);
                }

                return viewModel;
            };
                        
            ViewModel.Start();
        }

        public override void SaveState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            ViewModel.Result?.SaveState(destinationPageType, parameters, state, mode);
            base.SaveState(destinationPageType, parameters, state, mode);
        }

        #endregion

        #region Private Methods

        private async void OnMessageTextBoxPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            var isCtrlDown = ctrlState == CoreVirtualKeyStates.Down || ctrlState == (CoreVirtualKeyStates.Down | CoreVirtualKeyStates.Locked);
            var isShiftDown = shiftState == CoreVirtualKeyStates.Down || shiftState == (CoreVirtualKeyStates.Down | CoreVirtualKeyStates.Locked);

            if (isCtrlDown)
            {
                switch (e.Key)
                {
                    case VirtualKey.R:
                        await ViewModel.Result.StartStopRecord();
                        e.Handled = true;
                        return;

                    case VirtualKey.S:
                        ViewModel.Result.ExpandCollapsePanel(ChatPanelTypes.ChatSettings);
                        e.Handled = true;
                        return;

                    case VirtualKey.M:
                        this.ChangeSelectMuliState();
                        e.Handled = true;
                        return;

                    case VirtualKey.D:
                        await this.DeleteChats();
                        e.Handled = true;
                        return;

                    case VirtualKey.C:
                        var copyMessages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
                        await this.ViewModel.Result.CopyMessages(copyMessages);
                        e.Handled = true;
                        return;

                    case VirtualKey.E:
                        var shareMessages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
                        this.ViewModel.Result.ShareMessages(shareMessages);
                        e.Handled = true;
                        return;
                }
            }

            if (isShiftDown)
            {
                switch (e.Key)
                {
                   case VirtualKey.Enter:
                        this.MessageTextBox.Text += "\r";
                        this.MessageTextBox.SelectionStart = this.MessageTextBox.Text.Length;
                        this.MessageTextBox.SelectionLength = 0;
                        e.Handled = true;
                        return;
                }
            }

            if (!isCtrlDown && !isShiftDown)
            {
                switch (e.Key)
                {
                    case VirtualKey.Enter:
                        await this.SendChatMessage();
                        e.Handled = true;
                        return;
                }
            }
        }
                
        private async void OnMessageTextBoxPreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key == VirtualKey.Control || e.Key == VirtualKey.R) && this.ViewModel.Result.IsRecording)
            {
                await ViewModel.Result.StartStopRecord();
                e.Handled = true;
            }
        }

        private void OnShowHideChatSettingsClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Result.ExpandCollapsePanel(ChatPanelTypes.ChatSettings);
        }

        private void OnSelectMultiClick(object sender, RoutedEventArgs e)
        {
            this.ChangeSelectMuliState();
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            if (this.SelectAll.IsChecked == true)
            {
                this.MessagesList.SelectedItems.Clear();

                foreach (var item in this.ViewModel.Result.ItemsCollection)
                {
                    this.MessagesList.SelectedItems.Add(item);
                }
            }
            else
            {
                this.MessagesList.SelectedItems.Clear();
            }
        }

        private void OnChatListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel.Result.MultiSelectModeEnabled)
            {
                this.DeleteMulti.IsEnabled = this.MessagesList.SelectedItems.Count > 0;
                this.ShareMulti.IsEnabled = this.MessagesList.SelectedItems.Count > 0;
                this.CopyMulti.IsEnabled = this.MessagesList.SelectedItems.Count > 0;

                this.SelectAll.IsChecked = this.MessagesList.SelectedItems.Count > 0 && 
                        this.MessagesList.SelectedItems.Count == this.ViewModel.Result.ItemsCollection.Count;
            }
        }

        private async void OnDeleteMultiClick(object sender, RoutedEventArgs e)
        {
            await this.DeleteChats();
        }

        private async void OnCopyMultiClick(object sender, RoutedEventArgs e)
        {
            var messages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
            await this.ViewModel.Result.CopyMessages(messages);
        }

        private void OnShareMultiClick(object sender, RoutedEventArgs e)
        {
            var messages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
            this.ViewModel.Result.ShareMessages(messages);
        }

        private void ChangeSelectMuliState()
        {
            this.ViewModel.Result.MultiSelectModeEnabled = !this.ViewModel.Result.MultiSelectModeEnabled;

            if (this.ViewModel.Result.MultiSelectModeEnabled)
            {
                this.DeleteMulti.IsEnabled = false;
                this.ShareMulti.IsEnabled = false;
                this.CopyMulti.IsEnabled = false;

                this.SelectAll.IsChecked = false;
            }
        }

        private async Task SendChatMessage()
        {
            try
            {
                this.MessageProgress.Visibility = Visibility.Visible;

                await this.MessagePanel.DisableUiAndExecuteAsync(async token =>
                {
                    await ViewModel.Result.SendMessage();
                });
            }
            finally
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    this.MessageProgress.Visibility = Visibility.Collapsed;
                    this.MessageTextBox.Focus(FocusState.Programmatic);
                });
            }
        }

        private async Task CreateImageVariation()
        {
            try
            {
                this.MessageProgress.Visibility = Visibility.Visible;

                await this.MessagePanel.DisableUiAndExecuteAsync(async token =>
                {
                    await ViewModel.Result.CreateImageVariation();
                });
            }
            finally
            {
                await this.DispatcherQueue.EnqueueAsync(() =>
                {
                    this.MessageProgress.Visibility = Visibility.Collapsed;
                    this.MessageTextBox.Focus(FocusState.Programmatic);
                });
            }
        }

        private async Task DeleteChats()
        {
            var messages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();

            if (messages.Length == 0)
            {
                return;
            }

            await this.ViewModel.Result.DeleteMessages(messages);

            if (this.ViewModel.Result.MultiSelectModeEnabled && this.ViewModel.Result.ItemsCollection.Count == 0)
            {
                this.ViewModel.Result.MultiSelectModeEnabled = false;
            }
        }
                
        private void OnMessagePanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MessagesList.Padding = new Thickness(MessagesList.Padding.Left, MessagesList.Padding.Top, MessagesList.Padding.Right, e.NewSize.Height);
        }

        #endregion
    }
}

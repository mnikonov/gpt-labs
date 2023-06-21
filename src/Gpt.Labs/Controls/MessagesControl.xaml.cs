using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Enums;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace Gpt.Labs.Controls
{
    public sealed partial class MessagesControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MessagesListViewModel),
            typeof(MessagesPage),
            new PropertyMetadata(null, null));  

        public static readonly DependencyProperty ShowSettingsButtonProperty = DependencyProperty.Register(
            nameof(ShowSettingsButton),
            typeof(bool),
            typeof(MessagesPage),
            new PropertyMetadata(true, null));

        public static readonly DependencyProperty IsMessagePanelEnabledProperty = DependencyProperty.Register(
            nameof(IsMessagePanelEnabled),
            typeof(bool),
            typeof(MessagesPage),
            new PropertyMetadata(true, null));

        #endregion

        #region Public Constructors

        public MessagesControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public MessagesListViewModel ViewModel
        {
            get => (MessagesListViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public bool ShowSettingsButton
        {
            get => (bool)GetValue(ShowSettingsButtonProperty);
            set => SetValue(ShowSettingsButtonProperty, value);
        }

        public bool IsMessagePanelEnabled
        {
            get => (bool)GetValue(IsMessagePanelEnabledProperty);
            set => SetValue(IsMessagePanelEnabledProperty, value);
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
                        await ViewModel.StartStopRecord();
                        e.Handled = true;
                        return;

                    case VirtualKey.S:
                        ViewModel.ExpandCollapsePanel(ChatPanelTypes.ChatSettings);
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
                        await this.ViewModel.CopyMessages(copyMessages);
                        e.Handled = true;
                        return;

                    case VirtualKey.E:
                        var shareMessages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
                        this.ViewModel.ShareMessages(shareMessages);
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
            if ((e.Key == VirtualKey.Control || e.Key == VirtualKey.R) && this.ViewModel.IsRecording)
            {
                await ViewModel.StartStopRecord();
                e.Handled = true;
            }
        }

        private void OnShowHideChatSettingsClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ExpandCollapsePanel(ChatPanelTypes.ChatSettings);
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

                foreach (var item in this.ViewModel.ItemsCollection)
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
            if (this.ViewModel.MultiSelectModeEnabled)
            {
                this.DeleteMulti.IsEnabled = this.MessagesList.SelectedItems.Count > 0;
                this.ShareMulti.IsEnabled = this.MessagesList.SelectedItems.Count > 0;
                this.CopyMulti.IsEnabled = this.MessagesList.SelectedItems.Count > 0;

                this.SelectAll.IsChecked = this.MessagesList.SelectedItems.Count > 0 && 
                        this.MessagesList.SelectedItems.Count == this.ViewModel.ItemsCollection.Count;
            }
        }

        private async void OnDeleteMultiClick(object sender, RoutedEventArgs e)
        {
            await this.DeleteChats();
        }

        private async void OnCopyMultiClick(object sender, RoutedEventArgs e)
        {
            var messages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
            await this.ViewModel.CopyMessages(messages);
        }

        private void OnShareMultiClick(object sender, RoutedEventArgs e)
        {
            var messages = this.MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
            this.ViewModel.ShareMessages(messages);
        }

        private void ChangeSelectMuliState()
        {
            this.ViewModel.MultiSelectModeEnabled = !this.ViewModel.MultiSelectModeEnabled;

            if (this.ViewModel.MultiSelectModeEnabled)
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
                    await ViewModel.SendMessage();
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
                    await ViewModel.CreateImageVariation();
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

            await this.ViewModel.DeleteMessages(messages);

            if (this.ViewModel.MultiSelectModeEnabled && this.ViewModel.ItemsCollection.Count == 0)
            {
                this.ViewModel.MultiSelectModeEnabled = false;
            }
        }
                
        private void OnMessagePanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MessagesList.Padding = new Thickness(MessagesList.Padding.Left, MessagesList.Padding.Top, MessagesList.Padding.Right, e.NewSize.Height);
        }

        #endregion
    }
}

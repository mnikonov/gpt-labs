using Gpt.Labs.Controls.Extensions;
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

        public static readonly DependencyProperty ShowOpenNewWindowButtonProperty = DependencyProperty.Register(
            nameof(ShowOpenNewWindowButton),
            typeof(bool),
            typeof(MessagesPage),
            new PropertyMetadata(true, null));

        #endregion

        #region Public Constructors

        public MessagesControl()
        {
            InitializeComponent();
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

        public bool ShowOpenNewWindowButton
        {
            get => (bool)GetValue(ShowOpenNewWindowButtonProperty);
            set => SetValue(ShowOpenNewWindowButtonProperty, value);
        }

        #endregion

        #region Private Methods

        private async void OnMessageTextBoxPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            var isCtrlDown = ctrlState is CoreVirtualKeyStates.Down or (CoreVirtualKeyStates.Down | CoreVirtualKeyStates.Locked);
            var isShiftDown = shiftState is CoreVirtualKeyStates.Down or (CoreVirtualKeyStates.Down | CoreVirtualKeyStates.Locked);

            if (isCtrlDown)
            {
                switch (e.Key)
                {
                    case VirtualKey.R:
                        await ViewModel.StartStopRecord();
                        e.Handled = true;
                        return;

                    case VirtualKey.I:
                        ViewModel.ExpandCollapsePanel(ChatPanelTypes.ChatSettings);
                        e.Handled = true;
                        return;

                    case VirtualKey.M:
                        ChangeSelectMuliState();
                        e.Handled = true;
                        return;

                    case VirtualKey.Delete:
                        await DeleteChats();
                        e.Handled = true;
                        return;

                    case VirtualKey.C:
                        var copyMessages = MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
                        await ViewModel.CopyMessages(copyMessages);
                        e.Handled = true;
                        return;

                    case VirtualKey.H:
                        var shareMessages = MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
                        ViewModel.ShareMessages(shareMessages);
                        e.Handled = true;
                        return;

                    case VirtualKey.N:
                        ViewModel.OpenChatInNewWindow();
                        e.Handled = true;
                        return;

                    case VirtualKey.G:
                        await ViewModel.RegenerateResponse();
                        e.Handled = true;
                        return;

                    case VirtualKey.D:
                        await ViewModel.DeleteLastMessages();
                        e.Handled = true;
                        return;
                }
            }

            if (isShiftDown)
            {
                switch (e.Key)
                {
                    case VirtualKey.Enter:
                        MessageTextBox.Text += "\r";
                        MessageTextBox.SelectionStart = MessageTextBox.Text.Length;
                        MessageTextBox.SelectionLength = 0;
                        e.Handled = true;
                        return;
                }
            }

            if (!isCtrlDown && !isShiftDown)
            {
                switch (e.Key)
                {
                    case VirtualKey.Enter:
                        await SendChatMessage();
                        e.Handled = true;
                        return;
                }
            }
        }

        private async void OnMessageTextBoxPreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key == VirtualKey.Control || e.Key == VirtualKey.R) && ViewModel.IsRecording)
            {
                await ViewModel.StartStopRecord();
                e.Handled = true;
            }
        }

        private void OnShowHideChatSettingsClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ExpandCollapsePanel(ChatPanelTypes.ChatSettings);
        }

        private void OnSelectMultiClick(object sender, RoutedEventArgs e)
        {
            ChangeSelectMuliState();
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            if (SelectAll.IsChecked == true)
            {
                MessagesList.SelectedItems.Clear();

                foreach (var item in ViewModel.ItemsCollection)
                {
                    MessagesList.SelectedItems.Add(item);
                }
            }
            else
            {
                MessagesList.SelectedItems.Clear();
            }
        }

        private void OnChatListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.MultiSelectModeEnabled)
            {
                DeleteMulti.IsEnabled = MessagesList.SelectedItems.Count > 0;
                ShareMulti.IsEnabled = MessagesList.SelectedItems.Count > 0;
                CopyMulti.IsEnabled = MessagesList.SelectedItems.Count > 0;

                SelectAll.IsChecked = MessagesList.SelectedItems.Count > 0 &&
                        MessagesList.SelectedItems.Count == ViewModel.ItemsCollection.Count;
            }
        }

        private async void OnDeleteMultiClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(DeleteChats);
        }

        private async void OnCopyMultiClick(object sender, RoutedEventArgs e)
        {
            var messages = MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
            await ViewModel.CopyMessages(messages);
        }

        private void OnShareMultiClick(object sender, RoutedEventArgs e)
        {
            var messages = MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();
            ViewModel.ShareMessages(messages);
        }

        private void ChangeSelectMuliState()
        {
            ViewModel.MultiSelectModeEnabled = !ViewModel.MultiSelectModeEnabled;

            if (ViewModel.MultiSelectModeEnabled)
            {
                DeleteMulti.IsEnabled = false;
                ShareMulti.IsEnabled = false;
                CopyMulti.IsEnabled = false;

                SelectAll.IsChecked = false;
            }
        }

        private async Task SendChatMessage()
        {
            await ViewModel.SendMessage();
        }

        private async Task CreateImageVariation()
        {
            await ViewModel.CreateImageVariation();
        }

        private async Task DeleteChats()
        {
            var messages = MessagesList.SelectedItems.OfType<OpenAIMessage>().ToArray();

            if (messages.Length == 0)
            {
                return;
            }

            await ViewModel.DeleteMessages(true, messages);

            if (ViewModel.MultiSelectModeEnabled && ViewModel.ItemsCollection.Count == 0)
            {
                ViewModel.MultiSelectModeEnabled = false;
            }
        }

        private void OnMessagePanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MessagesList.Padding = new Thickness(MessagesList.Padding.Left, MessagesList.Padding.Top, MessagesList.Padding.Right, e.NewSize.Height);
        }

        private void OnMessagePanelIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool val && val)
            {
                MessageTextBox.Focus(FocusState.Programmatic);
            }
        }

        #endregion
    }
}

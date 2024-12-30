using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Base;
using Gpt.Labs.ViewModels.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gpt.Labs
{
    public sealed partial class ChatsPage : StatePage
    {
        #region Fields

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(NotifyTaskCompletion<ChatsListViewModel>),
            typeof(ChatsPage),
            new PropertyMetadata(null, null));

        private Guid frameUid;

        private Frame chatFrame;

        #endregion

        #region Constructors

        public ChatsPage()
        {
            ViewModel = new NotifyTaskCompletion<ChatsListViewModel>();
            InitializeComponent();
        }

        #endregion

        #region Properties

        public NotifyTaskCompletion<ChatsListViewModel> ViewModel
        {
            get => (NotifyTaskCompletion<ChatsListViewModel>)GetValue(ViewModelProperty);

            private set => SetValue(ViewModelProperty, value);
        }

        #endregion

        #region Public Methods

        public override async Task LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            frameUid = parameters.GetValue<Guid>("frame-uid");

            await RegisterFrame();

            await base.LoadState(destinationPageType, parameters, state, mode);

            ViewModel.Function = async token =>
            {
                var viewModel = new ChatsListViewModel(() => RootPage);

                await viewModel.LoadStateAsync(destinationPageType, parameters, state, mode);

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

            RootPage?.SuspensionManager?.SaveFrameNavigationState(chatFrame);
            RootPage?.SuspensionManager?.UnregisterFrame(chatFrame, false);
        }

        public override Frame GetInnerFrame()
        {
            return chatFrame;
        }

        public async Task ClearBackState(params OpenAIChat[] chats)
        {
            if (ViewModel.Result.MultiSelectModeEnabled && ViewModel.Result.ItemsCollection.Count == 0)
            {
                ViewModel.Result.MultiSelectModeEnabled = false;
            }

            var sessionState = RootPage?.SuspensionManager?.SessionStateForFrame(chatFrame);

            foreach (var chat in chats)
            {
                bool hasRemovedStates = false;
                foreach (var state in sessionState.PageState.ToList())
                {
                    var chatId = state.Value.GetValue<Guid>(nameof(MessagesListViewModel.ChatId));
                    if (chatId == chat.Id)
                    {
                        sessionState.PageState.Remove(state.Key);
                        hasRemovedStates = true;
                    }
                }

                if (hasRemovedStates)
                {
                    var i = 0;

                    var states = sessionState.PageState.ToList();
                    foreach (var state in sessionState.PageState.ToList())
                    {
                        var newKey = $"Page-{i}";

                        if (newKey != state.Key)
                        {
                            var index = states.IndexOf(state);
                            states.RemoveAt(index);
                            states.Insert(index, new KeyValuePair<string, ViewModelState>(newKey, state.Value));
                        }

                        i++;
                    }

                    sessionState.PageState = states.ToDictionary(p => p.Key, p => p.Value);
                }

                foreach (var stack in chatFrame.BackStack.ToList())
                {
                    var query = Query.Parse(stack.Parameter);
                    var chatId = query.GetValue<Guid>("chat-id");

                    if (chatId == chat.Id)
                    {
                        chatFrame.BackStack.Remove(stack);
                    }
                }
            }

            if (chatFrame.Content is not null and StatePage page)
            {
                page.NavigationHelper.SetPageKey();
            }

            if (ViewModel.Result.SelectedElement == null && chatFrame.Content != null)
            {
                if (chatFrame.CanGoBack)
                {
                    chatFrame.GoBack();
                }
                else
                {
                    await RegisterFrame();
                }
            }

            foreach (var chat in chats)
            {
                foreach (var stack in chatFrame.ForwardStack.ToList())
                {
                    var query = Query.Parse(stack.Parameter);
                    var chatId = query.GetValue<Guid>("chat-id");

                    if (chatId == chat.Id)
                    {
                        chatFrame.ForwardStack.Remove(stack);
                    }
                }
            }

            RootPage?.UpdateBackState();
        }

        #endregion

        #region Private Methods

        private async void OnAddChatClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                var result = await ViewModel.Result.AddEditChat(null);

                if (result == SaveResult.Added)
                {
                    SelectChat(ViewModel.Result.ItemsCollection.FirstOrDefault());
                }
            });
        }

        private void OnChatListItemClick(object sender, ItemClickEventArgs e)
        {
            var chat = (OpenAIChat)e.ClickedItem;

            if (ViewModel.Result.SelectedElement?.Id != chat?.Id)
            {
                SelectChat(chat);
            }
        }

        private async void OnChatListDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var chat = args.Items.OfType<OpenAIChat>().FirstOrDefault();

            if (chat != null)
            {
                await ViewModel.Result.UpdateChatPosition(chat);
            }
        }

        private void OnSelectMultiClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Result.MultiSelectModeEnabled = !ViewModel.Result.MultiSelectModeEnabled;

            if (!ViewModel.Result.MultiSelectModeEnabled)
            {
                Bindings.Update();
            }
            else
            {
                SelectAll.IsChecked = false;
                DeleteMulti.IsEnabled = false;
            }
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            if (SelectAll.IsChecked == true)
            {
                ChatList.SelectedItems.Clear();

                foreach (var item in ViewModel.Result.ItemsCollection)
                {
                    ChatList.SelectedItems.Add(item);
                }
            }
            else
            {
                ChatList.SelectedItems.Clear();
            }
        }

        private void OnChatListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.Result.MultiSelectModeEnabled)
            {
                DeleteMulti.IsEnabled = ChatList.SelectedItems.Count > 0;
                SelectAll.IsChecked = ChatList.SelectedItems.Count > 0 && ChatList.SelectedItems.Count == ViewModel.Result.ItemsCollection.Count;
            }
        }

        private async void OnDeleteMultiClick(object sender, RoutedEventArgs e)
        {
            await sender.DisableUiAndExecuteAsync(async () =>
            {
                var chats = ChatList.SelectedItems.OfType<OpenAIChat>().ToArray();
                await ViewModel.Result.DeleteChats(chats);

                await ClearBackState(chats);
            });
        }

        private async Task RegisterFrame()
        {
            if (chatFrame != null)
            {
                RootPage?.SuspensionManager?.UnregisterFrame(chatFrame, true);

                RootGrid.Children.Remove(chatFrame);
            }

            chatFrame = new Frame();
            Grid.SetColumn(chatFrame, 1);
            RootGrid.Children.Add(chatFrame);

            await chatFrame.ExecuteOnLoaded(() =>
            {
                RootPage?.SuspensionManager?.RegisterFrame(chatFrame, $"ChatFrameState_{frameUid}");
            });
        }

        private void SelectChat(OpenAIChat chat)
        {
            if (chat == null)
            {
                return;
            }

            ViewModel.Result.SelectChat(chat);

            var query = new Query
            {
                { "chat-id", chat.Id }
            };

            chatFrame.Navigate(typeof(MessagesPage), query.ToString(), new EntranceNavigationTransitionInfo());
        }

        #endregion
    }
}

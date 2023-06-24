using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Base;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

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
            this.ViewModel = new NotifyTaskCompletion<ChatsListViewModel>();
            this.InitializeComponent();
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

        public override void LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            this.frameUid = parameters.GetValue<Guid>("frame-uid");

            this.RegisterFrame();
            
            base.LoadState(destinationPageType, parameters, state, mode);

            ViewModel.Function = async token =>
            {
                var viewModel = new ChatsListViewModel(() => this.RootPage);

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

            this.RootPage?.SuspensionManager?.SaveFrameNavigationState(this.chatFrame);
            this.RootPage?.SuspensionManager?.UnregisterFrame(this.chatFrame, false);
        }

        public override Frame GetInnerFrame()
        {
            return this.chatFrame;
        }

        public void ClearBackState(params OpenAIChat[] chats)
        {
            if (this.ViewModel.Result.MultiSelectModeEnabled && this.ViewModel.Result.ItemsCollection.Count == 0)
            {
                this.ViewModel.Result.MultiSelectModeEnabled = false;
            }

            var sessionState = this.RootPage?.SuspensionManager?.SessionStateForFrame(this.chatFrame);

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

                        i ++;
                    }

                    sessionState.PageState = states.ToDictionary(p => p.Key, p => p.Value);
                }

                foreach (var stack in this.chatFrame.BackStack.ToList())
                {
                    var query = Query.Parse(stack.Parameter);
                    var chatId = query.GetValue<Guid>("chat-id");

                    if (chatId == chat.Id)
                    {
                        this.chatFrame.BackStack.Remove(stack);
                    }
                }
            }

            if (this.chatFrame.Content != null && this.chatFrame.Content is StatePage page)
            {
                page.NavigationHelper.SetPageKey();
            }

            if (this.ViewModel.Result.SelectedElement == null && this.chatFrame.Content != null)
            {
                if (this.chatFrame.CanGoBack)
                {
                    this.chatFrame.GoBack();
                }
                else
                {
                    this.RegisterFrame();
                }
            }

            foreach (var chat in chats) 
            {
                foreach (var stack in this.chatFrame.ForwardStack.ToList())
                {
                    var query = Query.Parse(stack.Parameter);
                    var chatId = query.GetValue<Guid>("chat-id");

                    if (chatId == chat.Id)
                    {
                        this.chatFrame.ForwardStack.Remove(stack);
                    }
                }
            }

            this.RootPage?.UpdateBackState();
        }

        #endregion

        #region Private Methods

        private async void OnAddChatClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.Result.AddEditChat(null);
        }
                
        private void OnChatListItemClick(object sender, ItemClickEventArgs e)
        {
            var chat = (OpenAIChat)e.ClickedItem;

            if (this.ViewModel.Result.SelectedElement?.Id != chat?.Id)
            {
                this.ViewModel.Result.SelectChat(chat);

                var query = new Query
                {
                    { "chat-id", chat.Id }
                };

                this.chatFrame.Navigate(typeof(MessagesPage), query.ToString(), new EntranceNavigationTransitionInfo());
            }
        }

        private async void OnChatListDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var chat = args.Items.OfType<OpenAIChat>().FirstOrDefault();

            if (chat != null)
            {
                await this.ViewModel.Result.UpdateChatPosition(chat);
            }
        }

        private void OnSelectMultiClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Result.MultiSelectModeEnabled = !this.ViewModel.Result.MultiSelectModeEnabled;

            if (!this.ViewModel.Result.MultiSelectModeEnabled)
            {
                this.Bindings.Update();
            }
            else
            {
                this.SelectAll.IsChecked = false;
                this.DeleteMulti.IsEnabled = false;
            }
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            if (this.SelectAll.IsChecked == true)
            {
                this.ChatList.SelectedItems.Clear();

                foreach (var item in this.ViewModel.Result.ItemsCollection)
                {
                    this.ChatList.SelectedItems.Add(item);
                }
            }
            else
            {
                this.ChatList.SelectedItems.Clear();
            }
        }

        private void OnChatListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel.Result.MultiSelectModeEnabled)
            {
                this.DeleteMulti.IsEnabled = this.ChatList.SelectedItems.Count > 0;
                this.SelectAll.IsChecked = this.ChatList.SelectedItems.Count > 0 && this.ChatList.SelectedItems.Count == this.ViewModel.Result.ItemsCollection.Count;
            }
        }

        private async void OnDeleteMultiClick(object sender, RoutedEventArgs e)
        {
            var chats = this.ChatList.SelectedItems.OfType<OpenAIChat>().ToArray();
            await this.ViewModel.Result.DeleteChats(chats);

            ClearBackState(chats);
        }

        private void RegisterFrame()
        {
            if (this.chatFrame != null)
            {
                this.RootPage?.SuspensionManager?.UnregisterFrame(this.chatFrame, true);

                this.RootGrid.Children.Remove(this.chatFrame);
            }

            this.chatFrame = new Frame();
            Grid.SetColumn(this.chatFrame, 1);
            this.RootGrid.Children.Add(this.chatFrame);

            this.RootPage?.SuspensionManager?.RegisterFrame(this.chatFrame, $"ChatFrameState_{this.frameUid}");
        }

        #endregion
    }
}

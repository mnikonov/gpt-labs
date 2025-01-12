using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels.Collections;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Gpt.Labs.Controls.DependencyExtensions;

public class TaskBarMenuFlyout : DependencyObject
{
    #region Fields

    public static readonly DependencyProperty ChatsCollectionProperty = DependencyProperty.Register(
        "ChatsCollection",
        typeof(ObservableList<OpenAIChat, Guid>),
        typeof(DependencyObject),
        new PropertyMetadata(null, OnChatsCollectionChanged));

    public static readonly DependencyProperty ChatProperty = DependencyProperty.Register(
       "Chat",
       typeof(OpenAIChat),
       typeof(DependencyObject),
       new PropertyMetadata(null, null));

    #endregion

    #region Public Methods

    public static ObservableList<OpenAIChat, Guid> GetChatsCollection(DependencyObject element)
    {
        return (ObservableList<OpenAIChat, Guid>)element.GetValue(ChatsCollectionProperty);
    }

    public static void SetChatsCollection(DependencyObject element, ObservableList<OpenAIChat, Guid> value)
    {
        element.SetValue(ChatsCollectionProperty, value);
    }

    public static OpenAIChat GetChat(DependencyObject element)
    {
        return (OpenAIChat)element.GetValue(ChatsCollectionProperty);
    }

    public static void SetChat(DependencyObject element, OpenAIChat value)
    {
        element.SetValue(ChatsCollectionProperty, value);
    }

    #endregion

    #region Private Methods

    private static void OnChatsCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var menu = d as MenuFlyoutSubItem;
        var collection = e.NewValue as ObservableList<OpenAIChat, Guid>;

        if (menu != null)
        {
            InitializeMenuFlyoutItems(menu, collection);

            if (collection != null)
            {
                collection.CollectionChanged += (s, e) =>
                {
                    InitializeMenuFlyoutItems(menu, collection);
                };
            }
        }
    }

    private static void InitializeMenuFlyoutItems(MenuFlyoutSubItem menu, ObservableList<OpenAIChat, Guid> collection)
    {
        menu.Items.Clear();

        if (collection == null || collection.Count == 0)
        {
            menu.Visibility = Visibility.Collapsed;
        }
        else
        {
            menu.Visibility = Visibility.Visible;

            foreach (var item in collection)
            {
                var chatMenuItem = new ToggleMenuFlyoutItem() { Text = item.Title };
                chatMenuItem.Click += ChatMenuItem_Click;
                SetChat(chatMenuItem, item);
                menu.Items.Add(chatMenuItem);
            }
        }
    }

    private static void ChatMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var chatMenuItem = (ToggleMenuFlyoutItem)sender;
        var chat = GetChat(chatMenuItem);

        chat.TryGetChatWindow(out var window);

        if (window.Visible)
        {
            window.Hide(!WindowManager.HasNotHiddenWindowsExcept(window));
        }
        else
        {
            window.Show();
        }
    }

    #endregion
}

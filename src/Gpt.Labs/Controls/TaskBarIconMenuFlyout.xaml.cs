using Gpt.Labs.Controls.DependencyExtensions;
using Gpt.Labs.Helpers;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels;
using Gpt.Labs.ViewModels.Collections;
using H.Hooks;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;

namespace Gpt.Labs.Controls;

public sealed partial class TaskBarIconMenuFlyout
{
    private readonly LowLevelKeyboardHook keyboardHook = new();

    public TaskBarIconMenuFlyout()
    {
        InitializeComponent();

        TaskBarMenuFlyout.SetChatsCollection(Chats, DataManager.Instance[OpenAIChatType.Chat]);

        var exitApplicationCommand = (XamlUICommand)App.Current.Resources["ExitApplicationCommand"];
        exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

        //keyboardHook.Up += KeyboardHook_Up;
        //keyboardHook.Down += KeyboardHook_Down;

        keyboardHook.Start();
    }

    private void KeyboardHook_Down(object sender, KeyboardEventArgs args)
    {
        Debug.WriteLine($"Warn: {nameof(keyboardHook.Down)}: {args}");
    }

    private void KeyboardHook_Up(object sender, KeyboardEventArgs args)
    {
        Debug.WriteLine($"Warn: {nameof(keyboardHook.Up)}: {args}");
    }

    private void ExitApplicationCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        App.TrayIcon?.Dispose();

        if (WindowManager.HasWindows)
        {
            WindowManager.CloseWindows();
        }
        else
        {
            Environment.Exit(0);
        }
    }

    public DataManager ChatsManager => DataManager.Instance;

    public ObservableList<OpenAIChat, Guid> ChatsCollection => DataManager.Instance[OpenAIChatType.Chat];

    private void MainWindow_Click(object sender, RoutedEventArgs e)
    {
        App.GetMainWindow(out var window);

        if (window.Visible)
        {
            window.Hide(!WindowManager.HasNotHiddenWindowsExcept(window));
        }
        else
        {
            window.Show();
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        App.TrayIcon?.Dispose();

        if (WindowManager.HasWindows)
        {
            WindowManager.CloseWindows();
        }
        else
        {
            Environment.Exit(0);
        }
    }
}

using Gpt.Labs.Helpers.Extensions;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace Gpt.Labs.Helpers
{
    public static class WindowManager
    {
        private static readonly Dictionary<string, MainWindow> windows = [];

        public static bool HandleClosedEvents { get; set; } = true;

        public static IEnumerable<MainWindow> Enumerate()
        {
            return windows.Values;
        }

        public static bool HasWindows => windows.Count > 0;

        public static bool HasNotHiddenWindowsExcept(MainWindow window)
        {
            foreach (var item in Enumerate())
            {
                if (item.WindowId != window.WindowId && !item.IsWindowHidden())
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGet(Func<string> createWindowId, Action<MainWindow> initializeContent, out MainWindow window)
        {
            var windowId = createWindowId();

            if (windows.ContainsKey(windowId))
            {
                window = windows[windowId];
                return false;
            }

            window = new MainWindow(windowId);

            window.SetExtendsContentIntoTitleBar();
            window.ApplyTheme();

            window.Closed += Window_Closed;

            windows[window.WindowId] = window;

            initializeContent(window);

            return true;
        }

        public static MainWindow Get(string windowId)
        {
            return !windows.ContainsKey(windowId) ? null : windows[windowId];
        }

        public static void CloseWindows()
        {
            HandleClosedEvents = false;

            foreach (var window in Enumerate())
            {
                window.Close();
            }

            windows.Clear();

            HandleClosedEvents = true;
        }

        private static void Window_Closed(object sender, WindowEventArgs args)
        {
            var window = (MainWindow)sender;

            if (HandleClosedEvents)
            {
                args.Handled = true;
                window.Hide(!HasNotHiddenWindowsExcept(window));
            }
        }
    }
}

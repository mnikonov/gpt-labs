using System;
using System.Collections.Generic;

namespace Gpt.Labs.Helpers
{
    public static class WindowManager
    {
        private static Dictionary<Guid, MainWindow> windows = new Dictionary<Guid, MainWindow>();

        public static IEnumerable<MainWindow> Enumerate()
        {
            return windows.Values;
        }

        public static MainWindow CreateWindow()
        {
            var window = new MainWindow();

            windows[window.WindowId] = window;

            return window;
        }

        public static MainWindow GetWindow(Guid windowId)
        {
            if (!windows.ContainsKey(windowId))
            {
                return null;
            }

            return windows[windowId];
        }

        public static void UnregisterWindow(Guid windowId)
        {
            if (windows.ContainsKey(windowId))
            {
                windows.Remove(windowId);
            }
        }
    }
}

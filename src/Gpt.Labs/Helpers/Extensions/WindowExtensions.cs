using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using WinRT;
using WinRT.Interop;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class AppWindowExtensions
    {
        #region Private Fields

        private static readonly Guid _dtm_iid = new Guid(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

        private static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();

        #endregion

        #region Public Methods

        public static AppWindow GetAppWindow(this Window window)
        {
            var windowHandle = window.GetHwndForWindow();
            return GetAppWindowFromWindowHandle(windowHandle);
        }

        public static double GetDpiScale(this Window window)
        {
            IntPtr hWnd = window.GetHwndForWindow();
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }

        public static void Resize(this Window window, int width, int height)
        {
            var appWindow = window.GetAppWindow();

            var winPosition = new SizeInt32(width, height);

            appWindow.Resize(winPosition);
        }

        public static DataTransferManager GetDataTransferManager(this Window window)
        {
            IntPtr result;
            result = DataTransferManagerInterop.GetForWindow(window.GetHwndForWindow(), _dtm_iid);
            DataTransferManager dataTransferManager = MarshalInterface<DataTransferManager>.FromAbi(result);
            return (dataTransferManager);
        }

        public static void ShowShareUI(this Window window)
        {
            DataTransferManagerInterop.ShowShareUIForWindow(window.GetHwndForWindow());
        }

        public static IntPtr GetHwndForWindow(this Window window)
        {
            return WindowNative.GetWindowHandle(window);
        }

        public static void SetExtendsContentIntoTitleBar(this Window window, bool extendsContentIntoTitleBar = true)
        {
            var appWindow = window.GetAppWindow();

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = extendsContentIntoTitleBar;
            }
        }

        public static void SetTitle(this Window window, string title)
        {
            var appWindow = window.GetAppWindow();

            appWindow.Title = title;
        }

        public static async Task OpenChatInNewWindows(this OpenAIChat chat)
        {
            var window = WindowManager.CreateWindow();

            var content = new SingleWindowPage();
            content.Title.Text = chat.Title;
            content.SetWindowId(window.WindowId);

            window.Content = content;

            window.SetExtendsContentIntoTitleBar();

            window.Resize((int)(600 * window.GetDpiScale()), window.AppWindow.Size.Height);

            window.ApplyTheme();

            window.Activate();

            await content.ExecuteOnLoaded(() =>
            {
                var query = new Query
                {
                    { "chat-id", chat.Id }
                };

                content.PageFrame.Navigate(typeof(SingleChatPage), query.ToString(), new DrillInNavigationTransitionInfo());
            });
        }

        #endregion

        #region Private Methods

        [DllImport("Shcore.dll", SetLastError = true)]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        private static AppWindow GetAppWindowFromWindowHandle(IntPtr windowHandle)
        {
            var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }

        [ComImport]
        [Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDataTransferManagerInterop
        {
            IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
            void ShowShareUIForWindow(IntPtr appWindow);
        }

        #endregion

        #region Enums

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        #endregion
    }
}

using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Gpt.Labs
{
    public abstract partial class BasePage : Page
    {
        #region Fields

        private string windowId;

        private Frame frame;

        #endregion

        #region Constructors

        public BasePage()
        {
            Unloaded += OnBasePageUnloaded;
        }

        #endregion

        #region Properties

        public SuspensionManager SuspensionManager { get; private set; }

        public bool HasFrame => frame != null;

        public MainWindow Window => WindowManager.Get(windowId);

        #endregion

        #region Public Methods

        public void SetWindowId(string windowId)
        {
            this.windowId = windowId;
        }

        public void RegisterFrame(Frame frame, string sessionStateKey, bool initSuspensionManager)
        {
            if (frame == null)
            {
                return;
            }

            this.frame = frame;

            if (initSuspensionManager)
            {
                SuspensionManager = new SuspensionManager();
                SuspensionManager.RegisterFrame(frame, sessionStateKey);
            }
        }

        public bool IsFrameHasContent()
        {
            return HasFrame && frame.Content != null;
        }

        public Type GetFrameContentType()
        {
            return frame?.Content?.GetType();
        }

        public bool CanGoBack()
        {
            if (!HasFrame)
            {
                return false;
            }

            var innerFrame = GetPageInnerFrame();
            return frame.CanGoBack || (innerFrame != null && innerFrame.CanGoBack);
        }

        public bool CanGoForward()
        {
            if (!HasFrame)
            {
                return false;
            }

            var innerFrame = GetPageInnerFrame();
            return frame.CanGoForward || (innerFrame != null && innerFrame.CanGoForward);
        }

        public void GoBack()
        {
            if (!HasFrame)
            {
                return;
            }

            var innerFrame = GetPageInnerFrame();

            if (innerFrame != null && innerFrame.CanGoBack)
            {
                innerFrame.GoBack();
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
            }
        }

        public void GoForward()
        {
            var innerFrame = GetPageInnerFrame();

            if (innerFrame != null && innerFrame.CanGoForward)
            {
                innerFrame.GoForward();
                return;
            }

            if (frame.CanGoForward)
            {
                frame.GoForward();
            }
        }

        public bool Navigate(Type page)
        {
            return Navigate(page, []);
        }

        public bool Navigate(Type page, Query parameter)
        {
            return Navigate(page, parameter, new DrillInNavigationTransitionInfo());
        }

        public bool Navigate(Type page, Query parameter, NavigationTransitionInfo infoOverride)
        {
            if (page == null)
            {
                throw new ArgumentNullException("The page to navigate should be specified.");
            }

            var queryParam = parameter?.ToString();

            return frame.Navigate(page, queryParam, infoOverride);
        }

        public virtual void UpdateBackState()
        {
        }

        public virtual void UpdateTitleBarContent(UIElement content)
        {
        }

        #endregion

        #region Private Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var query = Query.Parse(e.Parameter);

            if (query.TryGetValue("WindowId", out string windowId))
            {
                this.windowId = windowId;
            }
        }

        private void OnBasePageUnloaded(object sender, RoutedEventArgs e)
        {
            if (Window == null)
            {
                return;
            }

            var popups = VisualTreeHelper.GetOpenPopups(Window);
            foreach (var popup in popups)
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                }
            }
        }

        private Frame GetPageInnerFrame()
        {
            return (frame?.Content as StatePage)?.GetInnerFrame();
        }

        #endregion
    }
}

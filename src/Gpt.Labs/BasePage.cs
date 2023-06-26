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

        private Guid windowId = Guid.Empty;

        private Frame frame;

        #endregion

        #region Constructors

        public BasePage()
        {
            this.Unloaded += this.OnBasePageUnloaded;
        }

        #endregion

        #region Properties

        public SuspensionManager SuspensionManager { get; private set; }

        public bool HasFrame => this.frame != null;

        public MainWindow Window => WindowManager.GetWindow(this.windowId);

        #endregion

        #region Public Methods

        public void SetWindowId(Guid windowId)
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
                this.SuspensionManager = new SuspensionManager();
                this.SuspensionManager.RegisterFrame(frame, sessionStateKey);
            }
        }

        public bool IsFrameHasContent()
        {
            return this.HasFrame && this.frame.Content != null;
        }

        public Type GetFrameContentType()
        {
            return frame?.Content?.GetType();
        }

        public bool CanGoBack()
        {
            if (!this.HasFrame)
            {
                return false;
            }

            var innerFrame = this.GetPageInnerFrame();
            return this.frame.CanGoBack || (innerFrame != null && innerFrame.CanGoBack);
        }

        public bool CanGoForward()
        {
            if (!this.HasFrame)
            {
                return false;
            }

            var innerFrame = this.GetPageInnerFrame();
            return this.frame.CanGoForward || (innerFrame != null && innerFrame.CanGoForward);
        }

        public void GoBack()
        {
            if (!this.HasFrame)
            {
                return;
            }

            var innerFrame = this.GetPageInnerFrame();

            if (innerFrame != null && innerFrame.CanGoBack)
            {
                innerFrame.GoBack();
                return;
            }

            if (this.frame.CanGoBack)
            {
                this.frame.GoBack();
            }
        }

        public void GoForward()
        {
            var innerFrame = this.GetPageInnerFrame();

            if (innerFrame != null && innerFrame.CanGoForward)
            {
                innerFrame.GoForward();
                return;
            }

            if (this.frame.CanGoForward)
            {
                this.frame.GoForward();
            }
        }

        public bool Navigate(Type page)
        {
            return this.Navigate(page, new Query());
        }

        public bool Navigate(Type page, Query parameter)
        {
            return this.Navigate(page, parameter, new DrillInNavigationTransitionInfo());
        }

        public bool Navigate(Type page, Query parameter, NavigationTransitionInfo infoOverride)
        {
            if (page == null)
            {
                throw new ArgumentNullException("The page to navigate should be specified.");
            }

            var queryParam = parameter?.ToString();

            return this.frame.Navigate(page, queryParam, infoOverride);
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

            if (query.TryGetValue("WindowId", out Guid windowId))
            {
                this.windowId = windowId;
            }
        }

        private void OnBasePageUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.Window == null)
            {
                return;
            }

            var popups = VisualTreeHelper.GetOpenPopups(this.Window);
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
            return (this.frame?.Content as StatePage)?.GetInnerFrame();
        }

        #endregion
    }
}

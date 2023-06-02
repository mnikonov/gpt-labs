using Gpt.Labs.Common.Interfaces;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gpt.Labs.Helpers.Navigation
{
    /// <summary>
    /// NavigationHelper aids in navigation between pages. It manages
    /// the backstack and integrates SuspensionManager to handle process
    /// lifetime management and state management when navigating between pages.
    /// </summary>
    /// <example>
    /// To make use of NavigationHelper, follow these two steps or
    /// start with a BasicPage or any other Page item template other than BlankPage.
    ///
    /// 1) Create an instance of the NavigationHelper somewhere such as in the
    ///     constructor for the page and register a callback for the LoadState and
    ///     SaveState events.
    /// <code>
    ///     public MyPage()
    ///     {
    ///         this.InitializeComponent();
    ///         this.navigationHelper = new NavigationHelper(this);
    ///         this.navigationHelper.LoadState += navigationHelper_LoadState;
    ///         this.navigationHelper.SaveState += navigationHelper_SaveState;
    ///     }
    ///
    ///     private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    ///     { }
    ///     private void navigationHelper_SaveState(object sender, LoadStateEventArgs e)
    ///     { }
    /// </code>
    ///
    /// 2) Register the page to call into the NavigationManager whenever the page participates
    ///     in navigation by overriding the <see cref="Microsoft.UI.Xaml.Controls.Page.OnNavigatedTo"/>
    ///     and <see cref="Microsoft.UI.Xaml.Controls.Page.OnNavigatedFrom"/> events.
    /// <code>
    ///     protected override void OnNavigatedTo(NavigationEventArgs e)
    ///     {
    ///         navigationHelper.OnNavigatedTo(e);
    ///     }
    ///
    ///     protected override void OnNavigatedFrom(NavigationEventArgs e)
    ///     {
    ///         navigationHelper.OnNavigatedFrom(e);
    ///     }
    /// </code>
    /// </example>
    public class NavigationHelper
    {
        #region Fields

        private string pageKey;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationHelper"/> class.
        /// </summary>
        /// <param name="page">A reference to the current page used for navigation.
        /// This reference allows for frame manipulation.
        /// </param>
        public NavigationHelper(Page page)
        {
            this.Page = page;
        }

        #endregion

        #region Properties

        private Frame Frame => this.Page.Frame;

        private Page Page { get; }

        #endregion

        #region Public Methods

        public void SetPageKey()
        {
            this.pageKey = "Page-" + this.Frame.BackStackDepth;
        }

        /// <summary>
        /// Invoked when this page will no longer be displayed in a Frame.
        /// This method calls <see cref="SaveState"/>, where all page specific
        /// navigation and process lifetime management logic should be placed.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property provides the group to be displayed.</param>
        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = ShellPage.Current.SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new ViewModelState();

#if SHOW_DEBUG_NOTIFICATIONS
            var watch = Stopwatch.StartNew();
#endif
            var query = Query.Parse(e.Parameter);
            
            (this.Page as IPageStateStore)?.SaveState(
                e.SourcePageType,
                query,
                pageState,
                e.NavigationMode);
            frameState.PageState[this.pageKey] = pageState;

#if SHOW_DEBUG_NOTIFICATIONS
            watch.Stop();

            Notifications.ShowDebugInfo(
                $"{e.NavigationMode} navigation from {this.Page.GetType().Name} Page",
                $"Save State - {(int)watch.Elapsed.TotalMilliseconds}");
#endif
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// This method calls <see cref="LoadState"/>, where all page specific
        /// navigation and process lifetime management logic should be placed.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property provides the group to be displayed.</param>
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            var frameState = ShellPage.Current.SuspensionManager.SessionStateForFrame(this.Frame);
            this.pageKey = "Page-" + this.Frame.BackStackDepth;

#if SHOW_DEBUG_NOTIFICATIONS
            var watch = Stopwatch.StartNew();
#endif
            ViewModelState state;

            if (e.NavigationMode == NavigationMode.New)
            {
                var nextPageKey = this.pageKey;
                var nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.PageState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                state = new ViewModelState();
            }
            else
            {
                state = (ViewModelState)frameState.PageState[this.pageKey];
            }

            var eventProperties = new ViewModelState()
            {
                ["SourcePage"] = e.SourcePageType.Name,
                ["Query"] = e.Parameter != null ? e.Parameter.ToString() : string.Empty,
                ["NavigationMode"] = e.NavigationMode.ToString()
            };

            foreach (var stateItem in state)
            {
                eventProperties.Add(stateItem.Key, stateItem.Value);
            }

            "PageNavigation".LogEvent(eventProperties);
            
            (this.Page as IPageStateStore)?.LoadState(
                e.SourcePageType,
                Query.Parse(e.Parameter),
                state,
                e.NavigationMode);

            ShellPage.Current.UpdateBackState();

#if SHOW_DEBUG_NOTIFICATIONS
            watch.Stop();
            Notifications.ShowDebugInfo(
                $"{e.NavigationMode} navigation to {this.Page.GetType().Name} Page",
                $"Load State - {(int)watch.Elapsed.TotalMilliseconds}");
#endif
        }

        #endregion
    }
}

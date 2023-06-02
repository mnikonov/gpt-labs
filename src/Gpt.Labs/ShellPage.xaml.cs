using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs
{
    public sealed partial class ShellPage : Page
    {
        #region Fields

        public static readonly DependencyProperty IsProgressActiveProperty = DependencyProperty.Register(
            "IsProgressActive",
            typeof(bool),
            typeof(ShellPage),
            new PropertyMetadata(false, OnIsProgressActiveChanged));

        #endregion

        #region Constructors

        public ShellPage()
        {
            this.InitializeComponent();

            Current = this;

            this.SuspensionManager.RegisterFrame(this.ShellFrame, "ShellFrameState");

            this.ViewModel = new ShellViewModel();

            this.Unloaded += this.OnShellUnloaded;
            
            this.ShellFrame.Navigated += this.OnFrameNavigated;

            this.MainNavigationView.BackRequested += this.OnBackRequested;
        }

        #endregion

        #region Properties

        public static ShellPage Current { get; private set; }

        public ShellViewModel ViewModel { get; }

        public SuspensionManager SuspensionManager { get; } = new SuspensionManager();

        public bool IsProgressActive
        {
            get => (bool)this.GetValue(IsProgressActiveProperty);

            set => this.SetValue(IsProgressActiveProperty, value);
        }

        public ApplicationSettings Settings { get; } = ApplicationSettings.Instance;
        
        public Grid PageHeader
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool IsFrameHasContent()
        {
            return this.ShellFrame.Content != null;
        }

        public Type GetFrameContentType()
        {
            return ShellFrame.Content?.GetType();
        }

        public bool CanGoBack()
        {
            var innerFrame = this.GetPageInnerFrame();
            return this.ShellFrame.CanGoBack || (innerFrame != null && innerFrame.CanGoBack);
        }

        public bool CanGoForward()
        {
            var innerFrame = this.GetPageInnerFrame();
            return this.ShellFrame.CanGoForward || (innerFrame != null && innerFrame.CanGoForward);
        }

        public void GoBack()
        {
            var innerFrame = this.GetPageInnerFrame();

            if (innerFrame != null && innerFrame.CanGoBack)
            {
                innerFrame.GoBack();
                return;
            }

            if (this.ShellFrame.CanGoBack)
            {
                this.ShellFrame.GoBack();
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

            if (this.ShellFrame.CanGoForward)
            {
                this.ShellFrame.GoForward();
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

            return this.ShellFrame.Navigate(page, queryParam, infoOverride);
        }

        public void UpdateBackState()
        {
            this.MainNavigationView.IsBackEnabled = this.CanGoBack();
        }

        #endregion

        #region Private Methods

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var query = Query.Parse(e.Parameter);

            if (query.TryGetValue("IsTerminated", out bool value) && value)
            {
                await this.SuspensionManager.RestoreAsync();
            }

            if (!this.IsFrameHasContent())
            {
                //this.Navigate(typeof(TestPage));
                var chatQuery = new Query
                {
                    { "chat-type", (int)OpenAIChatType.Chat },
                    { "frame-uid", Guid.NewGuid() }
                };

                this.Navigate(typeof(ChatsPage), chatQuery);
            }
            else
            {
                this.MainNavigationView.IsBackEnabled = this.CanGoBack();
                this.MainNavigationView.IsPaneOpen = true;
                this.MainNavigationView.IsPaneOpen = false;
                this.ViewModel.ApplyMenuSelection(this.MainNavigationView, this.GetFrameContentType(), string.Empty);
            }

            if (query.TryGetValue("ProtocolActivatedUrl", out string activationUrl) && !string.IsNullOrEmpty(activationUrl))
            {
                var uri = new Uri(activationUrl);

                var navigationMap = new Dictionary<string, Type>();

                if (navigationMap.ContainsKey(uri.LocalPath))
                {
                    this.Navigate(
                        navigationMap[uri.LocalPath],
                        Query.Parse(uri.Query));
                }
            }
        }

        private static void OnIsProgressActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var shell = d as ShellPage;
            var newValue = (bool)e.NewValue;
            var oldValue = (bool)e.OldValue;

            if (shell != null && newValue != oldValue)
            {
                shell.MainNavigationView.IsEnabled = !newValue;

                shell.Progress.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OnShellUnloaded(object sender, RoutedEventArgs e)
        {
            var popups = VisualTreeHelper.GetOpenPopups(App.Window);
            foreach (var popup in popups)
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                }
            }
        }
        
        private void OnMainNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                this.Navigate(typeof(SettingsPage));
                return;
            }

            var tag = sender.MenuItems.OfType<NavigationViewItem>().First(menuItem => (string)menuItem.Content == (string)args.InvokedItem).Tag as string;

            var navigationParameters = tag.Split('?');

            Query query = null;

            if (navigationParameters.Length > 1)
            {
                query = Query.Parse($"?{navigationParameters[1]}");
            }

            var pageType = Type.GetType($"Gpt.Labs.{navigationParameters[0]}");

            if (pageType == typeof(ChatsPage))
            {
                query.Add("frame-uid", Guid.NewGuid());
            }

            //if (this.ShellFrame?.Content != null && this.ShellFrame.Content.GetType() == pageType)
            //{
            //    return;
            //}

            this.Navigate(pageType, query);
        }

        private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            // don't go back if the nav pane is overlayed
            if (this.MainNavigationView.IsPaneOpen && (this.MainNavigationView.DisplayMode == NavigationViewDisplayMode.Compact || this.MainNavigationView.DisplayMode == NavigationViewDisplayMode.Minimal))
            {
                return;
            }

            this.GoBack();
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            this.ViewModel.ApplyMenuSelection(this.MainNavigationView, e.SourcePageType, e.Parameter?.ToString());
        }

        private void OnMainNavigationViewLoaded(object sender, RoutedEventArgs e)
        {
            this.PageHeader = (Grid)this.MainNavigationView.GetDescendantOfType<Grid>("PageHeader");
        }

        private Frame GetPageInnerFrame()
        {
            return (this.ShellFrame.Content as BasePage)?.GetInnerFrame();
        }

        #endregion
    }
}

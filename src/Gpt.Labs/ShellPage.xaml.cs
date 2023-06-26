using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs
{
    public sealed partial class ShellPage : BasePage
    {
        #region Constructors

        public ShellPage()
        {
            this.InitializeComponent();

            this.RegisterFrame(this.ShellFrame, "ShellFrameState", true);

            this.ViewModel = new ShellViewModel();

            this.ShellFrame.Navigated += this.OnFrameNavigated;

            this.MainNavigationView.BackRequested += this.OnBackRequested;
        }

        #endregion

        #region Properties

        public ShellViewModel ViewModel { get; }

        public ApplicationSettings Settings { get; } = ApplicationSettings.Instance;
        
        public Grid PageHeader
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods
                
        public override void UpdateBackState()
        {
            this.MainNavigationView.IsBackEnabled = this.CanGoBack();
        }

        public override void UpdateTitleBarContent(UIElement content)
        {
            this.AppTitleBar.Content = content;
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

            await this.ExecuteOnLoaded(() =>
            {
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
            });
        }

        private void OnMainNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                this.Navigate(typeof(SettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
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

            this.Navigate(pageType, query, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } );
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

        #endregion
    }
}

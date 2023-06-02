using Gpt.Labs.Common.Interfaces;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using System;

namespace Gpt.Labs
{
    public abstract partial class BasePage : Page, IPageStateStore
    {
        #region Public Static Dependency Propertys

        public static readonly DependencyProperty AppTitleBarContentProperty = DependencyProperty.Register(
            "AppTitleBarContent",
            typeof(UIElement),
            typeof(BasePage),
            new PropertyMetadata(null, AppTitleBarContentChangedCallback));

        #endregion

        #region Public Constructors

        protected BasePage()
        {
            this.NavigationHelper = new NavigationHelper(this);
        }

        #endregion

        #region Properties

        public UIElement AppTitleBarContent
        {
            get => (UIElement)this.GetValue(AppTitleBarContentProperty);
            set => this.SetValue(AppTitleBarContentProperty, value);
        }

        public ShellPage Shell => ShellPage.Current;
                
        public NavigationHelper NavigationHelper { get; }

        #endregion

        #region Public Methods

        public static T GetCurrentPage<T>()
            where T : class
        {
            return ShellPage.Current.ShellFrame.Content as T;
        }

        public static BasePage GetCurrent()
        {
            return ShellPage.Current.ShellFrame.Content as BasePage;
        }

        public virtual Frame GetInnerFrame()
        {
            return null;
        }

        public virtual void LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
        }

        public virtual void SaveState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
        } 

        #endregion

        #region Protected Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.NavigationHelper.OnNavigatedFrom(e);
        }
        
        #endregion

        #region Private Methods

        private static void AppTitleBarContentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is UIElement element)
            {
                ShellPage.Current.AppTitleBar.Content = element;
            }
        }

        #endregion
    }
}

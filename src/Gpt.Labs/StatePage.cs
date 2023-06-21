using Gpt.Labs.Common.Interfaces;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using System;
using Gpt.Labs.Helpers;

namespace Gpt.Labs
{
    public abstract partial class StatePage : Page, IPageStateStore
    {
        #region Public Static Dependency Propertys

        public static readonly DependencyProperty AppTitleBarContentProperty = DependencyProperty.Register(
            "AppTitleBarContent",
            typeof(UIElement),
            typeof(StatePage),
            new PropertyMetadata(null, AppTitleBarContentChangedCallback));

        #endregion

        #region Public Constructors

        protected StatePage()
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

        public BasePage RootPage => this.Frame.GetParent<BasePage>();

        public MainWindow Window => RootPage?.Window;
                
        public NavigationHelper NavigationHelper { get; }

        #endregion

        #region Public Methods

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
            base.OnNavigatedTo(e);

            this.NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.NavigationHelper.OnNavigatedFrom(e);

            base.OnNavigatedFrom(e);
        }
        
        #endregion

        #region Private Methods

        private static void AppTitleBarContentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatePage page && e.NewValue is UIElement element)
            {
                page.RootPage?.UpdateTitleBarContent(element);
            }
        }

        #endregion
    }
}

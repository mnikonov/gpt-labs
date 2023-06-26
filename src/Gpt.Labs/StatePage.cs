using Gpt.Labs.Common.Interfaces;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using System;
using Gpt.Labs.Helpers;
using Gpt.Labs.Controls.Extensions;
using System.Threading.Tasks;

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

        public virtual Task LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode)
        {
            return Task.CompletedTask;  
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {            
            base.OnNavigatedTo(e);

            await this.NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.NavigationHelper.OnNavigatedFrom(e);

            base.OnNavigatedFrom(e);
        }
        
        #endregion

        #region Private Methods

        private static async void AppTitleBarContentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatePage page && e.NewValue is UIElement element)
            {
                await page.ExecuteOnLoaded(() =>
                {
                    page.RootPage.UpdateTitleBarContent(element);
                });
            }
        }

        #endregion
    }
}

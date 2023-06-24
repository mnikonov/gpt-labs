using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models.Base;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace Gpt.Labs.ViewModels.Base
{
    public abstract class ViewModelBase : ObservableObject
    {
        #region Fields

        Func<BasePage> getBasePage;

        #endregion

        #region Constructors

        public ViewModelBase(Func<BasePage> getBasePage)
        {
            this.getBasePage = getBasePage;
        }

        #endregion

        #region Properties

        public BasePage BasePage => this.getBasePage();

        public MainWindow Window => this.BasePage?.Window;

        public DispatcherQueue DispatcherQueue => this.Window?.DispatcherQueue;

        #endregion

        #region Public Methods

        public void Navigate<TPageType>(Query param, NavigationTransitionInfo infoOverride = null)
        {
            this.BasePage?.Navigate(typeof(TPageType), param, infoOverride);
        }

        #endregion
    }
}

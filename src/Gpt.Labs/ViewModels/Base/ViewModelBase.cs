using Gpt.Labs.Controls.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models.Base;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.Base
{
    public abstract class ViewModelBase : ObservableObject
    {
        #region Properties

        public ShellPage Shell => ShellPage.Current;

        public DispatcherQueue DispatcherQueue => App.Window.DispatcherQueue;

        #endregion

        #region Public Methods

        public void Navigate<TPageType>(Query param, NavigationTransitionInfo infoOverride = null)
        {
            this.Shell.Navigate(typeof(TPageType), param, infoOverride);
        }

        public Task BlockUiAndExecute(Func<CancellationToken, Task> action, bool showProgress = true, CancellationToken cancellationToken = default)
        {
            return this.Shell.BlockUiAndExecute(action, showProgress, cancellationToken);
        }

        #endregion

        #region Private Methods

        protected T GetCurrentPage<T>()
            where T : class
        {
            return ShellPage.Current.ShellFrame.Content as T;
        }

        #endregion
    }
}

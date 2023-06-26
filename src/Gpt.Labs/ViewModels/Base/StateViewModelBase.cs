using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels.Interfaces;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.Base
{
    public abstract class StateViewModelBase : ViewModelBase, IViewModelStateStore, IDisposable
    {
        #region Fields

        protected string stateExtension = string.Empty;

        private bool disposed;

        #endregion

        #region Public Constructors

        protected StateViewModelBase(Func<BasePage> getBasePage)
            : base(getBasePage)
        {
        }

        protected StateViewModelBase(Func<BasePage> getBasePage, string stateExtension)
            : this(getBasePage)
        {
            this.stateExtension = stateExtension;
        }

        ~StateViewModelBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion

        #region IStateStore Methods

        public abstract Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode);

        public abstract void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode);

        #endregion

        #region Private Methods

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
        }

        #endregion
    }
}

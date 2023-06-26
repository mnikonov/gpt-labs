using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.Common.Interfaces
{
    public interface IPageStateStore
    {
        #region Public Methods

        Task LoadState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode);

        void SaveState(
            Type destinationPageType,
            Query parameters,
            ViewModelState state,
            NavigationMode mode);

        #endregion
    }
}

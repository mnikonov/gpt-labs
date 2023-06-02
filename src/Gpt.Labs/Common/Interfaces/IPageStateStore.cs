using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Gpt.Labs.Common.Interfaces
{
    public interface IPageStateStore
    {
        #region Public Methods

        void LoadState(
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

using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.Interfaces
{
    public interface IViewModelStateStore
    {
        #region Public Methods and Operators

        Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode);

        void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode);

        #endregion
    }
}

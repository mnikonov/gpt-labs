using Gpt.Labs.Models.Base;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace Gpt.Labs.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        #region Fields

        private NavigationViewItem selected;

        #endregion

        #region Constructors

        public ShellViewModel()
        {
        }

        #endregion

        #region Properties

        public NavigationViewItem Selected
        {
            get => this.selected;
            set => this.Set(ref this.selected, value);
        }

        #endregion

        #region Public Methods

        public void ApplyMenuSelection(NavigationView navigationView, Type pageType, string parameter)
        {
            if (pageType == null)
            {
                return;
            }

            if (pageType == typeof(SettingsPage))
            {
                this.Selected = navigationView.SettingsItem as NavigationViewItem;
                return;
            }

            var tag = pageType.Name + parameter;

            var navigationItem = navigationView.MenuItems.OfType<NavigationViewItem>()
                .FirstOrDefault(menuItem => tag.Contains(menuItem.Tag.ToString()));

            if (navigationItem != null)
            {
                this.Selected = navigationItem;
            }
        }

        #endregion
    }
}

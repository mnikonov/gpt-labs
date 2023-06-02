namespace Gpt.Labs.Helpers.Converters
{
    #region Usings

    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Data;

    #endregion

    public class ListViewSelectionModeConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (bool)value;
            var defaultSelection = (parameter as string) == "None" ? ListViewSelectionMode.None : ListViewSelectionMode.Single;

            return val ? ListViewSelectionMode.Multiple : defaultSelection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
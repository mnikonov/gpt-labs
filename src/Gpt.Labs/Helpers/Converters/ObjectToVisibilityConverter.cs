using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    public class ObjectToVisibilityConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visibleIfNull = (parameter as string) == "=";

            if (visibleIfNull)
            {
                return value == null ? Visibility.Visible : Visibility.Collapsed;
            }

            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

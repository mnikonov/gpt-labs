namespace Gpt.Labs.Helpers.Converters
{
    using System;

    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;

    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var param = parameter as string;
            var isVisible = value as bool?;

            if (isVisible == null)
            {
                return Visibility.Collapsed;
            }

            if (param == "Invert")
            {
                isVisible = !isVisible;
            }

            return isVisible.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

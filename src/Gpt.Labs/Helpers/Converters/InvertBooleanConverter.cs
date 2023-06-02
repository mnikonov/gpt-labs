namespace Gpt.Labs.Helpers.Converters
{
    #region Usings

    using System;

    using Microsoft.UI.Xaml.Data;

    #endregion

    public class InvertBooleanConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool toCovert ? !(bool?)toCovert : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
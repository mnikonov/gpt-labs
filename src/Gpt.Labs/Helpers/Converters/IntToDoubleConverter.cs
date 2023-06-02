using Microsoft.UI.Xaml.Data;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    internal class IntToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return double.NaN;
            }

            return (double)(int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var val = (double)value;

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>) && double.IsNaN(val))
            {
                return null;
            }

            return (int)val;
        }
    }
}

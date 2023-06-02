using Microsoft.UI.Xaml.Data;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var format = parameter as string;

        if (value == null || format == null)
        {
            return null;
        }

        return string.Format(format, value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
}

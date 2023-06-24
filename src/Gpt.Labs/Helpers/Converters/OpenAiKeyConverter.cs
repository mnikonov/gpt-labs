using Microsoft.UI.Xaml.Data;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    public class OpenAiKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = value as string;

            if (string.IsNullOrEmpty(val))
            {
                return App.ResourceLoader.GetString("NotSpecified");
            }

            var keyParts = val.Split('-');

            return $"{keyParts[0]} - {new string('⚹', keyParts[1].Length - 3)}{keyParts[1].Substring(keyParts[1].Length - 3, 3)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

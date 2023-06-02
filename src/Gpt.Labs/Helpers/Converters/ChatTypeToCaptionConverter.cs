using Gpt.Labs.Models.Enums;
using Microsoft.UI.Xaml.Data;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    public class ChatTypeToCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (OpenAIChatType)value;

            switch (val)
            {
                case OpenAIChatType.Chat:
                    return App.ResourceLoader.GetString("ChatCaption");

                case OpenAIChatType.Image:
                    return App.ResourceLoader.GetString("ImageCaption");
            }

            return null;
        }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
}

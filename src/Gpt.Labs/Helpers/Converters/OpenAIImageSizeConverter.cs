using Gpt.Labs.Models.Enums;
using Microsoft.UI.Xaml.Data;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    public class OpenAIImageSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var size = (OpenAIImageSize)value;

            switch (size)
            {
                case OpenAIImageSize.Small:
                    return "256x256";
                case OpenAIImageSize.Medium:
                    return "512x512";
                case OpenAIImageSize.Large:
                    return "1024x1024";
                default:
                    return string.Empty;
            } 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var size = (string)value;

            switch (size)
            {
                case "256x256":
                    return OpenAIImageSize.Small;
                case "512x512":
                    return OpenAIImageSize.Medium;
                case "1024x1024":
                default:
                    return OpenAIImageSize.Large;
            }
        }
    }
}

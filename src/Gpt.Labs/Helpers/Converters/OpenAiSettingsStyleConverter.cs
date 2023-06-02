using Microsoft.UI.Xaml.Data;
using System;
using Gpt.Labs.Models.Enums;
using Microsoft.UI.Xaml;

namespace Gpt.Labs.Helpers.Converters
{
    internal class OpenAiSettingsStyleConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (OpenAIChatType)value;

            switch (val)
            {
                case OpenAIChatType.Chat:
                    return (Style)App.Current.Resources["OpenAiChatSettings"];

                 case OpenAIChatType.Image:
                    return (Style)App.Current.Resources["OpenAiImageSettings"];
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

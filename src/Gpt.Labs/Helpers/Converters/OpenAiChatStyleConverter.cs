using Microsoft.UI.Xaml.Data;
using System;
using Gpt.Labs.Models.Enums;
using Microsoft.UI.Xaml;
namespace Gpt.Labs.Helpers.Converters
{
    internal class OpenAiChatStyleConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (OpenAIChatType)value;

            switch (val)
            {
                case OpenAIChatType.Chat:
                    return (Style)App.Current.Resources["ChatStyle"];
                case OpenAIChatType.Image:
                    return (Style)App.Current.Resources["ImageChatStyle"];
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

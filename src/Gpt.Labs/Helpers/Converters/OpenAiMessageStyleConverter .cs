using Microsoft.UI.Xaml.Data;
using System;
using Gpt.Labs.Models.Enums;
using Microsoft.UI.Xaml;
namespace Gpt.Labs.Helpers.Converters
{
    internal class OpenAiMessageStyleConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (OpenAIRole)value;

            switch (val)
            {
                 case OpenAIRole.User:
                        return (Style)App.Current.Resources["UserMessageStyle"];
                    case OpenAIRole.Assistant:
                        return (Style)App.Current.Resources["AssistantMessageStyle"];
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

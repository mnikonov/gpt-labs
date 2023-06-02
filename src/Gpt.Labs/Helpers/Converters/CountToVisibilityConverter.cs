﻿using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;

namespace Gpt.Labs.Helpers.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var operation = '=';
            var compareTo = 0;

            if (!string.IsNullOrEmpty(parameter as string))
            {
                var parts = ((string)parameter).ToCharArray();

                operation = parts[0];

                if (parts.Length > 1)
                {
                    compareTo = int.Parse(parts[1].ToString());
                }
            }

            if (value is int count && ((operation == '=' && count == compareTo) || (operation == '>' && count > compareTo) || (operation == '<' && count < compareTo)))
            {
                return Visibility.Visible;
            }

            if (value is string text && ((operation == '=' && string.IsNullOrEmpty(text)) || (operation == '>' && text.Length > compareTo) || (operation == '<' && text.Length < compareTo)))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

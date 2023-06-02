using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using System.Linq;

namespace Gpt.Labs.Helpers.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(parameter is string param))
            {
                return DependencyProperty.UnsetValue;
            }

            if (value is Enum && Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            var matchValues = param.Split(',');

            return matchValues.Any(p => Enum.Parse(value.GetType(), p.Trim()).Equals(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var param = parameter as string;
            var isSelected = value as bool?;

            if (!isSelected.HasValue || !isSelected.Value || string.IsNullOrEmpty(param))
            {
                return 0;
            }

            return Enum.Parse(targetType, param);
        }

        #endregion
    }
}

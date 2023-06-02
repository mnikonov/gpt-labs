using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Controls.DependencyExtensions
{
    public class NumberBoxExtensions : DependencyObject
    {
        #region Fields

        public static readonly DependencyProperty HandleIntegerNumberProperty = DependencyProperty.Register(
            "HandleIntegerNumber",
            typeof(bool),
            typeof(DependencyObject),
            new PropertyMetadata(false, OnApplyIntegerNumberHandler));

        #endregion

        #region Public Methods

        public static bool GetHandleIntegerNumber(DependencyObject element)
        {
            return (bool)element.GetValue(HandleIntegerNumberProperty);
        }

        public static void SetHandleIntegerNumber(DependencyObject element, bool value)
        {
            element.SetValue(HandleIntegerNumberProperty, value);
        }

        #endregion

        #region Private Methods

        private static void OnApplyIntegerNumberHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numberBox = (NumberBox)d;
            var val = (bool)e.NewValue;

            if (val)
            {
                numberBox.ValueChanged += (sender, args) =>
                {
                    if (!double.IsNaN(args.NewValue) && args.NewValue % 1 != 0)
                    {
                        sender.Value = (int)args.NewValue;
                    }
                };
            }
        }

        #endregion
    }
}

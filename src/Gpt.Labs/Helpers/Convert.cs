using Microsoft.UI.Xaml.Markup;
using Windows.UI;

namespace Gpt.Labs.Helpers
{
    public static class Convert
    {
        public static Color ToColor(this string color)
        {
            return (Color)XamlBindingHelper.ConvertValue(typeof(Color), color);
        }
    }
}

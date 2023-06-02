using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class ListViewExtensions
    {
        public static int GetIndexFromPoint(this ListView listView, Point point)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                var container = listView.ContainerFromIndex(i) as ListViewItem;
                if (container != null && container.RenderTransform != null && container.RenderTransform is TranslateTransform transform)
                {
                    var margin = container.Margin;
                    var bounds = new Rect(margin.Left - transform.X, margin.Top - transform.Y, container.ActualWidth, container.ActualHeight);

                    if (bounds.Contains(point))
                    {
                        return i;
                    }
                }
            }

            return -1;

        }
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Gpt.Labs.Helpers
{
    public static class UIHelper
    {
        #region Public Methods

        public static IEnumerable<DependencyObject> GetDescendantsOfType<T>(this DependencyObject start)
        {
            for (var i = 0; i < start.GetChildrenCount(); i++)
            {
                var child = start.GetChild(i);

                if (child is T)
                {
                    yield return child;
                }

                foreach (var item in GetDescendantsOfType<T>(child))
                {
                    yield return item;
                }
            }
        }

        public static DependencyObject GetChild(this DependencyObject element, int i)
        {
            return VisualTreeHelper.GetChild(element, i);
        }

        public static int GetChildrenCount(this DependencyObject element)
        {
            return VisualTreeHelper.GetChildrenCount(element);
        }

        public static IEnumerable<FrameworkElement> GetDescendantsOfType<T>(this DependencyObject start, string name)
        {
            for (var i = 0; i < start.GetChildrenCount(); i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);

                if (child is T && child is FrameworkElement element && !string.IsNullOrEmpty(element.Name) && element.Name.Equals(name))
                {
                    yield return element;
                }

                foreach (var item in GetDescendantsOfType<T>(child, name))
                {
                    yield return item;
                }
            }
        }

        public static DependencyObject GetDescendantOfType<T>(this DependencyObject start)
        {
            return start.GetDescendantsOfType<T>().FirstOrDefault();
        }

        public static FrameworkElement GetDescendantOfType<T>(this DependencyObject start, string name)
        {
            return start.GetDescendantsOfType<T>(name).FirstOrDefault();
        }

        public static T GetParent<T>(this DependencyObject element, int maxDepth = -1, int circle = 0)
            where T : DependencyObject
        {            
            if (element == null)
            {
                return default;
            }

            if (element is T typedElement)
            {
                return typedElement;
            }

            if (maxDepth != -1 && maxDepth <= circle)
            {
                return default;
            }

            var parent = (element as FrameworkElement)?.Parent;

            if (parent == null)
            {
                parent = VisualTreeHelper.GetParent(element);
            }

            ++circle;

            return parent.GetParent<T>(maxDepth, circle);
        }

        public static bool HasParent(this DependencyObject element)
        {
            if (element == null)
            {
                return false;
            }

            var parent = (element as FrameworkElement)?.Parent;

            if (parent == null)
            {
                parent = VisualTreeHelper.GetParent(element);
            }

            return parent != null;
        }

        public static bool IsVisibleToUser(this FrameworkElement element, ItemsControl container)
        {
            if (element == null || container == null)
            {
                return false;
            }

            var elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.DesiredSize.Width, element.DesiredSize.Height));
            var containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return elementBounds.Top < containerBounds.Bottom && elementBounds.Bottom > containerBounds.Top;
        }

        #endregion
    }
}

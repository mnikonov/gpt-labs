using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Microsoft.UI.Windowing;
using Gpt.Labs.Helpers.Extensions;
using System.Runtime.InteropServices;

namespace Gpt.Labs.Controls
{
    public partial class PageHeader : ContentControl
    {
        #region Fields

        private ContentPresenter container;

        private readonly AppWindowTitleBar titleBar;

        #endregion

        #region Constructors

        public PageHeader()
        {
            DefaultStyleKey = typeof(PageHeader);

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                titleBar = App.Window.GetAppWindow().TitleBar;
            }
        }

        #endregion

        #region Methods

        protected override void OnApplyTemplate()
        {
            container = (ContentPresenter)GetTemplateChild("HeaderContentPresenter");

            if (titleBar != null)
            {
                container.SizeChanged -= OnContainerSizeChanged;
                container.SizeChanged += OnContainerSizeChanged;
                container.LayoutUpdated -= OnContainerLayoutUpdated;
                container.LayoutUpdated += OnContainerLayoutUpdated;

                this.Unloaded -= OnUnloaded;
                this.Unloaded += OnUnloaded;
            }

            base.OnApplyTemplate();

            if (titleBar != null)
            {
                SetDragRectangles();
            }
        }

        #endregion

        private void OnContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width == e.NewSize.Width)
            {
                return;
            }

            SetDragRectangles();
        }

        private void OnContainerLayoutUpdated(object sender, object e)
        {
            SetDragRectangles();
        }

        private void SetDragRectangles()
        {
            if (container == null || container.Height == 0)
            {
                return;
            }

            var scale = App.Window.GetDpiScale();

    #if DEBUG
            var y = (int)(22 * scale);
            var height = (int)(container.ActualHeight * scale) - y;
    #else
            var y = 0;
            var height = (int)(this.container.ActualHeight * scale);
    #endif

            if (height < 0)
            {

            }

            container.Margin = new Thickness(0, 0, titleBar.RightInset / scale, 0);

            try
            {
                var containerPosition = container.TransformToVisual(App.Window.Content).TransformPoint(new Point(0, 0));

                var x = containerPosition.X;
                var width = 0d;

                var rects = new List<RectInt32>();

                var controls = (container.Content as Grid)?.Children?.OfType<Control>();

                if (controls != null)
                {
                    foreach (var control in controls)
                    {
                        var controlPosition = control.TransformToVisual(App.Window.Content).TransformPoint(new Point(0, 0));

                        if (controlPosition.X > x)
                        {
                            width = controlPosition.X - x;

                            if (width > 0)
                            {
                                rects.Add(new RectInt32((int)(x * scale), y, (int)(width * scale), height));
                            }

                            x = (int)(controlPosition.X + control.ActualWidth);
                        }
                    }
                }

                width = (int)(container.ActualWidth + containerPosition.X - x + titleBar.RightInset);

                if (width > 0)
                {
                    rects.Add(new RectInt32((int)(x * scale), y, (int)(width * scale), height));
                }

                titleBar.SetDragRectangles(rects.ToArray());
            }
            catch (COMException)
            {
                // Exception handling not necessary in this case
            }
            
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (container != null)
            {
                container.SizeChanged -= OnContainerSizeChanged;
                container.LayoutUpdated -= OnContainerLayoutUpdated;
            }

            this.Unloaded -= OnUnloaded;
        }
    }
}

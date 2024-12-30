using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Graphics;

namespace Gpt.Labs.Controls;

public partial class PageHeader : ContentControl
{
    #region Fields

    private ContentPresenter container;

    private Window window;

    private AppWindowTitleBar titleBar;

    #endregion

    #region Constructors

    public PageHeader()
    {
        DefaultStyleKey = typeof(PageHeader);
    }

    #endregion

    #region Methods

    protected override void OnApplyTemplate()
    {
        window = this.GetParent<BasePage>().Window;

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            titleBar = window.GetAppWindow().TitleBar;
        }

        container = (ContentPresenter)GetTemplateChild("HeaderContentPresenter");

        if (titleBar != null)
        {
            container.SizeChanged -= OnContainerSizeChanged;
            container.SizeChanged += OnContainerSizeChanged;
            container.LayoutUpdated -= OnContainerLayoutUpdated;
            container.LayoutUpdated += OnContainerLayoutUpdated;

            Unloaded -= OnUnloaded;
            Unloaded += OnUnloaded;
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

        var scale = window.GetDpiScale();

        var y = 0;
        var height = (int)(container.ActualHeight * scale);

        container.Margin = new Thickness(0, 0, titleBar.RightInset / scale, 0);

        try
        {
            var containerPosition = container.TransformToVisual(window.Content).TransformPoint(new Point(0, 0));

            var x = containerPosition.X;
            var width = 0d;

            var rects = new List<RectInt32>();

            var controls = (container.Content as Grid)?.Children?.OfType<Control>();

            if (controls != null)
            {
                foreach (var control in controls)
                {
                    var controlPosition = control.TransformToVisual(window.Content).TransformPoint(new Point(0, 0));

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

        Unloaded -= OnUnloaded;
    }
}

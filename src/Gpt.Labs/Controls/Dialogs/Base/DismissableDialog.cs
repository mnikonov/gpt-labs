using Gpt.Labs.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Gpt.Labs.Controls.Dialogs.Base
{
    public partial class DismissableDialog : ContentDialogBase
    {
        #region Fields

        public static readonly DependencyProperty CanAutoCloseProperty = DependencyProperty.Register(
            "CanAutoClose",
            typeof(bool),
            typeof(DismissableDialog),
            new PropertyMetadata(false, CanAutoClosePropertyChangedCallback));

        private Rectangle lockRectangle;

        #endregion

        #region Constructors

        public DismissableDialog(Window window)
            : base(window)
        {
        }

        #endregion

        #region Properties

        public bool CanAutoClose
        {
            get => (bool)this.GetValue(CanAutoCloseProperty);

            set => this.SetValue(CanAutoCloseProperty, value);
        }

        #endregion

        #region  Private Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var popups = VisualTreeHelper.GetOpenPopups(this.window);
            foreach (var popup in popups)
            {
                var child = popup.Child as Rectangle;
                if (child != null)
                {
                    this.lockRectangle = child;
                    this.AttachDismissEvent();
                }
            }
        }

        private static void CanAutoClosePropertyChangedCallback(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            (d as DismissableDialog)?.AttachDismissEvent();
        }

        private void AttachDismissEvent()
        {
            if (this.lockRectangle != null)
            {
                this.lockRectangle.Tapped -= this.OnLockRectangleTapped;

                if (this.CanAutoClose)
                {
                    this.lockRectangle.Tapped += this.OnLockRectangleTapped;
                }
            }
        }

        private void OnLockRectangleTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            this.Hide();
            this.lockRectangle.Tapped -= this.OnLockRectangleTapped;
        }

        #endregion
    }
}

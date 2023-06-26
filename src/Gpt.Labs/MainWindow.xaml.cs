using ColorCode.Common;
using Gpt.Labs.Helpers;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using Windows.UI.ViewManagement;

namespace Gpt.Labs
{
    public sealed partial class MainWindow : Window
    {
        #region Fields

        private UISettings uISettings;

        #endregion

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            this.uISettings = new UISettings();
            this.uISettings.ColorValuesChanged += OnUISettingsColorValuesChanged;

            this.Closed += OnMainWindowClosed;
        }

        #endregion

        #region Properties

        public Guid WindowId { get; } = Guid.NewGuid();

        #endregion

        #region Private Methods

        private void OnMainWindowClosed(object sender, WindowEventArgs args)
        {
            this.Closed -= OnMainWindowClosed;
            this.uISettings.ColorValuesChanged -= OnUISettingsColorValuesChanged;

            WindowManager.UnregisterWindow(this.WindowId);
        }

        private void OnUISettingsColorValuesChanged(UISettings sender, object args)
        {
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
		        () =>
		            {
                        if (ApplicationSettings.Instance.AppTheme == ElementTheme.Default && this.Content != null)
                        {
                            this.ApplyTheme();
                        }
		            });
        }

        #endregion
    }
}

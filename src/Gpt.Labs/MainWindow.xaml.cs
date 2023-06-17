using Gpt.Labs.Helpers;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace Gpt.Labs
{
    public sealed partial class MainWindow : Window
    {
        private UISettings uISettings;

        public MainWindow()
        {
            this.InitializeComponent();

            this.uISettings = new UISettings();
            this.uISettings.ColorValuesChanged += OnUISettingsColorValuesChanged;
        }

        private void OnUISettingsColorValuesChanged(UISettings sender, object args)
        {
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
		        () =>
		            {
                        if (ApplicationSettings.Instance.AppTheme == ElementTheme.Default)
                        {
                            SystemThemeHelper.ApplyTheme((FrameworkElement)App.Window.Content);
                        }
		            });
        }
    }
}

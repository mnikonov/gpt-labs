using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;

#if !DEBUG

using System.Globalization;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

#endif

namespace Gpt.Labs
{
    public partial class App : Application
    {
        #region Constructors

        public App()
        {
            this.InitializeComponent();

#if !DEBUG
            AppCenter.Configure("{APP_CENTER_SECRET}");
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
            
            if (AppCenter.Configured)
            {
                AppCenter.Start(typeof(Analytics));
                AppCenter.Start(typeof(Crashes));
            }
#endif

            this.UnhandledException += this.AppUnhandledException;
        }

#endregion

        #region Properties

        public static ResourceLoader ResourceLoader { get; } = new ();

        #endregion

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.ActivateAsync(args.UWPLaunchActivatedEventArgs);
        }

        #region Private Methods

        private void ActivateAsync(IActivatedEventArgs args)
        {
            var window = WindowManager.CreateWindow();

            if (!(window.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += this.OnNavigationFailed;

                window.Content = rootFrame;
            }

            window.SetTitle(ResourceLoader.GetString("AppDisplayName"));
            window.SetExtendsContentIntoTitleBar();
            window.ApplyTheme();

            if (rootFrame.Content == null)
            {
                var query = new Query
                {
                    ["IsTerminated"] = args.PreviousExecutionState == ApplicationExecutionState.Terminated,
                    ["WindowId"] = window.WindowId
                };

                if (args is ProtocolActivatedEventArgs protocolArgs && protocolArgs.Uri != null)
                {
                    query["ProtocolActivatedUrl"] = protocolArgs.Uri.AbsoluteUri;
                }

                rootFrame.Navigate(typeof(ActivationPage), query.ToString(), new CommonNavigationTransitionInfo());
            }

            window.Activate();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        private void AppUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            e.Exception.LogError("App unhandled exception occured");

            e.Handled = true;
        }

        #endregion
    }
}

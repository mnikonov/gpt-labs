﻿using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;

namespace Gpt.Labs
{
    public partial class App : Application
    {
        #region Constructors

        public App()
        {
            this.InitializeComponent();

            this.UnhandledException += this.AppUnhandledException;
        }

        #endregion

        #region Properties

        public static Window Window { get; private set; }

        public static ResourceLoader ResourceLoader { get; } = new ();

        #endregion

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.ActivateAsync(args.UWPLaunchActivatedEventArgs);

            var appWindow = Window.GetAppWindow();
            appWindow.Title = ResourceLoader.GetString("AppDisplayName");

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            }
        }


        #region Private Methods

        private void ActivateAsync(IActivatedEventArgs args)
        {
            Window = new MainWindow();

            if (!(Window.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += this.OnNavigationFailed;

                Window.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                var query = new Query
                {
                    ["IsTerminated"] = args.PreviousExecutionState == ApplicationExecutionState.Terminated
                };

                if (args is ProtocolActivatedEventArgs protocolArgs && protocolArgs.Uri != null)
                {
                    query["ProtocolActivatedUrl"] = protocolArgs.Uri.AbsoluteUri;
                }

                rootFrame.Navigate(typeof(ActivationPage), query.ToString(), new CommonNavigationTransitionInfo());
            }

            Window.Activate();
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

            e.Handled = true;
        }

        #endregion
    }
}

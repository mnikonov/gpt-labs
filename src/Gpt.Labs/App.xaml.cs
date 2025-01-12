using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using H.NotifyIcon;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

#if !DEBUG

using System.Globalization;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

#endif

namespace Gpt.Labs;

public partial class App : Application
{
    private static Mutex mut = new Mutex(false, @"GptLab-InitDatabase");

    #region Constructors

    public App()
    {
        MigrateDatabase();

        InitializeComponent();

#if !DEBUG
        AppCenter.Configure("{APP_CENTER_SECRET}");
        AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
        
        if (AppCenter.Configured)
        {
            AppCenter.Start(typeof(Analytics));
            AppCenter.Start(typeof(Crashes));
        }
#endif

        UnhandledException += AppUnhandledException;

        AppInstance keyInstance = AppInstance.FindOrRegisterForKey(Program.AppInstanceKey);

        if (keyInstance.IsCurrent)
        {
            keyInstance.Activated += KeyInstance_Activated;
        }
    }

    #endregion

    #region Properties

    public static ResourceLoader ResourceLoader { get; } = new();

    public static TaskbarIcon TrayIcon { get; private set; }

    #endregion

    public static bool GetMainWindow(out MainWindow window, IActivatedEventArgs args = null)
    {
        return WindowManager.TryGet(() => typeof(ActivationPage).CreatePageId(), window =>
        {
            window.SetTitle(ResourceLoader.GetString("AppDisplayName"));

            if (window.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                window.Content = rootFrame;
            }

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
        }, out window);
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        InitializeTrayIcon();

        var kind = AppInstance.FindOrRegisterForKey(Program.AppInstanceKey).GetActivatedEventArgs().Kind;

        if (kind != ExtendedActivationKind.StartupTask)
        {
            GetMainWindow(out var window, args.UWPLaunchActivatedEventArgs);
            window.Show();
        }
    }

    #region Private Methods

    private void KeyInstance_Activated(object sender, AppActivationArguments e)
    {
        TrayIcon.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
        {
            if (GetMainWindow(out var window) || !window.Visible)
            {
                window.Show();
            }
            else
            {
                window.BringToFront();
            }
        });
    }

    private void InitializeTrayIcon()
    {
        if (TrayIcon != null)
        {
            return;
        }

        TrayIcon = (TaskbarIcon)Resources["TrayIcon"];

        TrayIcon.ForceCreate();
    }

    private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
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

    private void MigrateDatabase()
    {
        mut.WaitOne();

        try
        {
            using (var db = new DataContext())
            {
                db.Database.Migrate();

                // var users = db.Profiles.ToList();
                // var dbFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
        }
        finally
        {
            mut.ReleaseMutex();
        }
    }

    #endregion
}

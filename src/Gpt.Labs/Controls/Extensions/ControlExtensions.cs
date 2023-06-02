using Gpt.Labs.Helpers.Extensions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.Controls.Extensions
{
    public static class ControlExtensions
    {
        public static void DisableUiAndExecute(this object controlObj, Action action)
        {
            if (controlObj is Control control)
            {
                control.DisableUiAndExecute(action);
            }
        }

        public static async Task DisableUiAndExecuteAsync(this object controlObj, Func<CancellationToken, Task> action, CancellationToken token = default)
        {
            if (controlObj is Control control)
            {
                await control.DisableUiAndExecuteAsync(action, token);
            }
        }

        public static void DisableUiAndExecute(this Control control, Action action)
        {
            control.IsEnabled = false;

            try
            {
                action();
            }
            finally
            {
                control.IsEnabled = true;
            }
        }

        public static async Task DisableUiAndExecuteAsync(this Control control, Func<CancellationToken, Task> action, CancellationToken token = default)
        {
            control.IsEnabled = false;

            try
            {
                await action(token);
            }
            finally
            {
                await App.Window.DispatcherQueue.EnqueueAsync(() =>
                {
                    control.IsEnabled = true;
                });
            }
        }

        public static async Task BlockUiAndExecute(this ShellPage shell, Func<CancellationToken, Task> action, bool showProgress = true, CancellationToken token = default)
        {
            try
            {
                if (showProgress)
                {
                    shell.IsProgressActive = true;
                }

                await action(token);
            }
            finally
            {
                if (showProgress)
                {
                    await App.Window.DispatcherQueue.EnqueueAsync(() =>
                    {
                        shell.IsProgressActive = false;
                    });
                }
            }
        }
    }
}

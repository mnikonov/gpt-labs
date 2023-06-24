using Microsoft.UI.Xaml;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.Controls.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static async Task ExecuteOnLoaded(this FrameworkElement element, Action action, CancellationToken token = default)
        {
            if (element.IsLoaded)
            {
                action();
                return;
            }

            var tcs = new TaskCompletionSource<object>();
            
            using (token.Register(() => { tcs.TrySetCanceled(token); }))
            {
                RoutedEventHandler loaded = (s, e) =>
                    {
                        action();
                    };

                try
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    element.Loaded += loaded;

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    await tcs.Task;
                }
                finally
                {
                    element.Loaded -= loaded;
                }
            }
        }
    }
}

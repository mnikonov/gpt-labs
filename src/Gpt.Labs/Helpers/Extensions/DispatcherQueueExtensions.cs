using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class DispatcherQueueExtensions
    {
        private static readonly bool IsHasThreadAccessPropertyAvailable = ApiInformation.IsMethodPresent("Windows.System.DispatcherQueue", "HasThreadAccess");

        public static async Task EnqueueAsync(this DispatcherQueue dispatcher, Action function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal, CancellationToken token = default)
        {
            if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
            {
                function();
                return;
            }

            var taskCompletionSource = new TaskCompletionSource<object>();
            
            using (token.Register(() => taskCompletionSource.TrySetCanceled(token)))
            {
                if (!dispatcher.TryEnqueue(
                        priority,
                        () =>
                            {
                                try
                                {
                                    function();

                                    if (!token.IsCancellationRequested)
                                    {
                                        taskCompletionSource.SetResult(null);
                                    }
                                }
                                catch (Exception e)
                                {
                                    taskCompletionSource.SetException(e);
                                }
                            }))
                {
                    taskCompletionSource.SetException(new InvalidOperationException("Failed to enqueue the operation"));
                }

                await taskCompletionSource.Task;   
            }
        }

        public static async Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcher, Func<T> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal, CancellationToken token = default)
        {
            if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
            {
                return function();
            }

            var taskCompletionSource = new TaskCompletionSource<T>();

            using (token.Register(() => taskCompletionSource.TrySetCanceled(token)))
            {
                if (!dispatcher.TryEnqueue(
                        priority,
                        () =>
                            {
                                try
                                {
                                    var result = function();
                                    
                                    if (!token.IsCancellationRequested)
                                    {
                                        taskCompletionSource.SetResult(result);
                                    }
                                }
                                catch (Exception e)
                                {
                                    taskCompletionSource.SetException(e);
                                }
                            }))
                {
                    taskCompletionSource.SetException(new InvalidOperationException("Failed to enqueue the operation"));
                }

                return await taskCompletionSource.Task;   
            }
        }

        public static async Task EnqueueAsync(this DispatcherQueue dispatcher, Func<Task> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal, CancellationToken token = default)
        {
            if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
            {
                await function();
                return;
            }

            var taskCompletionSource = new TaskCompletionSource<object>();

            using (token.Register(() => taskCompletionSource.TrySetCanceled(token)))
            {
                if (!dispatcher.TryEnqueue(
                        priority,
                        async () =>
                            {
                                try
                                {
                                    await function();
                                    
                                    if (!token.IsCancellationRequested)
                                    {
                                        taskCompletionSource.SetResult(null);
                                    }
                                }
                                catch (Exception e)
                                {
                                    taskCompletionSource.SetException(e);
                                }
                            }))
                {
                    taskCompletionSource.SetException(new InvalidOperationException("Failed to enqueue the operation"));
                }

                await taskCompletionSource.Task;   
            }
        }

        public static async Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcher, Func<Task<T>> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal, CancellationToken token = default)
        {
            if (IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess)
            {
                return await function();
            }

            var taskCompletionSource = new TaskCompletionSource<T>();

            using (token.Register(() => taskCompletionSource.TrySetCanceled(token)))
            {
                if (!dispatcher.TryEnqueue(
                        priority,
                        async () =>
                            {
                                try
                                {
                                    var result = await function();
                                    
                                    if (!token.IsCancellationRequested)
                                    {
                                        taskCompletionSource.SetResult(result);
                                    }
                                }
                                catch (Exception e)
                                {
                                    taskCompletionSource.SetException(e);
                                }
                            }))
                {
                    taskCompletionSource.SetException(new InvalidOperationException("Failed to enqueue the operation"));
                }

                return await taskCompletionSource.Task;   
            }
        }
    }
}

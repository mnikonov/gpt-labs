using Gpt.Labs.Helpers.Extensions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.Base
{
    public class NotifyTaskCompletion<TResult> : INotifyPropertyChanged
    {
        #region Private Members

        private CancellationTokenSource cancellation;

        private Task currentTask;

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Func<CancellationToken, Task<TResult>> Function { get; set; }

        public string ErrorMessage => this.Exception?.Message;

        public Exception Exception { get; private set; }

        public bool IsCanceled { get; private set; }

        public bool IsCompleted { get; private set; }

        public bool IsFaulted { get; private set; }

        public TResult Result { get; private set; }

        #endregion

        #region Public Methods

        public void Reset()
        {
            try
            {
                if (this.cancellation != null)
                {
                    this.cancellation.Cancel();
                    this.cancellation.Dispose();
                    this.cancellation = null;
                }

                this.Function = null;

                (this.Result as IDisposable)?.Dispose();
                this.Result = default;

                this.Exception = null;
                this.IsCanceled = false;
                this.IsCompleted = false;
                this.IsFaulted = false;
            }
            catch (Exception ex)
            {
                ex.LogError("Unable Reset NotifyTaskCompletion source");
            } 
        }

        public void Start(bool showProgress = true)
        {
            var previousTask = this.currentTask;

            this.cancellation?.Cancel();
            this.cancellation?.Dispose();
            this.cancellation = null;

            (this.Result as IDisposable)?.Dispose();
            this.Result = default;

            this.cancellation = new CancellationTokenSource();

            this.currentTask = this.StartNewTask(showProgress, previousTask, this.cancellation.Token);
        }

        #endregion

        #region Private Methods

        private async Task StartNewTask(bool showProgress, Task previousTask, CancellationToken token)
        {
            if (previousTask != null)
            {
                try
                {
                    await previousTask;
                }
                catch (Exception)
                {
                    // no need to handle
                }
            }

            try
            {
                if (showProgress)
                {
                    var shell = ShellPage.Current;
                    if (shell != null)
                    {
                        shell.IsProgressActive = true;
                    }
                }

                this.Result = await this.Function(this.cancellation.Token);
                this.IsCompleted = true;

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Result)));
            }
            catch (TaskCanceledException)
            {
                this.IsCanceled = true;

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            catch (OperationCanceledException)
            {
                this.IsCanceled = true;

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            catch (Exception ex)
            {
                this.IsFaulted = true;
                this.Exception = ex;

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exception)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            finally
            {
                if (showProgress)
                {
                    var shell = ShellPage.Current;
                    if (shell != null)
                    {
                        shell.IsProgressActive = false;
                    }
                }
            }
        }

        #endregion
    }
}

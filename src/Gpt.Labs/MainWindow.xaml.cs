using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace Gpt.Labs
{
    public sealed partial class MainWindow : Window
    {
        #region Fields

        private UISettings uISettings;
                
        private DataTransferManager dataTransferManager;

        private ShareContent share;

        #endregion

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            this.uISettings = new UISettings();
            this.uISettings.ColorValuesChanged += OnUISettingsColorValuesChanged;

            this.Closed += OnMainWindowClosed;
            
            this.dataTransferManager = this.GetDataTransferManager();
            this.dataTransferManager.DataRequested += this.OnDataTransferManagerDataRequested;
        }

        #endregion

        #region Properties

        public Guid WindowId { get; } = Guid.NewGuid();

        #endregion

        #region Public Methods

        public void SetShareContent(ShareContent share)
        {
            this.share = share;
        }

        #endregion

        #region Private Methods

        private void OnMainWindowClosed(object sender, WindowEventArgs args)
        {
            this.Closed -= OnMainWindowClosed;
            this.uISettings.ColorValuesChanged -= OnUISettingsColorValuesChanged;
            this.dataTransferManager.DataRequested -= this.OnDataTransferManagerDataRequested;
            
            WindowManager.UnregisterWindow(this.WindowId);
        }

        private async void OnDataTransferManagerDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var requestData = args.Request.Data;
            var differal = args.Request.GetDeferral();

            try
            {
                if (!string.IsNullOrEmpty(share.Title))
                {
                    requestData.Properties.Title = share.Title;
                }

                if (!string.IsNullOrEmpty(share.Message))
                {
                    requestData.SetText(share.Message);
                }

                if (share.Files.Count > 0)
                {
                    var files = new List<IStorageItem>();

                    foreach (var filePath in this.share.Files)
                    {
                        var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(filePath);
                        files.Add(file);
                    }

                    requestData.SetStorageItems(files);
                }
            }
            finally
            {
                differal.Complete();
            }
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

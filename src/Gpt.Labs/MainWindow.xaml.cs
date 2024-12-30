using Gpt.Labs.Helpers;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
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

        private readonly UISettings uISettings;

        private readonly DataTransferManager dataTransferManager;

        private ShareContent share;

        #endregion

        #region Constructors

        public MainWindow(string windowId)
        {
            WindowId = windowId;

            InitializeComponent();

            uISettings = new UISettings();
            uISettings.ColorValuesChanged += OnUISettingsColorValuesChanged;

            dataTransferManager = this.GetDataTransferManager();
            dataTransferManager.DataRequested += OnDataTransferManagerDataRequested;
        }

        #endregion

        #region Properties

        public string WindowId { get; private set; }

        #endregion

        #region Public Methods

        public void SetShareContent(ShareContent share)
        {
            this.share = share;
        }

        #endregion

        #region Private Methods


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

                    foreach (var filePath in share.Files)
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
                        if (ApplicationSettings.Instance.AppTheme == ElementTheme.Default && Content != null)
                        {
                            this.ApplyTheme();
                        }
                    });
        }

        #endregion
    }
}

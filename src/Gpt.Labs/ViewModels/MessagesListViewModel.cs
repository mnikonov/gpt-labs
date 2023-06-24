using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.ViewModels.Base;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using OpenAI;
using OpenAI.Audio;
using System.IO;
using System.Threading;
using System.Linq;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.ViewModels.Enums;
using System.Net.Http;
using Microsoft.UI.Xaml.Controls;
using Microsoft.EntityFrameworkCore;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors;
using Gpt.Labs.ViewModels.Collections.Interfaces;
using System.Collections.Generic;
using Gpt.Labs.ViewModels.Collections;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.UI.Xaml;

namespace Gpt.Labs.ViewModels
{
    public class MessagesListViewModel : ListViewModel<OpenAIMessage>, IQueryableDataProvider<OpenAIMessage, Guid>
    {
        #region Fields

        private string message;

        private bool isRecording;
        
        private bool disposed;

        private Stopwatch watch = new Stopwatch();

        private OpenAIClient openAiClient;

        private OpenAIChat chat;

        private EndpointProcessor<string> messageProcessor;

        private MediaCapture mediaCapture;

        private InMemoryRandomAccessStream mediaMemoryBuffer;

        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private ChatPanelTypes expandedPanels;

        private bool multiSelectModeEnabled;

        private DataTransferManager dataTransferManager;

        private OpenAIMessage[] shareMessages;

        #endregion

        #region Public Constructors

        public MessagesListViewModel(Func<BasePage> getBasePage)
            : base(getBasePage)
        {
            this.openAiClient = new OpenAIClient(new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, ApplicationSettings.Instance.OpenAIOrganization ));
            this.mediaCapture = new MediaCapture();       
        }

        #endregion

        #region Properties

        public Guid ChatId { get; private set; }

        public OpenAIChat Chat 
        { 
            get => this.chat;
            set => this.Set(ref this.chat, value);
        }

        public string Message 
        { 
            get => this.message;
            set => this.Set(ref this.message, value);
        }

        public bool IsRecording 
        { 
            get => this.isRecording;
            set => this.Set(ref this.isRecording, value);
        }

        public int ExpandedPanels
        {
            get => (int)this.expandedPanels;
            set => this.Set(ref this.expandedPanels, (ChatPanelTypes)value);
        }

        public bool MultiSelectModeEnabled 
        { 
            get => this.multiSelectModeEnabled; 
            set => this.Set(ref this.multiSelectModeEnabled, value); 
        }

        #endregion

        #region Public Methods

        public override async Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            await this.mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            });

            if (mode == NavigationMode.New)
            {
                this.ChatId = parameters.GetValue<Guid>("chat-id");
            }
            else
            {
                this.ChatId = state.GetValue<Guid>(nameof(this.ChatId));
            }

            await this.LoadChatInfo();

            var collectiom = new ObservableList<OpenAIMessage, Guid>(this, p => p.Id);
            var messagesCount = ((IQueryableDataProvider<OpenAIMessage, Guid>)this).GetCount();
            collectiom.Initialize(messagesCount > 0 ? messagesCount - 1 : 0);

            this.ItemsCollection = collectiom;

            switch (this.Chat.Type)
            {
                case OpenAIChatType.Chat:
                    this.messageProcessor = new ChatEndpointProcessor(this.openAiClient, this.Chat, this.ItemsCollection, this.DispatcherQueue, () => { this.Message = string.Empty; });
                    break;
                case OpenAIChatType.Image:
                    this.messageProcessor = new ImageEndpointProcessor(this.openAiClient, this.Chat, this.ItemsCollection, this.DispatcherQueue, () => { this.Message = string.Empty; });
                    break;
            }

            this.dataTransferManager = this.Window.GetDataTransferManager();
            this.dataTransferManager.DataRequested += this.OnDataTransferManagerDataRequested;

            await base.LoadStateAsync(destinationPageType, parameters, state, mode);
        }

        public override void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            base.SaveState(destinationPageType, parameters, state, mode);

            state.SetValue(nameof(this.ChatId), this.ChatId);

            this.dataTransferManager.DataRequested -= this.OnDataTransferManagerDataRequested;
        }

        public async Task CancelSettings()
        {
            var dialog = this.Window.CreateYesNoDialog("Confirm", "CancellSettingsChanges");
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await this.LoadChatInfo();
            }
        }

        public async Task SaveSettings()
        {
            using (var context = new DataContext())
            {
                context.Entry(this.Chat.Settings).State = EntityState.Modified;

                switch (this.Chat.Type)
                {
                    case OpenAIChatType.Chat:
                        var chatSettings = (OpenAIChatSettings)this.Chat.Settings;

                        var oldStops = await context.Stops.AsNoTracking().Where(p => p.SettingsId == chatSettings.Id).ToListAsync();
                        
                        var existStops = new HashSet<Guid>();

                        foreach (var item in chatSettings.Stop)
                        {
                            if (item.IsNew)
                            {
                                context.Add(item);
                            }
                            else
                            {
                                context.Entry(item).State = EntityState.Modified;
                            }
                                                        
                            existStops.Add(item.Id);
                        }

                        foreach (var item in oldStops)
                        {
                            if (!existStops.Contains(item.Id))
                            {
                                 context.Entry(item).State = EntityState.Deleted;
                            }
                        }

                        var oldBias = await context.LogitBias.AsNoTracking().Where(p => p.SettingsId == chatSettings.Id).ToListAsync();

                        var existBias = new HashSet<Guid>();

                        foreach (var item in chatSettings.LogitBias)
                        {
                            if (item.IsNew)
                            {
                                context.Add(item);
                            }
                            else
                            {
                                context.Entry(item).State = EntityState.Modified;
                            }

                            existBias.Add(item.Id);
                        }

                        foreach (var item in oldBias)
                        {
                            if (!existBias.Contains(item.Id))
                            {
                                 context.Entry(item).State = EntityState.Deleted;
                            }
                        }

                        break;
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task CreateImageVariation()
        {
            var imagePicker = new FileOpenPicker();
            imagePicker.ViewMode = PickerViewMode.Thumbnail;
            imagePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            imagePicker.FileTypeFilter.Add(".png");

            var hwnd = WindowNative.GetWindowHandle(this.Window);
            InitializeWithWindow.Initialize(imagePicker, hwnd);

            var file = await imagePicker.PickSingleFileAsync();
            
            if (file == null)
            {
                return;
            }

            var properties = await file.GetBasicPropertiesAsync();

            if (properties.Size > 4 * 1024 * 1024)
            {
                var dialog = this.Window.CreateOkDialog("Error", "ImageVariationValidationError");
                await dialog.ShowAsync();
                return;
            }

            try
            {
                var imageVariationProcessor = new ImageVariationEndpointProcessor(this.openAiClient, this.Chat, this.ItemsCollection, this.DispatcherQueue, () => { this.Message = string.Empty; });
                await imageVariationProcessor.ProcessAsync(file);
            }
            catch (HttpRequestException ex)
            {
                var error = ex.ToOpenAiError();

                await this.DispatcherQueue.EnqueueAsync(async () =>
                {
                    ContentDialog dialog = null;

                    if (error != null)
                    {
                        dialog = this.Window.CreateErrorDialog(error);
                    }
                    else
                    {
                        ex.LogError();
                        dialog = this.Window.CreateExceptionDialog(ex);
                    }

                    await dialog.ShowAsync();
                });
            }
            catch (Exception ex)
            {
                ex.LogError();
                await this.DispatcherQueue.EnqueueAsync(async () =>
                {
                    await this.Window.CreateExceptionDialog(ex).ShowAsync();
                });
            }
        }

        public async Task SendMessage()
        {
            if (string.IsNullOrEmpty(this.Message))
            {
                return;
            }

            try
            {
                await this.messageProcessor.ProcessAsync(this.Message);
            }
            catch (HttpRequestException ex)
            {
                var error = ex.ToOpenAiError();

                await this.DispatcherQueue.EnqueueAsync(async () =>
                {
                    ContentDialog dialog = null;

                    if (error != null)
                    {
                        dialog = this.Window.CreateErrorDialog(error);
                    }
                    else
                    {
                        ex.LogError();
                        dialog = this.Window.CreateExceptionDialog(ex);
                    }

                    await dialog.ShowAsync();
                });
            }
            catch (Exception ex)
            {
                ex.LogError();
                await this.DispatcherQueue.EnqueueAsync(async () =>
                {
                    await this.Window.CreateExceptionDialog(ex).ShowAsync();
                });
            }
        }

        public async Task DeleteMessages(params OpenAIMessage[] messages)
        {
            ContentDialog dialog;

            if (messages.Length == 1)
            {
                dialog = this.Window.CreateYesNoDialog("Confirm", "DeleteMessage");
            }
            else
            {
                dialog = this.Window.CreateYesNoDialog("Confirm", "DeleteMessages");
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                using (var context = new DataContext())
                {
                    foreach (var message in messages)
                    {
                        context.Entry(message).State = EntityState.Deleted;

                        await context.SaveChangesAsync();

                        this.ItemsCollection.Remove(message);
                    } 
                }

                if (this.Chat.Type == OpenAIChatType.Image)
                {
                    foreach(var message in messages)
                    {
                        var imageFile = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync($"{message.ChatId}\\{message.Id}.png");

                        if (imageFile != null)
                        {
                            await imageFile.DeleteAsync();
                        }
                    }
                }
            }
        }

        public async Task OpenChatInNewWindow()
        {
            await this.Chat.OpenChatInNewWindows();
        }

        public async Task StartStopRecord()
        {
            await this.semaphoreSlim.WaitAsync();

            try
            {
                if (!this.IsRecording)
                {
                    await this.StartRecord();
                }
                else
                {
                    await this.StopRecord();
                }
            }
            finally
            {
                this.semaphoreSlim.Release();
            }
        }


        public void ExpandCollapsePanel(ChatPanelTypes panelType)
        {
            if (this.IsPanelExpanded(this.ExpandedPanels, (int)panelType))
            {
                this.CollapsePanel(panelType);
            }
            else
            {
                this.ExpandPanel(panelType);
            }
        }

        public void ExpandPanel(ChatPanelTypes panelType)
        {
            this.ExpandedPanels |= (int)panelType;
        }

        public void CollapsePanel(ChatPanelTypes panelType)
        {
            this.ExpandedPanels &= ~(int)panelType;
        }

        public bool IsPanelExpanded(int panels, int panelType)
        {
            return (panels & panelType) != 0;
        }

        public async Task CopyMessages(params OpenAIMessage[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }

            var dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };

            dataPackage.SetText(messages.Format());

            if (this.Chat.Type == OpenAIChatType.Image)
            {
                var images = new List<IStorageItem>();

                foreach (var message in messages)
                {
                    if (message.Role == OpenAIRole.Assistant)
                    {
                        var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync($"{message.ChatId}\\{message.Id}.png");
                        images.Add(file);
                    }
                }

                if (images.Count > 0)
                {
                    dataPackage.SetStorageItems(images);
                }
            }

            Clipboard.SetContent(dataPackage);
        }

        public void ShareMessages(params OpenAIMessage[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }

            this.shareMessages = messages;

            this.Window.ShowShareUI();
        }

        #endregion

        #region Private Methods

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.mediaCapture?.Dispose();
                    this.mediaCapture = null;

                    this.mediaMemoryBuffer?.Dispose();
                    this.mediaMemoryBuffer = null;

                    this.semaphoreSlim.Dispose();
                    this.semaphoreSlim = null;
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }

        private async void OnDataTransferManagerDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var requestData = args.Request.Data;
            var differal = args.Request.GetDeferral();

            try
            {
                requestData.Properties.Title = this.Chat.Title;

                requestData.SetText(this.shareMessages.Format());

                if (this.Chat.Type == OpenAIChatType.Image)
                {
                    var images = new List<IStorageItem>();

                    foreach (var message in this.shareMessages)
                    {
                        if (message.Role == OpenAIRole.Assistant)
                        {
                            var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync($"{message.ChatId}\\{message.Id}.png");
                            images.Add(file);
                        }
                    }

                    if (images.Count > 0)
                    {
                        requestData.SetStorageItems(images);
                    }
                }
            }
            finally
            {
                differal.Complete();
            }
        }

        private async Task StartRecord()
        {        
            this.IsRecording = true;

            this.mediaMemoryBuffer = new InMemoryRandomAccessStream();

            this.watch.Start();

            await this.mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Low), this.mediaMemoryBuffer);
        }

        private async Task StopRecord()
        {
            try
            {
                await this.mediaCapture.StopRecordAsync();
                
                this.watch.Stop();

                if (watch.ElapsedMilliseconds > 2000)
                {
                    this.mediaMemoryBuffer.Seek(0);

                    var request = new AudioTranscriptionRequest(this.mediaMemoryBuffer.AsStream(), "voice.mp3");
                    var result = await openAiClient.AudioEndpoint.CreateTranscriptionAsync(request);
                                
                    this.Message = result;
                }
                else
                {
                    this.Message = string.Empty;
                }
            }
            catch (Exception)
            {
                this.Message = string.Empty;
            }
            finally 
            {                 
                this.mediaMemoryBuffer?.Dispose();
                this.mediaMemoryBuffer = null;

                this.IsRecording = false;

                this.watch.Reset();
            }
        }

        private async Task LoadChatInfo()
        {
            using (var context = new DataContext())
            {
                this.Chat = await context.Chats.AsNoTracking().Include(p => p.Settings).SingleAsync(p => p.Id == this.ChatId);

                switch (this.Chat.Type)
                {
                    case OpenAIChatType.Chat:
                        var chatSettings = (OpenAIChatSettings)this.Chat.Settings;

                        chatSettings.Stop = new ObservableCollection<OpenAIStop>();
                        await foreach (var item in context.Stops.AsNoTracking().Where(p => p.SettingsId == chatSettings.Id).AsAsyncEnumerable())
                        {
                            chatSettings.Stop.Add(item);
                        }

                        chatSettings.LogitBias = new ObservableCollection<OpenAILogitBias>();
                        await foreach (var item in context.LogitBias.AsNoTracking().Where(p => p.SettingsId == chatSettings.Id).AsAsyncEnumerable())
                        {
                            chatSettings.LogitBias.Add(item);
                        }

                        break;
                }
            }
        }

        bool IQueryableDataProvider<OpenAIMessage, Guid>.Contains(Guid id)
        {
            using (var context = new DataContext())
            {
                return context.Messages.Contains(this.ChatId, id);
            }
        }

        OpenAIMessage IQueryableDataProvider<OpenAIMessage, Guid>.GetById(Guid id)
        {
            using (var context = new DataContext())
            {
                return context.Messages.GetById(this.ChatId, id);
            }
        }

        int IQueryableDataProvider<OpenAIMessage, Guid>.GetCount()
        {
            using (var context = new DataContext())
            {
                return context.Messages.Count(this.ChatId);
            }
        }

        IEnumerable<OpenAIMessage> IQueryableDataProvider<OpenAIMessage, Guid>.GetInRange(int skip, int take)
        {
            using (var context = new DataContext())
            {
                return context.Messages.GetInRange(this.ChatId, skip, take).ToList();
            }
        }

        int IQueryableDataProvider<OpenAIMessage, Guid>.IndexOf(OpenAIMessage item)
        {
            using (var context = new DataContext())
            {
                return context.Messages.IndexOf(item) - 1;
            }
        }

        #endregion
    }
}

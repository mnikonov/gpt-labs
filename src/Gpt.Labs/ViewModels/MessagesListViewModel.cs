using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Helpers.Navigation;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Exceptions;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.ViewModels.Base;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.Collections.Interfaces;
using Gpt.Labs.ViewModels.Enums;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OpenAI;
using OpenAI.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace Gpt.Labs.ViewModels
{
    public class MessagesListViewModel : ListViewModel<OpenAIMessage>, IQueryableDataProvider<OpenAIMessage, Guid>
    {
        #region Fields

        private string message;

        private bool isRecording;

        private bool disposed;

        private readonly Stopwatch watch = new();

        private OpenAIChat chat;

        private EndpointProcessor<OpenAIMessage> messageProcessor;

        private MediaCapture mediaCapture;

        private InMemoryRandomAccessStream mediaMemoryBuffer;

        private SemaphoreSlim semaphoreSlim = new(1, 1);

        private ChatPanelTypes expandedPanels;

        private bool multiSelectModeEnabled;

        private CancellationTokenSource cancellation;

        private bool processingMessage;

        #endregion

        #region Public Constructors

        public MessagesListViewModel(Func<BasePage> getBasePage)
            : base(getBasePage)
        {
            mediaCapture = new MediaCapture();
        }

        #endregion

        #region Properties

        public Guid ChatId { get; private set; }

        public OpenAIChat Chat
        {
            get => chat;
            set => Set(ref chat, value);
        }

        public string Message
        {
            get => message;
            set => Set(ref message, value);
        }

        public bool IsRecording
        {
            get => isRecording;
            set => Set(ref isRecording, value);
        }

        public int ExpandedPanels
        {
            get => (int)expandedPanels;
            set => Set(ref expandedPanels, (ChatPanelTypes)value);
        }

        public bool MultiSelectModeEnabled
        {
            get => multiSelectModeEnabled;
            set => Set(ref multiSelectModeEnabled, value);
        }

        public bool ProcessingMessage
        {
            get => processingMessage;
            set => Set(ref processingMessage, value);
        }

        public IReadOnlyCollection<string> SupportedChatModels { get; private set; }

        #endregion

        #region Public Methods

        public override async Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            });

            ChatId = mode == NavigationMode.New ? parameters.GetValue<Guid>("chat-id") : state.GetValue<Guid>(nameof(ChatId));

            await LoadChatInfo();

            var collectiom = new ObservableList<OpenAIMessage, Guid>(this, p => p.Id);
            var messagesCount = ((IQueryableDataProvider<OpenAIMessage, Guid>)this).GetCount();
            collectiom.Initialize(messagesCount > 0 ? messagesCount - 1 : 0);

            ItemsCollection = collectiom;

            switch (Chat.Type)
            {
                case OpenAIChatType.Chat:
                    messageProcessor = new ChatEndpointProcessor(Chat, ItemsCollection, DispatcherQueue, () => { Message = string.Empty; });
                    break;
                case OpenAIChatType.Image:
                    messageProcessor = new ImageEndpointProcessor(Chat, ItemsCollection, DispatcherQueue, () => { Message = string.Empty; });
                    break;
            }

            await InitSupportedChatModels();

            await base.LoadStateAsync(destinationPageType, parameters, state, mode);
        }

        public override void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            base.SaveState(destinationPageType, parameters, state, mode);

            state.SetValue(nameof(ChatId), ChatId);
        }

        public async Task CancelSettings()
        {
            var dialog = Window.CreateYesNoDialog("Confirm", "CancellSettingsChanges");
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await LoadChatInfo();
            }
        }

        public async Task SaveSettings()
        {
            using var context = new DataContext();
            context.Entry(Chat.Settings).State = EntityState.Modified;

            switch (Chat.Type)
            {
                case OpenAIChatType.Chat:
                    var chatSettings = (OpenAIChatSettings)Chat.Settings;

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

        public async Task CreateImageVariation()
        {
            var imagePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            imagePicker.FileTypeFilter.Add(".png");

            var hwnd = WindowNative.GetWindowHandle(Window);
            InitializeWithWindow.Initialize(imagePicker, hwnd);

            var file = await imagePicker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            var properties = await file.GetBasicPropertiesAsync();

            if (properties.Size > 4 * 1024 * 1024)
            {
                var dialog = Window.CreateOkDialog("Error", "ImageVariationValidationError");
                await dialog.ShowAsync();
                return;
            }

            await WrapOpenAiRequest(async () =>
            {
                var imageVariationProcessor = new ImageVariationEndpointProcessor(Chat, ItemsCollection, DispatcherQueue, () => { Message = string.Empty; });
                await imageVariationProcessor.ProcessAsync(file, cancellation.Token);
            });
        }

        public async Task SendMessage()
        {
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }

            await WrapOpenAiRequest(() =>
            {
                return messageProcessor.ProcessAsync(new OpenAIMessage { Role = OpenAIRole.User, Content = Message, ChatId = ChatId }, cancellation.Token);
            });
        }

        public void CancelChatRequest()
        {
            if (!ProcessingMessage || cancellation == null || cancellation.IsCancellationRequested)
            {
                return;
            }

            cancellation.Cancel();
        }

        public async Task RegenerateResponse()
        {
            if (ProcessingMessage || ItemsCollection.Count == 0)
            {
                return;
            }

            var messagesToDelete = new List<OpenAIMessage>();

            OpenAIMessage userMessage = null;

            for (int i = ItemsCollection.Count - 1; i >= 0; i--)
            {
                if (ItemsCollection[i].Role == OpenAIRole.User)
                {
                    userMessage = ItemsCollection[i];
                    break;
                }

                messagesToDelete.Add(ItemsCollection[i]);
            }

            if (userMessage == null)
            {
                return;
            }

            await DeleteMessages(false, messagesToDelete.ToArray());

            await WrapOpenAiRequest(() =>
            {
                return messageProcessor.ProcessAsync(userMessage, cancellation.Token);
            });
        }

        public async Task DeleteLastMessages()
        {
            if (ProcessingMessage || ItemsCollection.Count == 0)
            {
                return;
            }

            var messagesToDelete = new List<OpenAIMessage>();

            for (int i = ItemsCollection.Count - 1; i >= 0; i--)
            {
                messagesToDelete.Add(ItemsCollection[i]);

                if (ItemsCollection[i].Role == OpenAIRole.User)
                {
                    break;
                }
            }

            await DeleteMessages(true, messagesToDelete.ToArray());
        }

        public async Task DeleteMessages(bool showConfirmationDialog, params OpenAIMessage[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }

            if (showConfirmationDialog)
            {
                ContentDialog dialog = messages.Length == 1
                    ? Window.CreateYesNoDialog("Confirm", "DeleteMessage")
                    : Window.CreateYesNoDialog("Confirm", "DeleteMessages", messages.Length);
                var result = await dialog.ShowAsync();

                if (result != ContentDialogResult.Primary)
                {
                    return;
                }
            }

            using (var context = new DataContext())
            {
                foreach (var message in messages)
                {
                    context.Entry(message).State = EntityState.Deleted;

                    await context.SaveChangesAsync();

                    ItemsCollection.Remove(message);
                }
            }

            if (Chat.Type == OpenAIChatType.Image)
            {
                foreach (var message in messages)
                {
                    var imageFile = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync($"{message.ChatId}\\{message.Id}.png");

                    if (imageFile != null)
                    {
                        await imageFile.DeleteAsync();
                    }
                }
            }
        }

        public async Task OpenChatInNewWindow()
        {
            await Chat.OpenChatInNewWindows();
        }

        public async Task StartStopRecord()
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                if (!IsRecording)
                {
                    await StartRecord();
                }
                else
                {
                    await StopRecord();
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }


        public void ExpandCollapsePanel(ChatPanelTypes panelType)
        {
            if (IsPanelExpanded(ExpandedPanels, (int)panelType))
            {
                CollapsePanel(panelType);
            }
            else
            {
                ExpandPanel(panelType);
            }
        }

        public void ExpandPanel(ChatPanelTypes panelType)
        {
            ExpandedPanels |= (int)panelType;
        }

        public void CollapsePanel(ChatPanelTypes panelType)
        {
            ExpandedPanels &= ~(int)panelType;
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

            if (Chat.Type == OpenAIChatType.Image)
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

            Window.SetShareContent(Chat.ShareChatContent(messages));
            Window.ShowShareUI();
        }

        #endregion

        #region Private Methods

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    mediaCapture?.Dispose();
                    mediaCapture = null;

                    mediaMemoryBuffer?.Dispose();
                    mediaMemoryBuffer = null;

                    semaphoreSlim.Dispose();
                    semaphoreSlim = null;
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }

        private async Task StopRecord()
        {
            try
            {
                await mediaCapture.StopRecordAsync();

                watch.Stop();

                if (watch.ElapsedMilliseconds > 2000)
                {
                    mediaMemoryBuffer.Seek(0);

                    await WrapOpenAiRequest(async () =>
                    {
                        var client = new OpenAIClient(new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, !string.IsNullOrEmpty(chat.Settings.OpenAIOrganization) ? chat.Settings.OpenAIOrganization : ApplicationSettings.Instance.OpenAIOrganization));
                        var request = new AudioTranscriptionRequest(mediaMemoryBuffer.AsStream(), "voice.mp3");
                        var result = await client.WrapAction((client, token) => client.AudioEndpoint.CreateTranscriptionTextAsync(request, token), cancellation.Token);

                        Message = result;
                    });
                }
                else
                {
                    Message = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Message = string.Empty;
                ex.LogError("Exception on attempt to stop recording");
            }
            finally
            {
                mediaMemoryBuffer?.Dispose();
                mediaMemoryBuffer = null;

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    IsRecording = false;
                });

                watch.Reset();
            }
        }

        private async Task StartRecord()
        {
            IsRecording = true;

            mediaMemoryBuffer = new InMemoryRandomAccessStream();

            watch.Start();

            await mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Low), mediaMemoryBuffer);
        }

        private async Task LoadChatInfo()
        {
            using var context = new DataContext();
            Chat = await context.Chats.AsNoTracking().Include(p => p.Settings).SingleAsync(p => p.Id == ChatId);

            switch (Chat.Type)
            {
                case OpenAIChatType.Chat:
                    var chatSettings = (OpenAIChatSettings)Chat.Settings;

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

        private async Task InitSupportedChatModels()
        {
            try
            {
                var authentication = new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, ApplicationSettings.Instance.OpenAIOrganization);
                var api = new OpenAIClient(authentication);
                var allModels = await api.WrapAction((client) => client.ModelsEndpoint.GetModelsAsync());

                switch (Chat.Type)
                {
                    case OpenAIChatType.Chat:
                        SupportedChatModels = allModels.Where(p => p.Id.Contains("gpt")).OrderByDescending(p => p.CreatedAt).Select(p => p.Id).ToList().AsReadOnly();
                        break;
                    case OpenAIChatType.Image:
                        SupportedChatModels = allModels.Where(p => p.Id.Contains("dall")).OrderByDescending(p => p.CreatedAt).Select(p => p.Id).ToList().AsReadOnly();
                        break;
                }
            }
            catch (OpenAiException ex)
            {
                await Window.CreateErrorDialog(ex).ShowAsync();
            }
            catch (Exception ex)
            {
                ex.LogError();

                await Window.CreateExceptionDialog(ex).ShowAsync();
            }
        }

        private async Task WrapOpenAiRequest(Func<Task> action)
        {
            try
            {
                ProcessingMessage = true;
                cancellation = new CancellationTokenSource();

                await action();
            }
            catch (OpenAiException ex)
            {
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    await Window.CreateErrorDialog(ex).ShowAsync();
                });
            }
            catch (TaskCanceledException)
            {
                // no need to handle
            }
            catch (OperationCanceledException)
            {
                // no need to handle
            }
            catch (Exception ex)
            {
                ex.LogError();

                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    await Window.CreateExceptionDialog(ex).ShowAsync();
                });
            }
            finally
            {
                cancellation?.Dispose();
                cancellation = null;

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    ProcessingMessage = false;
                });
            }
        }

        bool IQueryableDataProvider<OpenAIMessage, Guid>.Contains(Guid id)
        {
            using var context = new DataContext();
            return context.Messages.Contains(ChatId, id);
        }

        OpenAIMessage IQueryableDataProvider<OpenAIMessage, Guid>.GetById(Guid id)
        {
            using var context = new DataContext();
            return context.Messages.GetById(ChatId, id);
        }

        int IQueryableDataProvider<OpenAIMessage, Guid>.GetCount()
        {
            using var context = new DataContext();
            return context.Messages.Count(ChatId);
        }

        IEnumerable<OpenAIMessage> IQueryableDataProvider<OpenAIMessage, Guid>.GetInRange(int skip, int take)
        {
            using var context = new DataContext();
            return context.Messages.GetInRange(ChatId, skip, take).ToList();
        }

        int IQueryableDataProvider<OpenAIMessage, Guid>.IndexOf(OpenAIMessage item)
        {
            using var context = new DataContext();
            return context.Messages.IndexOf(item) - 1;
        }

        #endregion
    }
}

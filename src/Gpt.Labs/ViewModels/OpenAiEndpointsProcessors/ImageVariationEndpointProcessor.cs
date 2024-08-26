using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using Microsoft.UI.Dispatching;
using OpenAI;
using OpenAI.Images;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors
{
    public class ImageVariationEndpointProcessor : EndpointProcessor<StorageFile>
    {
        #region Constructors

        public ImageVariationEndpointProcessor(OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, DispatcherQueue dispatcher, Action cleanUserMessage)
            : base(chat, messagesCollection, dispatcher, cleanUserMessage)
        {
        }

        #endregion

        #region Public Methods

        public override async Task ProcessAsync(StorageFile storageFile, CancellationToken token)
        {
            var settings = chat.GetSettings<OpenAIImageSettings>();

            var responseMessages = new OpenAIMessage[settings.N];

            var chatRequest = settings.ToImageVariationRequest(storageFile);

            var client = new OpenAIClient(GetAuthentication());

            var result = await client.WrapAction((client, token) => client.ImagesEndPoint.CreateImageVariationAsync(chatRequest, token), token);

            await HandleChatResponseAsync(storageFile, result, responseMessages, token);

            await SaveChatMessagesAsync(token, responseMessages);
        }

        #endregion

        #region Private Methods

        private async Task HandleChatResponseAsync(StorageFile storageFile, IReadOnlyList<ImageResult> images, OpenAIMessage[] responseMessages, CancellationToken token)
        {
            var message = await AddMessageToCollectionAsync(new OpenAIMessage() { Role = OpenAIRole.User, Content = string.Empty, ChatId = chat.Id }, token);

            await CleanUserMessageAsync(token);

            var chatFolder = await GetChatFolder(token);

            var messageFile = await storageFile.CopyAsync(chatFolder, $"{message.Id}.png").AsTask(token);

            await dispatcher.EnqueueAsync(() =>
            {
                message.Content = $"![Image]({messageFile.Path})";
            },
            DispatcherQueuePriority.Normal,
            token);

            await SaveChatMessagesAsync(token, message);

            using var client = new HttpClient();
            for (int i = 0; i < images.Count; i++)
            {
                string imageUrl = images[i].Url;

                var responseMessage = await GetMessageAsync(responseMessages, i, token);
                await SaveImageAsync(client, chatFolder, responseMessage, imageUrl, token);
            }
        }

        public async Task SaveImageAsync(HttpClient client, StorageFolder chatFolder, OpenAIMessage message, string imageUrl, CancellationToken token)
        {
            using var imageData = await client.GetStreamAsync(imageUrl, token);
            var file = await chatFolder.CreateFileAsync($"{message.Id}.png", CreationCollisionOption.ReplaceExisting).AsTask(token);

            using (var writeStream = await file.OpenStreamForWriteAsync())
            {
                await imageData.CopyToAsync(writeStream, token);
                await writeStream.FlushAsync(token);
            }

            await dispatcher.EnqueueAsync(() =>
            {
                message.Content = $"![Image]({file.Path})";
            },
            DispatcherQueuePriority.Normal, token);
        }

        private async Task<StorageFolder> GetChatFolder(CancellationToken token)
        {
            var chatFolder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(chat.Id.ToString()).AsTask(token);

            chatFolder ??= await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(chat.Id.ToString()).AsTask(token);

            return (StorageFolder)chatFolder;
        }

        #endregion
    }
}

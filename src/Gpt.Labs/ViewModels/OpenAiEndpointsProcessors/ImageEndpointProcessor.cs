using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using Microsoft.UI.Dispatching;
using OpenAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors
{
    public class ImageEndpointProcessor : EndpointProcessor<OpenAIMessage>
    {
        #region Constructors

        public ImageEndpointProcessor(OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, DispatcherQueue dispatcher, Action cleanUserMessage) 
            : base(chat, messagesCollection, dispatcher, cleanUserMessage)
        {
        }

        #endregion

        #region Public Methods

        public override async Task ProcessAsync(OpenAIMessage userMessage, CancellationToken token)
        {
            var settings = this.chat.GetSettings<OpenAIImageSettings>();

            var responseMessages = new OpenAIMessage[settings.N];

            var chatRequest = settings.ToImageGenerationRequest(userMessage);

            var client = new OpenAIClient(this.authentication);

            var result = await client.ImagesEndPoint.GenerateImageAsync(chatRequest, token);
            await this.HandleChatResponseAsync(userMessage, result, responseMessages, token);
        }

        #endregion

        #region Private Methods

        private async Task HandleChatResponseAsync(OpenAIMessage userMessage, IReadOnlyList<string> images, OpenAIMessage[] responseMessages, CancellationToken token)
        {
            if (userMessage.IsNew)
            {
                await AddMessageToCollectionAsync(userMessage, token);
                await CleanUserMessageAsync(token);
            }

            await CleanUserMessageAsync(token);

            var chatFolder = await this.GetChatFolder(token);

            using (var client = new HttpClient())
            {
                for (int i = 0; i < images.Count; i++)
                {
                    string imageUrl = images[i];

                    var responseMessage = await GetMessageAsync(responseMessages, i, token);
                    await this.SaveImageAsync(client, chatFolder, responseMessage, imageUrl, token);
                    
                    await SaveChatMessagesAsync(token, responseMessage);
                }
            }
        }

        public async Task SaveImageAsync(HttpClient client, StorageFolder chatFolder, OpenAIMessage message, string imageUrl, CancellationToken token)
        {
            using(var imageData = await client.GetStreamAsync(imageUrl, token))
            {
                var file = await chatFolder.CreateFileAsync($"{message.Id}.png", CreationCollisionOption.ReplaceExisting).AsTask(token);

                using (var writeStream = await file.OpenStreamForWriteAsync())
                {
                    await imageData.CopyToAsync(writeStream, token);
                    await writeStream.FlushAsync(token);
                }

                await this.dispatcher.EnqueueAsync(() =>
                {
                    message.Content = $"![Image]({file.Path})";
                },
                DispatcherQueuePriority.Normal,
                token);
            }
        }

        private async Task<StorageFolder> GetChatFolder(CancellationToken token)
        {
            var chatFolder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(chat.Id.ToString()).AsTask(token);

            if (chatFolder == null)
            {
                chatFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(chat.Id.ToString()).AsTask(token);
            }

            return (StorageFolder)chatFolder;
        }

        #endregion
    }
}

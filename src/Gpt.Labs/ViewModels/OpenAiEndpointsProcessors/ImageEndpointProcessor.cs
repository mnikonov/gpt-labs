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
using System.Threading.Tasks;
using Windows.Storage;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors
{
    public class ImageEndpointProcessor : EndpointProcessor<string>
    {
        #region Constructors

        public ImageEndpointProcessor(OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, DispatcherQueue dispatcher, Action cleanUserMessage) 
            : base(chat, messagesCollection, dispatcher, cleanUserMessage)
        {
        }

        #endregion

        #region Public Methods

        public override async Task ProcessAsync(string userMessage)
        {
            var settings = this.chat.GetSettings<OpenAIImageSettings>();

            var responseMessages = new OpenAIMessage[settings.N];

            var chatRequest = settings.ToImageGenerationRequest(userMessage);

            var client = new OpenAIClient(this.authentication);

            var result = await client.ImagesEndPoint.GenerateImageAsync(chatRequest);
            await this.HandleChatResponseAsync(userMessage, result, responseMessages);

            await SaveChatMessagesAsync(responseMessages);
        }

        #endregion

        #region Private Methods

        private async Task HandleChatResponseAsync(string userMessage, IReadOnlyList<string> images, OpenAIMessage[] responseMessages)
        {
            var message = await AddMessageToCollectionAsync(userMessage, OpenAIRole.User);

            await CleanUserMessageAsync();

            var chatFolder = await this.GetChatFolder();

            using (var client = new HttpClient())
            {
                for (int i = 0; i < images.Count; i++)
                {
                    string imageUrl = images[i];

                    var responseMessage = await GetMessageAsync(responseMessages, i);
                    await this.SaveImageAsync(client, chatFolder, responseMessage, imageUrl);
                }
            }
        }

        private async Task<OpenAIMessage> GetMessageAsync(OpenAIMessage[] messages, int index)
        {
            if (messages[index] == null)
            {
                messages[index] = await AddMessageToCollectionAsync(string.Empty, OpenAIRole.Assistant);

                if (messages.Length > 1)
                {
                    for (var i = 0; i < messages.Length; i++)
                    {
                        if (i == index || messages[i] == null)
                        {
                            continue;
                        }

                        var collectionMessage = messagesCollection.GetById(messages[i].Id);

                        if (messages[i] != collectionMessage)
                        {
                            collectionMessage.Content = messages[i].Content;
                            messages[i] = collectionMessage;
                        }
                    }
                }
            }

            return messages[index];
        }

        public async Task SaveImageAsync(HttpClient client, StorageFolder chatFolder, OpenAIMessage message, string imageUrl)
        {
            using(var imageData = await client.GetStreamAsync(imageUrl))
            {
                var file = await chatFolder.CreateFileAsync($"{message.Id}.png", CreationCollisionOption.ReplaceExisting);

                using (var writeStream = await file.OpenStreamForWriteAsync())
                {
                    await imageData.CopyToAsync(writeStream);
                    await writeStream.FlushAsync();
                }

                await this.dispatcher.EnqueueAsync(() =>
                {
                    message.Content = $"![Image]({file.Path})";
                });
            }
        }

        private async Task<StorageFolder> GetChatFolder()
        {
            var chatFolder = (await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(chat.Id.ToString()));

            if (chatFolder == null)
            {
                chatFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(chat.Id.ToString());
            }

            return (StorageFolder)chatFolder;
        }

        #endregion
    }
}

﻿using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using OpenAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors
{
    public class ImageVariationEndpointProcessor : EndpointProcessor<StorageFile>
    {
        #region Constructors

        public ImageVariationEndpointProcessor(OpenAIClient openAiClient, OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, Action cleanUserMessage) 
            : base(openAiClient, chat, messagesCollection, cleanUserMessage)
        {
        }

        #endregion

        #region Public Methods

        public override async Task ProcessAsync(StorageFile storageFile)
        {
            var settings = this.chat.GetSettings<OpenAIImageSettings>();

            var responseMessages = new OpenAIMessage[settings.N];

            var chatRequest = settings.ToImageVariationRequest(storageFile);

            var result = await openAiClient.ImagesEndPoint.CreateImageVariationAsync(chatRequest);

            await this.HandleChatResponseAsync(storageFile, result, responseMessages);

            await SaveChatMessagesAsync(responseMessages);
        }

        #endregion

        #region Private Methods

        private async Task HandleChatResponseAsync(StorageFile storageFile, IReadOnlyList<string> images, OpenAIMessage[] responseMessages)
        {
            var message = await AddMessageToCollectionAsync(string.Empty, OpenAIRole.User);

            await CleanUserMessageAsync();

            var chatFolder = await this.GetChatFolder();
                        
            var messageFile = await storageFile.CopyAsync(chatFolder, $"{message.Id}.png");

            await App.Window.DispatcherQueue.EnqueueAsync(() =>
            {
                message.Content = $"![Image]({messageFile.Path})";
            });

            await SaveChatMessagesAsync(message);

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

                await App.Window.DispatcherQueue.EnqueueAsync(() =>
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

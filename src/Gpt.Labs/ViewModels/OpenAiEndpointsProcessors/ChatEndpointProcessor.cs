using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using Microsoft.UI.Dispatching;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors
{
    public class ChatEndpointProcessor : EndpointProcessor<string>
    {
        #region Constructors

        public ChatEndpointProcessor(OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, DispatcherQueue dispatcher, Action cleanUserMessage) 
            : base(chat, messagesCollection, dispatcher, cleanUserMessage)
        {
        }

        #endregion

        #region Public Methods

        public override async Task ProcessAsync(string userMessage)
        {
            var settings = this.chat.GetSettings<OpenAIChatSettings>();

            var responseMessages = new OpenAIMessage[settings.N];
            bool userMessageInitialized = false;

            var chatRequest = settings.ToChatRequest(this.messagesCollection, userMessage);

            var client = new OpenAIClient(this.authentication);

            if (settings.Stream)
            {
                await foreach (var result in client.ChatEndpoint.StreamCompletionEnumerableAsync(chatRequest))
                {
                    userMessageInitialized = await this.HandleChatResponseAsync(userMessage, result, responseMessages, userMessageInitialized);
                }
            }
            else
            {
                var result = await client.ChatEndpoint.GetCompletionAsync(chatRequest);
                await this.HandleChatResponseAsync(userMessage, result, responseMessages, userMessageInitialized);
            }

            await SaveChatMessagesAsync(responseMessages);
        }

        #endregion

        #region Private Methods

        private async Task<bool> HandleChatResponseAsync(string userMessage, ChatResponse chatResponse, OpenAIMessage[] responseMessages, bool userMessageInitialized)
        {
            if (!userMessageInitialized)
            {
                var message = await AddMessageToCollectionAsync(userMessage, OpenAIRole.User);

                await CleanUserMessageAsync();

                userMessageInitialized = true;
            }

            foreach (var choise in chatResponse.Choices)
            {
                var settings = this.chat.GetSettings<OpenAIChatSettings>();
                var content = settings.Stream ? choise.Delta?.Content : choise.Message?.Content;

                if (!string.IsNullOrEmpty(content))
                {
                    var responseMessage = await GetMessageAsync(responseMessages, choise.Index);

                    await this.dispatcher.EnqueueAsync(() =>
                    {
                        responseMessage.Content = responseMessage.Content + content;
                    });
                }
            }

            return userMessageInitialized;
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

        #endregion
    }
}

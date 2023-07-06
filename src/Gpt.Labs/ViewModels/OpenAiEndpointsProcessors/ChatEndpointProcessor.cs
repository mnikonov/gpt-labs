using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Extensions;
using Gpt.Labs.ViewModels.Collections;
using Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base;
using Microsoft.UI.Dispatching;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors
{
    public class ChatEndpointProcessor : EndpointProcessor<OpenAIMessage>
    {
        #region Constructors

        public ChatEndpointProcessor(OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, DispatcherQueue dispatcher, Action cleanUserMessage) 
            : base(chat, messagesCollection, dispatcher, cleanUserMessage)
        {
        }

        #endregion

        #region Public Methods

        public override async Task ProcessAsync(OpenAIMessage userMessage, CancellationToken token)
        {
            var settings = this.chat.GetSettings<OpenAIChatSettings>();

            var responseMessages = new OpenAIMessage[settings.N];

            var chatRequest = settings.ToChatRequest(this.messagesCollection, userMessage);

            var client = new OpenAIClient(this.GetAuthentication());

            try
            {
                if (settings.Stream)
                {
                    await client.WrapAction(async (client, token) =>
                    {
                        await foreach (var result in client.ChatEndpoint.StreamCompletionEnumerableAsync(chatRequest, token))
                        {
                            await this.HandleChatResponseAsync(userMessage, result, responseMessages, token);
                        }

                        return true;
                    }, 
                    token);
                }
                else
                {
                    var result = await client.WrapAction((client, token) => client.ChatEndpoint.GetCompletionAsync(chatRequest, token), token);
                    await this.HandleChatResponseAsync(userMessage, result, responseMessages, token);
                }
            }
            finally
            {
                if (responseMessages.Length > 0)
                {                    
                    await SaveChatMessagesAsync(default, responseMessages);
                }
            }
        }

        #endregion

        #region Private Methods

        private async Task HandleChatResponseAsync(OpenAIMessage userMessage, ChatResponse chatResponse, OpenAIMessage[] responseMessages, CancellationToken token)
        {
            if (userMessage.IsNew)
            {
                await AddMessageToCollectionAsync(userMessage, token);
                await CleanUserMessageAsync(token);
            }

            foreach (var choise in chatResponse.Choices)
            {
                var settings = this.chat.GetSettings<OpenAIChatSettings>();
                var content = settings.Stream ? choise.Delta?.Content : choise.Message?.Content;

                if (!string.IsNullOrEmpty(content))
                {
                    var responseMessage = await GetMessageAsync(responseMessages, choise.Index, token);

                    await this.dispatcher.EnqueueAsync(() =>
                    {
                        responseMessage.Content = responseMessage.Content + content;
                    },
                    DispatcherQueuePriority.Normal,
                    token);
                }
            }
        }

        #endregion
    }
}

using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels.Collections;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base
{
    public abstract class EndpointProcessor<TMessage>
    {
        #region Fields

        protected OpenAIClient openAiClient;

        protected OpenAIChat chat;

        protected ObservableList<OpenAIMessage, Guid> messagesCollection;

        private Action cleanUserMessage;

        #endregion

        #region Constructors

        public EndpointProcessor(OpenAIClient openAiClient, OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, Action cleanUserMessage) 
        { 
            this.openAiClient = openAiClient; 
            this.chat = chat;
            this.messagesCollection = messagesCollection;
            this.cleanUserMessage = cleanUserMessage;
        }

        #endregion

        #region Methods

        public abstract Task ProcessAsync(TMessage message);

        #endregion

        #region Protected Methods

        protected async Task<OpenAIMessage> AddMessageToCollectionAsync(string message, OpenAIRole role)
        {
            var aiMessage = new OpenAIMessage() { Role = role, Content = message, ChatId = chat.Id };

            await SaveChatMessagesAsync(aiMessage);

            await App.Window.DispatcherQueue.EnqueueAsync(() =>
            {
                messagesCollection.Add(aiMessage);
            });

            return messagesCollection[messagesCollection.IndexOf(aiMessage)];
        }

        protected Task CleanUserMessageAsync()
        {
            return App.Window.DispatcherQueue.EnqueueAsync(() =>
            {
                cleanUserMessage();
            });
        }

        protected async Task SaveChatMessagesAsync(params OpenAIMessage[] messages)
        {
            using (var context = new DataContext())
            {
                foreach (var message in messages)
                {
                    if (message.IsNew)
                    {
                        context.Add(message);
                    }
                    else
                    {
                        context.Entry(message).State = EntityState.Modified;
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        #endregion
    }
}

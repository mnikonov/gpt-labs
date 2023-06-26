using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.ViewModels.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gpt.Labs.ViewModels.OpenAiEndpointsProcessors.Base
{
    public abstract class EndpointProcessor<TMessage>
    {
        #region Fields

        protected OpenAIChat chat;

        protected ObservableList<OpenAIMessage, Guid> messagesCollection;

        protected DispatcherQueue dispatcher;

        protected OpenAIAuthentication authentication;

        private Action cleanUserMessage;

        #endregion

        #region Constructors

        public EndpointProcessor(OpenAIChat chat, ObservableList<OpenAIMessage, Guid> messagesCollection, DispatcherQueue dispatcher, Action cleanUserMessage) 
        { 
            this.chat = chat;
            this.messagesCollection = messagesCollection;
            this.cleanUserMessage = cleanUserMessage;
            this.dispatcher = dispatcher;

            this.authentication = new OpenAIAuthentication(ApplicationSettings.Instance.OpenAIApiKey, !string.IsNullOrEmpty(this.chat.Settings.OpenAIOrganization) ? this.chat.Settings.OpenAIOrganization : ApplicationSettings.Instance.OpenAIOrganization );
        }

        #endregion

        #region Methods

        public abstract Task ProcessAsync(TMessage message, CancellationToken token);

        #endregion

        #region Protected Methods

        protected async Task<OpenAIMessage> AddMessageToCollectionAsync(OpenAIMessage message, CancellationToken token)
        {
            await SaveChatMessagesAsync(token, message);

            await this.dispatcher.EnqueueAsync(() =>
            {
                messagesCollection.Add(message);
            }, 
            DispatcherQueuePriority.Normal, 
            token);

            return messagesCollection[messagesCollection.IndexOf(message)];
        }

        protected Task CleanUserMessageAsync(CancellationToken token)
        {
            return this.dispatcher.EnqueueAsync(() =>
            {
                cleanUserMessage();
            }, 
            DispatcherQueuePriority.Normal, 
            token);
        }

        protected async Task SaveChatMessagesAsync(CancellationToken token, params OpenAIMessage[] messages)
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

                await context.SaveChangesAsync(token);
            }
        }

        protected async Task<OpenAIMessage> GetMessageAsync(OpenAIMessage[] messages, int index, CancellationToken token)
        {
            if (messages[index] == null)
            {
                messages[index] = await AddMessageToCollectionAsync(new OpenAIMessage() { Role = OpenAIRole.Assistant, Content = string.Empty, ChatId = chat.Id }, token);

                if (messages.Length > 1)
                {
                    for (var i = 0; i < messages.Length; i++)
                    {
                        if (i == index || messages[i] == null)
                        {
                            continue;
                        }

                        var collectionMessage = messagesCollection.GetById(messages[i].Id);

                        token.ThrowIfCancellationRequested();

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

using Gpt.Labs.Models.Enums;
using OpenAI.Chat;
using OpenAI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gpt.Labs.Models.Extensions
{
    public static class OpenAiChatSettingsExtensions
    {
        public static ChatRequest ToChatRequest(this OpenAIChatSettings settings, ICollection<OpenAIMessage> allMessages, string userMessage)
        {
            var messages = new List<Message>
            {
                new Message(Role.System, settings.SystemMessage),
            };

            var messagesToAdd = allMessages.Skip(allMessages.Count - settings.LastNMessagesToInclude).Select(p => p.ToChatRequestMessage());
            messages.AddRange(messagesToAdd);
            messages.Add(new Message(Role.User, userMessage));

            return new ChatRequest(
                messages, 
                new Model(settings.ModelId), 
                settings.Temperature,
                settings.TopP,
                settings.N,
                settings.Stop == null || settings.Stop.Count == 0 ? null : settings.Stop.Select(p => p.Token).ToArray(),
                settings.MaxTokens,
                settings.PresencePenalty,
                settings.FrequencyPenalty,
                settings.LogitBias == null || settings.LogitBias.Count == 0 ? null : settings.LogitBias.ToDictionary(p => p.Token, p => (double)p.Bias),
                settings.User);
        }
    }
}

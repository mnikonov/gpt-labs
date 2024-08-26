using OpenAI;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Linq;

namespace Gpt.Labs.Models.Extensions
{
    public static class OpenAiChatSettingsExtensions
    {
        public static ChatRequest ToChatRequest(this OpenAIChatSettings settings, ICollection<OpenAIMessage> allMessages, OpenAIMessage userMessage)
        {
            var messages = new List<Message>
            {
                new(Role.System, settings.SystemMessage),
            };
            var messagesToAdd = allMessages.Skip(allMessages.Count - settings.LastNMessagesToInclude).Select(p => p.ToChatRequestMessage());
            messages.AddRange(messagesToAdd);
            messages.Add(new Message(Role.User, userMessage.Content));

            return new ChatRequest(
                messages,
                settings.ModelId,
                settings.FrequencyPenalty,
                settings.LogitBias == null || settings.LogitBias.Count == 0 ? null : settings.LogitBias.ToDictionary(p => p.Token, p => (double)p.Bias),
                settings.MaxTokens,
                settings.N,
                settings.PresencePenalty,
                ChatResponseFormat.Text,
                seed: null, // TODO add this paramether to settings
                settings.Stop == null || settings.Stop.Count == 0 ? null : settings.Stop.Select(p => p.Token).ToArray(),
                settings.Temperature,
                settings.TopP,
                topLogProbs: null, // TODO add this paramether to settings 
                parallelToolCalls: null,
                jsonSchema: null,
                settings.User);
        }
    }
}

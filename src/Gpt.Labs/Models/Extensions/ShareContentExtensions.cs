using Gpt.Labs.Helpers.Extensions;
using Gpt.Labs.Models.Enums;

namespace Gpt.Labs.Models.Extensions
{
    public static class ShareContentExtensions
    {
        public static ShareContent ShareChatContent(this OpenAIChat chat, params OpenAIMessage[] messages)
        {
            var share = new ShareContent
            {
                Title = chat.Title,
                Message = messages.Format().ConvertCrLfToLf()
            };

            if (chat.Type == OpenAIChatType.Image)
            {
                foreach (var message in messages)
                {
                    if (message.Role == OpenAIRole.Assistant)
                    {
                        share.Files.Add($"{message.ChatId}\\{message.Id}.png");
                    }
                }
            }

            return share;
        }
    }
}

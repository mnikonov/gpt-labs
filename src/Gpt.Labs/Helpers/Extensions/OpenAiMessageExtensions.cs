using Gpt.Labs.Models;
using System;
using System.Linq;
using System.Text;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class OpenAiMessageExtensions
    {
        public static string Format(this OpenAIMessage[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return string.Empty;
            }

            var assistantMsg = App.ResourceLoader.GetString("Assistant");
            var userMsg = App.ResourceLoader.GetString("User");

            var sb = new StringBuilder();

            foreach (var message in messages.OrderBy(p => p.CreatedDate)) 
            {
                if (messages.Length > 1)
                {
                    switch (message.Role)
                    {
                        case Models.Enums.OpenAIRole.Assistant:
                            sb.Append(assistantMsg);
                            break;
                        case Models.Enums.OpenAIRole.User:
                            sb.Append(userMsg);
                            break;
                    }

                    sb.Append("\n\n");
                }
                
                sb.Append(message.Content);

                if (messages.Length > 1)
                {
                    sb.Append("\n\n");
                }
            }

            return sb.ToString();
        }
    }
}

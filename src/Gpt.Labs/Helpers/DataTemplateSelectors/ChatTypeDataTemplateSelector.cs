using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gpt.Labs.Helpers.DataTemplateSelectors
{
    public class ChatTypeDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ChatDataTemplate { get; set; }

        public DataTemplate ImageDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var chat = item as OpenAIChat;
            
            if (chat != null)
            {
                switch (chat.Type)
                {
                    case OpenAIChatType.Chat:
                        return this.ChatDataTemplate;

                    case OpenAIChatType.Image:
                        return this.ImageDataTemplate;
                }
            }

            return null;
        }
    }
}

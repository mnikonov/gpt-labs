using Gpt.Labs.Models.Base;
using Gpt.Labs.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gpt.Labs.Models
{
    public abstract class OpenAISettings : AuditableEntity
    {
        #region Fields

        private int n = 1;
                
        public string user;

        private OpenAIChat chat;

        private Guid chatId;

        private OpenAIChatType type;

        private string openAIOrganization;

        #endregion

        #region Properties

        public OpenAIChat Chat
        {
            get => this.chat;
            set => this.Set(ref this.chat, value);
        }

        [ForeignKey("Chat")]
        public Guid ChatId
        {
            get => this.chatId;
            set => this.Set(ref this.chatId, value);
        }

        public OpenAIChatType Type
        {
            get => this.type;
            set => this.Set(ref this.type, value);
        }

        /// <summary>
        /// integer Optional Defaults to 1
        /// How many chat completion choices to generate for each input message.
        /// </summary>
        [Range(1, 10, ErrorMessage = "Value for 'Number of completions' must be between {1} and {2}.")]
        public int N
        { 
            get => this.n;
            set => this.Set(ref this.n, value);
        }

        /// <summary>
        /// string Optional
        /// A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.
        /// https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids
        /// </summary>
        [StringLength(250, ErrorMessage = "The field 'User' must be a string with a maximum length of {1}.")]
        public string User
        { 
            get => this.user;
            set => this.Set(ref this.user, value);
        }

        [StringLength(50, ErrorMessage = "The field 'Organization ID' must be a string with a maximum length of {1}.")]
        [RegularExpression(@"^org-.*$",  ErrorMessage = "The field 'Organization ID' must start with 'org-' prefix.")]
        public string OpenAIOrganization
        { 
            get => this.openAIOrganization;
            set => this.Set(ref this.openAIOrganization, value);
        }

        #endregion
    }
}

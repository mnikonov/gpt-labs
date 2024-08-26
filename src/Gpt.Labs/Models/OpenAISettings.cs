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

        private string modelId;

        private int n = 1;

        public string user;

        private OpenAIChat chat;

        private Guid chatId;

        private OpenAIChatType type;

        private string openAIOrganization;

        #endregion

        #region Properties

        /// <summary>
        /// string Required
        /// ID of the model to use. See the model endpoint compatibility table for details on which models work with the API.
        /// https://platform.openai.com/docs/models/model-endpoint-compatibility
        /// </summary>
        [Required(ErrorMessage = "The 'Model' field is required")]
        [StringLength(250, ErrorMessage = "The 'Model' field must be a string with a maximum length of {1}.")]
        public string ModelId
        {
            get => modelId;
            set => Set(ref modelId, value);
        }

        public OpenAIChat Chat
        {
            get => chat;
            set => Set(ref chat, value);
        }

        [ForeignKey("Chat")]
        public Guid ChatId
        {
            get => chatId;
            set => Set(ref chatId, value);
        }

        public OpenAIChatType Type
        {
            get => type;
            set => Set(ref type, value);
        }

        /// <summary>
        /// integer Optional Defaults to 1
        /// How many chat completion choices to generate for each input message.
        /// </summary>
        [Range(1, 10, ErrorMessage = "Value for 'Number of completions' must be between {1} and {2}.")]
        public int N
        {
            get => n;
            set => Set(ref n, value);
        }

        /// <summary>
        /// string Optional
        /// A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.
        /// https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids
        /// </summary>
        [StringLength(250, ErrorMessage = "The field 'User' must be a string with a maximum length of {1}.")]
        public string User
        {
            get => user;
            set => Set(ref user, value);
        }

        [StringLength(50, ErrorMessage = "The field 'Organization ID' must be a string with a maximum length of {1}.")]
        [RegularExpression(@"^org-.*$", ErrorMessage = "The field 'Organization ID' must start with 'org-' prefix.")]
        public string OpenAIOrganization
        {
            get => openAIOrganization;
            set => Set(ref openAIOrganization, value);
        }

        #endregion
    }
}

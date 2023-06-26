using Gpt.Labs.Models.Base;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Gpt.Labs.Models
{
    public partial class OpenAIMessage : AuditableEntity, ISelectable, IHover
    {
        #region Fields

        private string content;

        private OpenAIChat chat;

        private Guid chatId;

        private bool isSelected;

        private bool isHovered;

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

        public OpenAIRole Role { get; set; }

        [Required]
        public string Content
        {
            get => this.content;
            set => this.Set(ref this.content, value);
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.Set(ref this.isSelected, value);
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsHovered 
        {
            get => this.isHovered;
            set => this.Set(ref this.isHovered, value);
        }

        #endregion
    }
}

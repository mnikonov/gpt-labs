using Gpt.Labs.Models.Base;
using Gpt.Labs.Models.Enums;
using Gpt.Labs.Models.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Gpt.Labs.Models
{
    public class OpenAIChat : TitledEntity, ISelectable
    {
        #region Fields

        private OpenAIChatType type;

        private OpenAISettings settings;

        private bool isSelected;

        private int position;

        #endregion

        #region Properties

        public OpenAIChatType Type
        {
            get => this.type;
            set => this.Set(ref this.type, value);
        }

        public OpenAISettings Settings
        {
            get => this.settings;
            set => this.Set(ref this.settings, value);
        }
                
        public int Position
        {
            get => this.position;
            set => this.Set(ref this.position, value);
        }

        [JsonIgnore]
        [NotMapped]
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.Set(ref this.isSelected, value);
        }
                
        [JsonIgnore]
        public ICollection<OpenAIMessage> Messages { get; set; }

        #endregion
    }
}

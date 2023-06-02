using System;
using System.ComponentModel.DataAnnotations;

namespace Gpt.Labs.Models.Base
{
    public abstract class OpenAIToken : AuditableEntity
    {
        #region Fields

        private string token;

        private OpenAISettings settings;

        private Guid settingsId;

        #endregion

        #region Private Fields

        public OpenAISettings Settings
        {
            get => settings;
            set => Set(ref settings, value);
        }

        public Guid SettingsId
        {
            get => settingsId;
            set => Set(ref settingsId, value);
        }

        [Required(ErrorMessage = "The 'Token' field is required")]
        [StringLength(250, ErrorMessage = "The field 'Token' must be a string with a maximum length of {1}.")]
        public string Token
        {
            get => token;
            set => Set(ref token, value);
        }

        #endregion
    }
}

using Gpt.Labs.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gpt.Labs.Models
{
    public class OpenAIApiSettings : ObservableValidationObject
    {
        #region Fields

        private string organization;

        private string apiKey;

        #endregion

        #region Constructors

        public OpenAIApiSettings()
        {

        }

        public OpenAIApiSettings(string organization, string apiKey)
        {
            this.organization = organization;
            this.apiKey = apiKey;
        }

        #endregion

        #region Properties

        [Required(ErrorMessage = "The 'Secret Key' field is required")]
        [JsonPropertyOrder(2)]
        public string ApiKey
        {
            get => this.apiKey;
            set => this.Set(ref this.apiKey, value);
        }

        [StringLength(50, ErrorMessage = "The 'Organization ID' field must be a string with a maximum length of {1}.")]
        [RegularExpression(@"^org-.*$",  ErrorMessage = "The field 'Organization ID' must start with 'org-' prefix.")]
        [JsonPropertyOrder(2)]
        public string Organization
        {
            get => this.organization;
            set => this.Set(ref this.organization, value);
        }

        #endregion
    }
}
